using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Helpers;

namespace ChatSpammers
{
    public class DispatcherOfTwoChatsHandler : IHasFreeMethod
    {
        class TwoChatsHandlerAndAnother
        {
            CommandToDoWithChat doOnNextLoop = CommandToDoWithChat.None;
            public TwoChatsHandlerAndAnother() { }
            public TwoChatsHandlerAndAnother(TwoChatsHandler twoChatsHandler)
            {
                TwoChatsHandler = twoChatsHandler;
            }
            public TwoChatsHandler TwoChatsHandler;
            public int MediumErrorsAtStretch = 0;
            public bool IsCommandsConfirmed = true;
            public CommandToDoWithChat DoOnNextLoop
            {
                get { return doOnNextLoop; }
                set
                {
                    if (IsCommandsConfirmed)
                        doOnNextLoop = value;
                }
            }
        }
        enum CommandToDoWithChat { Remove, Start,None}

        List<TwoChatsHandlerAndAnother> allDialogsList = new List<TwoChatsHandlerAndAnother>();
        Task mainWorkTask;
        bool carefullyStop = false;
        bool abortWork = false;

        public bool IsWorking { get; private set; } = false;
        public int RealWorkUnitsCount { get { return allDialogsList.Count; } }
        public int NeededWorkUnitsCount { get; set; } = 1;
        public ChatSpammerSettings ChatsSettings { get; private set; }
        public IFactoryOfChatHandler FactoryOfChatHandler { get; private set; }
        public bool SaveLog { get; set; } = true;

        public DispatcherOfTwoChatsHandler(ChatSpammerSettings chatsSettings, IFactoryOfChatHandler factoryOfChatHandler)
        {
            ChatsSettings = chatsSettings.Clone() as ChatSpammerSettings;
            FactoryOfChatHandler = factoryOfChatHandler;
        }
        public void StartWork()
        {
            if (IsFree)
                throw new Exception("Work can`t be started, because current object is destroyed.");
            if (IsWorking)
                throw new Exception("Work has already started.");

            mainWorkTask=Task.Run(new Action(WorkingLoopThreadFunction));

        }
        /// <summary>
        /// Whait for all dialogs finish and don`t start new.
        /// </summary>
        public void СarefullyStopWork()
        {
            carefullyStop = true;
            SynchronizationHelper.WaitFor(
                    () => { return RealWorkUnitsCount > 0; },
                    25000
                );
            AbortWork();
        } 
        /// <summary>
        /// Abort all dialogs.
        /// </summary>
        public void AbortWork()
        {
            Task task = mainWorkTask;
            if (task == null)
                return;

            carefullyStop = true;
            abortWork = true;

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                SynchronizationHelper.WaitFor(
                    () => { return !(task.IsCanceled || task.IsCompleted || task.IsFaulted); },
                    25000
                );
            }
            else
            {
                task.Wait(25000);
            }
        }
        public bool IsFree { get; private set; } = false;
        public void Free()
        {
            if (IsFree)
                return;
            AbortWork();
            IsFree = true;
        }

