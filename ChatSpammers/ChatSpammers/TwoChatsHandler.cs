using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Helpers;

namespace ChatSpammers
{
    public class TwoChatsHandler : IHasFreeMethod
    {
        Task CallForwardingTask;
        bool isTalk1;
        bool isTalk2 ;
        List<ChatMessage> newMsg1 ;
        List<ChatMessage> newMsg2;
        TwoChatsHandlerCallbackArgs callbackArgs = new TwoChatsHandlerCallbackArgs();

        /// <summary>
        /// Создается на основе шаблона BotScenario из ChatSpammerSettings.
        /// </summary>
        public IBotScenario BotScenario { get; private set; }
        public BotController_Conversation ConversationController { get; private set; }
        public string UsedFolder { get; private set; }
        public int CurrentDialogNumber { get; private set; } = 0;
        public Exception LastException{ get; private set;}
        public string UniqueIdentifier{ get; private set;}
        public TwoChatsHandler(ChatSpammerSettings spammerSettings, IChatHandler chat1, IChatHandler chat2)
        {
            Settings = spammerSettings;
            UniqueIdentifier =  RandomTextGenerator.Generate(4);
            string dateStr = string.Format("{0:dd.MM.yy_HH.mm.ss}", DateTime.Now);
            UsedFolder = ResourcesAndConsts.Instance().FolderForСorrespondenceAndLogs + "/" + dateStr + "__" + UniqueIdentifier;
            Directory.CreateDirectory(UsedFolder);
            Chat1 = chat1;
            Chat2 = chat2;
            if (spammerSettings.BotScenario != null)
            {
                BotScenario = spammerSettings.BotScenario.Clone() as IBotScenario;
            }
        }
        public void Free()
        {
            if (IsFree)
                return;

            StopCallForwarding();
            Chat1?.Free();
            Chat1 = null;
            Chat2?.Free();
            Chat2 = null;
            BotScenario?.Free();
            BotScenario = null;
            ConversationController = null;
            IsFree = true;
        }
        public bool IsFree { get; private set; } = false;
        public IChatHandler Chat1
        {
            get;
            private set;
        }
        public IChatHandler Chat2
        {
            get;
            private set;
        }
        /// <summary>
        /// <para>Эта функция запускает поток, который проводит переадресацию между чатами.</para>
        /// <para>Очень важно понимать, что данный метод перехватит все возникающие в этом потоке ошибки и запишет их в LastException, 
        /// включая ошибки из потока браузеров, которые по умолчанию должны были вылитеть в том потоках, а не в данном.
        /// </para>
        /// </summary>
        public void StartCallForwarding()
        {
            Exception lastEx = null;
            if (Status == TwoChatsHandlerStatus.Working)
                lastEx = new Exception("Work has already began!");
            if (Status == TwoChatsHandlerStatus.FatalError)
                lastEx = new Exception("Can`t start after fatal error!");
            if (Status == TwoChatsHandlerStatus.Finishing)
                lastEx = new Exception("Can`t start while finishing work!");
            if(lastEx==null && !(Status == TwoChatsHandlerStatus.Stopped || Status == TwoChatsHandlerStatus.Aborted))
                lastEx = new Exception("Chat must be stopped before starting!");

            if (lastEx != null)
            {
                Status = TwoChatsHandlerStatus.FatalError;
                LastException =lastEx;
#if DEBUG
                throw LastException;
#endif
                return;
            }

            Status = TwoChatsHandlerStatus.Working;
            CallForwardingTask = Task.Run(() =>
            {
                try
                {
                    CurrentDialogNumber++;
                    string folderWithCorrespondence = string.Format("{0}/cor_{1}", UsedFolder, CurrentDialogNumber);
                    string correspondenceFilePath1 = folderWithCorrespondence + "/dialog1.html",
                        correspondenceFilePath2 = folderWithCorrespondence + "/dialog2.html";
                    int msgCount1 = 0, msgCount2 = 0, msgCountWas1 = 0, msgCountWas2 = 0;
                    if (Settings.SaveСorrespondence)
                        Directory.CreateDirectory(folderWithCorrespondence);
                    int loopWithoutConversation = 0;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    long elapsedMilisecondsOnLastUpdate = 0;
                    callbackArgs.ChatHandlerCallbackArgs1.IsRealPerson = Chat1.IsRealPerson;
                    callbackArgs.ChatHandlerCallbackArgs2.IsRealPerson = Chat2.IsRealPerson;

                    //throw new Exception("fok u!");
                    FindCompanions();
                    if (BotScenario != null)
                    {
                        if (BotScenario.IsExecuting)
                            throw new Exception("BotScenario is executing before start.");
                        ConversationController = new BotController_Conversation(callbackArgs);
                        BotScenario.ExecuteAsync(ConversationController);
                    }
                    
                    while (true)
                    {
                        UpdateChatsInfo();

                        //Get new messages
                        msgCountWas1 = msgCount1; msgCountWas2 = msgCount2;
                        newMsg1 = GetNewMessages(Chat1, ref msgCount1);
                        newMsg2 = GetNewMessages(Chat2, ref msgCount2);
                        SendMessages(Chat1, newMsg2);
                        SendMessages(Chat2, newMsg1);

                        //Save correspondence
                        if (Settings.SaveСorrespondence && Directory.Exists(folderWithCorrespondence))
                        {
                            SaveMessages(newMsg1, correspondenceFilePath1,1);
                            SaveMessages(newMsg2, correspondenceFilePath2,2);
                        }

                        //If conversation was finished by chat users.
                        if (!isTalk1 || !isTalk2)
                            loopWithoutConversation++;
                        else
                            loopWithoutConversation = 0;
                        if (loopWithoutConversation > 5)
                            FinishConversation(false);

                        //If not send messages for 60 seconds.
                        if (stopwatch.ElapsedMilliseconds - elapsedMilisecondsOnLastUpdate > 60000)
                        {
                            if (msgCountWas1 == msgCount1 || msgCountWas2 == msgCount2)
                                FinishConversation(false);
                            msgCountWas1 = msgCount1; msgCountWas2 = msgCount2;
                            elapsedMilisecondsOnLastUpdate = stopwatch.ElapsedMilliseconds;
                        }

                        //Check msg limit
                        if(msgCount1>= Settings.MessagesLimit || msgCount2 >= Settings.MessagesLimit)
                            FinishConversation(false);

                        //Check if not finished
                        if (Status != TwoChatsHandlerStatus.Working)
                            break;

                        MakeCallback();
                        Thread.Sleep(50);
                    }

                    if (Settings.SaveСorrespondence && Directory.Exists(folderWithCorrespondence))
                    {
                        int msgCount = (msgCount1 + msgCount2) / 2;
                        HelpFuncs.MoveDirSafety(folderWithCorrespondence, folderWithCorrespondence + "__MsgCount_" + msgCount.ToString());
                    }
                }
                catch (Exception ex)
                { 
                    Status = TwoChatsHandlerStatus.FatalError;
                    LastException = ex;
                }
                finally
                {
                    if (ConversationController!=null)
                        ConversationController.IsConversationFinished = true;
                }
            });
        }
        public void StopCallForwarding()
        {
            Task task = CallForwardingTask;
            if (task == null)
            {
                Status = TwoChatsHandlerStatus.Stopped;
                return;
            }

            Status = TwoChatsHandlerStatus.Finishing;
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                //По идее, этот код не будет вызван никогда.
                SynchronizationHelper.WaitFor(
                    () => { return !(task.IsCanceled || task.IsCompleted || task.IsFaulted); },
                    15000
                );
            }
            else
            {
                task.Wait(15000);
            }
            BotScenario?.FinishExecution(15000);
            if (!(task.IsCanceled || task.IsCompleted || task.IsFaulted))
            {
                task.Dispose();
                Status = TwoChatsHandlerStatus.Aborted;
            }
            else
                Status = TwoChatsHandlerStatus.Stopped;

        }
        public void FindCompanions()
        {
            Task task1 = Task.Run(() =>
              {
                  Chat1.StartConversation(Settings.ChatSettings1);
              });
            Task task2 = Task.Run(() =>
              {
                  Chat2.StartConversation(Settings.ChatSettings2);
              });
            bool finish = false;
            while (!finish && Status == TwoChatsHandlerStatus.Working)
            {
                finish=task1.Wait(200) && task2.Wait(200);
            }
        }
        public void UpdateChatsInfo()
        {
            isTalk1 = Chat1.IsStillTalking();
            isTalk2 = Chat2.IsStillTalking();
        }
        public TwoChatsHandlerStatus Status {  get; private set;}= TwoChatsHandlerStatus.Stopped;
        public ChatSpammerSettings Settings
        {
            get;
            private set;
        }
        List<ChatMessage> GetNewMessages(IChatHandler chat, ref int msgCountWas)
        {
            chat.UpdateMessagesList();
            int msgCount = chat.GetMessagesCount();
            List<ChatMessage> newMsg = new List<ChatMessage>();
            while (msgCountWas < msgCount)
            {
                ChatMessage cm = chat.GetMessageAt(msgCountWas);
                newMsg.Add(cm);
                msgCountWas++;
            }

            return newMsg;
        }
        void SendMessages(IChatHandler chat, List<ChatMessage> newMessagesFromAnotherChat)
        {
            if (newMessagesFromAnotherChat == null)
                return;
            foreach (ChatMessage item in newMessagesFromAnotherChat)
            {
                chat.SendMessage(item);
            }
        }
        void SaveMessages(List<ChatMessage> newMessages, string filePath, int chatNum)
        {
            Settings.OnNewMessages?.Invoke(newMessages, UniqueIdentifier, CurrentDialogNumber, chatNum);
            if (newMessages == null)
                return;
            List<string> msgList = new List<string>();
            foreach (ChatMessage item in newMessages)
            {
                if (item.CanResendMessage)
                    msgList.Add(ConvertMessageToString(item));
            }
            File.AppendAllLines(filePath, msgList);
        }
        string ConvertMessageToString(ChatMessage msg)
        {
            string res = "";
            res += "<p><b>";
            if (msg.IsCompanionsMessage)
                res += "<font color=\"red\">Current user-></font> ";
            else
                res += "<font color=\"blue\">Nekto-></font> ";
            res += "</b>";
            res += string.Format("\" {0} \";\n", msg.Text);
            res += "</p>";
            return res;
        }
        void FinishConversation(bool isCompleted)
        {
            Chat1.FinishConversation();
            Chat2.FinishConversation();
            Status = isCompleted? TwoChatsHandlerStatus.Complete : TwoChatsHandlerStatus.UntimelyFinished;
        }
        void MakeCallback()
        {
            if (BotScenario!=null)
            {
                //Init callbacks args.
                callbackArgs.ChatHandlerCallbackArgs1.NewMessages = newMsg1;
                //callbackArgs.ChatHandlerCallbackArgs1.IsStillTalking = isTalk1;
                //callbackArgs.ChatHandlerCallbackArgs1.IsSearchingCompanion = isSearch1;

                callbackArgs.ChatHandlerCallbackArgs2.NewMessages = newMsg2;
                //callbackArgs.ChatHandlerCallbackArgs2.IsStillTalking = isTalk2;
                //callbackArgs.ChatHandlerCallbackArgs2.IsSearchingCompanion = isSearch2;

                //Scenario
                ConversationController.OnCallback(callbackArgs);

                //Handle result of callback args.
                if (callbackArgs.IsCompleted != null)
                {
                    FinishConversation((bool)callbackArgs.IsCompleted);
                }
                SendMessages(Chat1, callbackArgs.ChatHandlerCallbackArgs1.MessagesToSend);
                SendMessages(Chat2, callbackArgs.ChatHandlerCallbackArgs2.MessagesToSend);
            }
        }
    }
}