        void WorkingLoopThreadFunction()
        {
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            IsWorking = true;
            carefullyStop = false;
            abortWork = false;
            while (true)
            {
                if (abortWork)
                    break;

                int realWorkUnitsCount = RealWorkUnitsCount;
                while (realWorkUnitsCount < NeededWorkUnitsCount && !carefullyStop)
                {
                    realWorkUnitsCount++;
                    var newItem = AddNewDialogHandler();
                    SaveToLog(newItem, "Try to start new thread dialog.", true);
                }
                while (realWorkUnitsCount > NeededWorkUnitsCount)
                {
                    realWorkUnitsCount--;
                    SaveToLog(allDialogsList[0], "Number of dialogs is crowded. This dialog will be removed.", true);
                    allDialogsList[0].DoOnNextLoop = CommandToDoWithChat.Remove;
                }
                CheckAllDialogHandlers();

                //Раньше иногда возникали проблемы с состоянием чатов.
                //Но отныне и навсегда чаты запускаются и удаляются ТОЛЬКО ЗДЕСЬ.
                foreach (var item in allDialogsList.ToArray())
                {
                    if (item == null || item.DoOnNextLoop == CommandToDoWithChat.None || item.TwoChatsHandler.IsFree)
                        continue;

                    //Этот код дает полную возможность асинхронно работать с большим количество.
                    var currentCmd = item.DoOnNextLoop;
                    item.DoOnNextLoop = CommandToDoWithChat.None;
                    item.IsCommandsConfirmed = false;
                    Task.Run(() =>
                    {
                        try
                        {
                            if (currentCmd == CommandToDoWithChat.Start)
                            {
                                item.TwoChatsHandler.StopCallForwarding();
                                item.TwoChatsHandler.StartCallForwarding();
                            }
                            else if (currentCmd == CommandToDoWithChat.Remove)
                            {
                                RemoveDialogHandler(item);
                            }
                        }
                        catch
                        {
#if DEBUG
                            throw;
#endif
                        }
                        item.IsCommandsConfirmed = true;  
                    });
                }

                Thread.Sleep(50);
            }

            foreach (var item in allDialogsList.ToArray())
            {
                RemoveDialogHandler(item);
            }
            allDialogsList.Clear();
            IsWorking = false;
        }
        void CheckAllDialogHandlers()
        {
            //Если коротко, то суть в том, что если фатальная ошибка или несколько обычных подряд - завершаем TwoChatsHandler.
            //Если несерьезная ошибка - перезапускаем.

            foreach(var item in allDialogsList.ToArray())
            {
                if (item == null || !item.IsCommandsConfirmed || item.TwoChatsHandler.IsFree)
                    continue;

                if (item.TwoChatsHandler.IsFree) //-V3022
                {
                    SaveToLog(item, "Chats handler class was disposed somewhere.", true);
                    item.DoOnNextLoop = CommandToDoWithChat.Remove;
                }
                else if (item.MediumErrorsAtStretch > 5)
                {
                    SaveToLog(item, "Medium error for too much times at stretch.", true);
                    item.DoOnNextLoop = CommandToDoWithChat.Remove;
                    //medium exception for some times at a stretch
                }
                else if(item.TwoChatsHandler.Status == TwoChatsHandlerStatus.FatalError)
                {
                    SaveToLog(item, "Fatal error in TwoChatsHandler.", true);
                    item.DoOnNextLoop = CommandToDoWithChat.Remove;
                    //fatal exception
                }
                //when not dangerous situation
                else if (item.TwoChatsHandler.Status == TwoChatsHandlerStatus.Working)
                {
                    item.MediumErrorsAtStretch = 0;
                }
                else if (item.TwoChatsHandler.Status == TwoChatsHandlerStatus.Complete)
                {
                    item.MediumErrorsAtStretch++;
                    SaveToLog(item, "Work is done. Will try to start new dialog.", false);
                    OnAfterFinishConversation(item);
                    //medium exception
                }
                else if (item.TwoChatsHandler.Status == TwoChatsHandlerStatus.UntimelyFinished)
                {
                    item.MediumErrorsAtStretch++;
                    SaveToLog(item, "Untimely finish. Will try to start new dialog.", false);
                    OnAfterFinishConversation(item);
                    //medium exception
                }
                else if (item.TwoChatsHandler.Status == TwoChatsHandlerStatus.Aborted)
                {
                    item.MediumErrorsAtStretch++;
                    SaveToLog(item, "Strange error, i don`t now why it can be. Will try to start new dialog.", true);
                    OnAfterFinishConversation(item);
                    //medium exception
                }
            }
        }
        void RemoveDialogHandler(TwoChatsHandlerAndAnother dialogAndAnother)
        {
            allDialogsList.Remove(dialogAndAnother);
            FactoryOfChatHandler.OnChatsDisposed(
                dialogAndAnother.TwoChatsHandler.Chat1,
                dialogAndAnother.TwoChatsHandler.Chat2,
                dialogAndAnother.TwoChatsHandler.Status
                );

            dialogAndAnother.TwoChatsHandler.StopCallForwarding();
            dialogAndAnother.TwoChatsHandler.Free();
        }
        TwoChatsHandlerAndAnother AddNewDialogHandler()
        {
            TwoChatsHandler dialog = new TwoChatsHandler(
                ChatsSettings,
                FactoryOfChatHandler.CreateChat1(ChatsSettings),
                FactoryOfChatHandler.CreateChat2(ChatsSettings)
                );
            /////////////////////////
            TwoChatsHandlerAndAnother newItem = new TwoChatsHandlerAndAnother(dialog);
            allDialogsList.Add(newItem);
            newItem.DoOnNextLoop = CommandToDoWithChat.Start;
            return newItem;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="longFormat">Если истина - запишет в лог всю доступную информацию о диалоге.</param>
        void SaveToLog(TwoChatsHandlerAndAnother dialogAndAnother, string message, bool longFormat)
        {
            if (!SaveLog)
                return;

            string logFile = string.Format("{0}/work_story.log",dialogAndAnother.TwoChatsHandler.UsedFolder);
            string logText = "";
            logText += string.Format("DateTime: {0}", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            logText += string.Format("=n=Dialog number: {0}", dialogAndAnother.TwoChatsHandler.CurrentDialogNumber.ToString());
            logText += string.Format("=n=Message: \" {0} \"", message);
            if (longFormat)
            {
                if (dialogAndAnother.TwoChatsHandler.BotScenario != null && dialogAndAnother.TwoChatsHandler.BotScenario.LastException!=null)
                {
                    logText += string.Format("=n=BotScenario exception: \" {0} \"", dialogAndAnother.TwoChatsHandler.BotScenario.LastException.Message);
                }
                string exStr = dialogAndAnother.TwoChatsHandler?.LastException?.Message ?? "No exception!";
                var ex = dialogAndAnother.TwoChatsHandler.LastException;
                logText += string.Format("=n=Exception: \" {0} \"", exStr);
                logText += string.Format("=n=TwoChatsHandler status: {0}", dialogAndAnother.TwoChatsHandler.Status.ToString());
            }
            logText = logText.Replace("\n", "\n      ").Replace("=n=","\n");
            logText += string.Format("\n-------\n\n");
            File.AppendAllLines(logFile, logText.Split('\n'));
        }
        void OnAfterFinishConversation(TwoChatsHandlerAndAnother dialogAndAnother)
        {
            if (carefullyStop)
                dialogAndAnother.DoOnNextLoop = CommandToDoWithChat.Remove;
            else
            {
                dialogAndAnother.DoOnNextLoop = CommandToDoWithChat.Start;
            }
                
        }


    }
}
