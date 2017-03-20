using System;
using System.Collections.Generic;
using CustomWebBrowsers;
using System.Threading;
using Helpers;

namespace ChatSpammers
{
    public class ChatvdvoemBasicWorker : IHasFreeMethod
    {
        string JsFor_InitCheckIfStillTalking = "function checkFunc2(){ var elem=$('.controlwrapper').css('display'); if(elem==='none') return 0;" +
            "if(elem==='block') return 1; return 'error '+ elem; }; function checkFunc1() { var elem =$('.disconnected').length; if (elem === 1) return 0;" +
            "if (elem === 0) return 1; return 'error ' + elem; };" +
            //"(function (){try { var checkRes1 = checkFunc1(); var checkRes2 = checkFunc2(); if (checkRes1 === 1 && checkRes2 === 1) return 1;"+
            //" if (checkRes1 === 0 || checkRes2 === 0) return 0; return 'res: '+checkRes1+' _ '+checkRes2;"+
            "function CheckIfStillTalking(){try {if (checkFunc1() === 1 && checkFunc2() === 1) return 1; return 0;" +
            "}catch(ex){return 'error '+ex;}};";
        string JsFor_CheckIfStillTalking = "CheckIfStillTalking();";
        string JsFor_InitGetCountOfMessages = "function GetCountOfMessages(){ try { var elem=$('.message'); return elem.length+0; } catch(ex){ return 0; }};";
        string JsFor_GetCountOfMessages = "GetCountOfMessages();";
        string chatStartUrl = "https://chatvdvoem.ru/";
        string JsFor_ClickSearchButtononStartUrl = "document.getElementById('chat_start').click();";
        string JsFor_FinishConversation = "document.getElementById('chat_close').click();";
        string JsFor_CheckIfSearchingCompanion = null;
        string JsFor_InitClickSearchNewCompanionButton = "function ClickSearchNewCompanionButton(){$(\"a[onclick = 'javascript:$('#chat_start').click();"+
            "return false;']\")[0].click();}";
        string JsFor_ClickSearchNewCompanionButton = "ClickSearchNewCompanionButton();";
        string JsFor_ClickSendMessageButton = "$('#text_send')[0].click(); $('#text_send')[0].click();";
        string JsFor_SimulateWriting = "$('textarea#text').sendkeys('s');";
        string JsFor_ClearTextArea = "$('textarea#text').sendkeys('{selectall}{del}');";
        string JsFor_InitCheckIfCompanionsMessageAt = "function CheckIfCompanionsMessageAt(msgNum){ try { var elem=$('.message').parent()[msgNum].className; "+
            "if(elem==='messageFrom') return 1; if(elem==='messageTo') return 0; } catch(ex){ return 'error'; } return 0;}";
        string JsFor_InitCheckCaptcha = "function CheckCaptcha(){ try{if(CheckIfStillTalking() && !($('#chvd-captcha').html()==='')) return 1; }"+
            "catch(ex){return ex;}return 0;}";
        string JsFor_CheckCaptcha = "CheckCaptcha();";
        string JsFor_SetIsBotFuncsInitialized = "var IsBotFuncsInitialized=1;";
        string JsFor_GetIsBotFuncsInitialized = "(function(){try{ if(IsBotFuncsInitialized===1) return 1;}catch(ex){} return 0;})();";

        public ICustomBrowser CustomBrowser { get; private set; }
        public ChatvdvoemBasicWorker(ICustomBrowser browser)
        {
            CustomBrowser = browser;
        }
        public void Free()
        {
            if (IsFree)
                return;

            CustomBrowser?.Free();
            CustomBrowser = null; 
            IsFree = true;
        }
        public bool IsFree
        {
            get;
            private set;
        } = false;
        public void WriteToBrowserLog(string text)
        {
            CustomBrowser.WriteToLog(text);
        }

        /// <summary>
        /// Не используется и, возможно, никогда не будет.
        /// </summary>
        public void SimulateWriting(int timeMS)
        {
            int waiInOneLoop = 200;
            while (timeMS > 0)
            {
                CustomBrowser.ExJs(JsFor_SimulateWriting);
                SynchronizationHelper.Pause(waiInOneLoop);
                timeMS -= waiInOneLoop;
            }
            CustomBrowser.ExJs(JsFor_ClearTextArea);
            SynchronizationHelper.Pause(50);
        }
        public void FindCompanion()
        {
            FinishConversation();
            FindCompanion(5);
        }
        void FindCompanion( int countOfTryes)
        {
            if (countOfTryes <= 0)
                throw new Exception("Out of tryes to find companion.");
            chatMessagesList = new List<ChatMessage>();

            /*companionSearchSettings != null && previousCompanionSearchSettings == companionSearchSettings &&*/
            if ( RestartConversationIfCan())
                return;

            CustomBrowser.LoadPage(chatStartUrl);
            InitJs();
            try
            {
                GetWithUpdate_IsStillTalking();
            }
            catch
            {
                throw new Exception("Page load exception!");
            }
            
            //SynchronizationHelper.Pause(500);
            //string scriptToSetSettings = JsFor_SetSearchSettings(companionSearchSettings);
            //CustomBrowser.ExJs(scriptToSetSettings);
            SynchronizationHelper.Pause(200);
            CustomBrowser.ExJs(JsFor_ClickSearchButtononStartUrl);
            InitJs();

            //SynchronizationHelper.WaitFor(
            //    GetWithUpdate_IsSearchingCompanion, 20000);
            SynchronizationHelper.WaitFor(
                () => {
                    if (GetWithUpdate_IsStillTalking())
                        return true;
                    SynchronizationHelper.Pause(250);
                    return false;
                }, 
                10000);

            //previousCompanionSearchSettings = companionSearchSettings;
            if (!GetWithUpdate_IsStillTalking()) {
                FindCompanion(countOfTryes - 1);
            }
            CheckCaptchaAndWait();
        }

        public void FinishConversation()
        {
            InitJs();
            CustomBrowser.ExJs(JsFor_FinishConversation);
            SynchronizationHelper.Pause(300);
        }
        public void SendMessage(string text)
        {
            InitJs();
            CustomBrowser.ExJs(JsFor_ClearTextArea);
            CustomBrowser.ExJs(JsFor_SetMessageBoxText(text));
            CustomBrowser.ExJs(JsFor_ClickSendMessageButton);

        }
        public void SendMessage(ChatMessage msg)
        {
            if (msg.IsCompanionsMessage)
                SendMessage(msg.Text);
        }

        List<ChatMessage> chatMessagesList = new List<ChatMessage>();
        public int GetSaved_MessagesCount()
        {
            return chatMessagesList.Count;
        }
        public ChatMessage GetSaved_MessageAt(int msgNum)
        {
            return (ChatMessage)chatMessagesList[msgNum];
        }

        /// <summary>
        /// Не рекомендуется использовать извне.
        /// </summary>
        public ChatMessage ReadMessageAt(int msgNum)
        {
            InitJs();
            string jsResText = CustomBrowser.ExJsWithResult(
                JsFor_GetMessageAt(msgNum)
                ).Trim();
            if(string.IsNullOrWhiteSpace(jsResText) || CustomBrowser.CheckJsResult_IsUndefined(jsResText) || jsResText == "msgError")
                ThrowInvalidJsResultExeption("JsFor_GetMessageAt", jsResText);

            string jsResBool = CustomBrowser.ExJsWithResult(
                JsFor_CheckIfCompanionsMessageAt(msgNum)
                ).Trim();
            if (!CustomBrowser.CheckJsResult_WhiteList(jsResBool, new string[] { "1", "0" }))
            {
                ThrowInvalidJsResultExeption("JsFor_CheckIfCompanionsMessageAt", jsResBool);
            }
            jsResText = jsResText.Replace("<br>","\n");

            ChatMessage res= new ChatMessage(
                jsResText,
                jsResBool == "1"
                );

            return res;
        }

        /// <summary>
        /// Не рекомендуется использовать извне.
        /// </summary>
        public int ReadMessagesCount()
        {
            InitJs();
            int? jsRes = null;
            try
            {
                jsRes = Convert.ToInt32(CustomBrowser.ExJsWithResult(
                    JsFor_GetCountOfMessages
                    ));
                return (int)jsRes;
            }
            catch
            {
                ThrowInvalidJsResultExeption("JsFor_GetCountOfMessages", "null");
                return -9999;
            }
        }
        public void Update_MessagesList()
        {
            int realMsgCount = ReadMessagesCount();
            int readedMsgCount = GetSaved_MessagesCount();
            for (int i = readedMsgCount; i < realMsgCount; i++)
            {
                chatMessagesList.Add(ReadMessageAt(i));
            }
        }
        
        bool isSearchingCompanion = false;
        public bool GetWithUpdate_IsSearchingCompanion()
        {
            isSearchingCompanion = false;
            return isSearchingCompanion;

            string jsRes = CustomBrowser.ExJsWithResult(JsFor_CheckIfSearchingCompanion).Trim();
            if (!CustomBrowser.CheckJsResult_WhiteList(jsRes, new string[] { "1", "0" }))
            {
                ThrowInvalidJsResultExeption("JsFor_CheckIfSearchingCompanion", jsRes);
            }

            isSearchingCompanion= jsRes == "1";
            return isSearchingCompanion;
        }
        public bool GetSaved_IsSearchingCompanion()
        {
            return isSearchingCompanion;
        }

        bool isStillTalking = false;
        public bool GetWithUpdate_IsStillTalking()
        {
            InitJs();
            string jsRes = Convert.ToString(CustomBrowser.ExJsWithResult(JsFor_CheckIfStillTalking)).Trim();
            if (!CustomBrowser.CheckJsResult_WhiteList(jsRes, new string[] { "1", "0" }))
            {
                ThrowInvalidJsResultExeption("JsFor_CheckIfStillTalking", jsRes);
            }

            isStillTalking = jsRes == "1";
            return isStillTalking;
        }
        public bool GetSaved_IsStillTalking()
        {
            return isStillTalking;
        }

        public bool GetWithUpdate_CheckCaptcha()
        {
            InitJs();
            string jsRes = Convert.ToString(CustomBrowser.ExJsWithResult(JsFor_CheckCaptcha)).Trim();
            if (!CustomBrowser.CheckJsResult_WhiteList(jsRes, new string[] { "1", "0" }))
            {
                ThrowInvalidJsResultExeption("JsFor_CheckCaptcha", jsRes);
            }
            return jsRes == "1";
        }
        public void CheckCaptchaAndWait()
        {
            int loopNum = 0;
            while (!IsFree && GetWithUpdate_CheckCaptcha() && loopNum++<40)
            {
                TrayHelper.ShowTextInTray("Captcha", "You must enter captcha in chatvdvoem window!");
                SynchronizationHelper.Pause(7000);
            }
            if (loopNum >= 40)
                throw new Exception("Captcha wasn`t entered!");
        }

        public bool GetWithUpdate_IsBotFuncsInitialized()
        {
            string jsRes = Convert.ToString(CustomBrowser.ExJsWithResult(JsFor_GetIsBotFuncsInitialized)).Trim();
            if (!CustomBrowser.CheckJsResult_WhiteList(jsRes, new string[] { "1", "0" }))
            {
                ThrowInvalidJsResultExeption("JsFor_GetIsBotFuncsInitialized", jsRes);
            }
            return jsRes == "1";
        }

        void BlockThreadToDebug()
        {
            int msgCount = 0;
            AwesomiumCustomBrowser custBr = CustomBrowser as AwesomiumCustomBrowser;
            int loopNum = 0;
            bool isTalk = false;
            while (true)
            {
                Thread.Sleep(2000);
                bool isTalkNew = GetWithUpdate_IsStillTalking();
                if (isTalkNew != isTalk)
                    custBr.WriteToLog(">    is talking: " + isTalkNew);
                if (isTalk)
                {
                    for (int i = msgCount; i < GetSaved_MessagesCount(); i++)
                    {
                        ChatMessage chMsg = GetSaved_MessageAt(i);
                        custBr.WriteToLog(string.Format(">{0} :: {1} ;", chMsg.IsCompanionsMessage ? "Stranger" : "You", chMsg.Text));
                        msgCount++;
                    }
                    Update_MessagesList();
                }
                else
                    msgCount = 0;
                isTalk = isTalkNew;
                //custBr.ExJs(JsFor_SetMessageBoxText(loopNum.ToString()));
                SendMessage(loopNum.ToString());
                loopNum++;

            }
        }

        /// <summary>
        /// Return true if conversation restarted.
        /// </summary>
        /// <returns></returns>
        bool RestartConversationIfCan()
        {
            bool res = false;
            try
            {
                CustomBrowser.ExJs(JsFor_ClickSearchNewCompanionButton);
                SynchronizationHelper.Pause(200);

                SynchronizationHelper.WaitFor(
                    GetWithUpdate_IsSearchingCompanion, 20000);
                SynchronizationHelper.WaitFor(
                    () => { return !GetWithUpdate_IsStillTalking(); }, 15000);
                res = GetWithUpdate_IsStillTalking();
            }
            catch { }
            return res;
        }
        void ThrowInvalidJsResultExeption(string jsStringName, string jsResult)
        {
            throw new Exception(string.Format("Invalid result from '{0}'.\nResult: {1}", jsStringName, jsResult));
        }
        void InitJs()
        {
            if (GetWithUpdate_IsBotFuncsInitialized())
                return;           
            CustomBrowser.ExJs(ResourcesAndConsts.Instance().JsLib_JqueryKeypressSimulator);
            CustomBrowser.ExJs(JsFor_InitCheckIfStillTalking);
            CustomBrowser.ExJs(JsFor_InitGetCountOfMessages);
            CustomBrowser.ExJs(JsFor_InitCheckIfCompanionsMessageAt);
            CustomBrowser.ExJs(JsFor_InitCheckCaptcha);
            CustomBrowser.ExJs(JsFor_InitClickSearchNewCompanionButton);
            CustomBrowser.ExJs(JsFor_SetIsBotFuncsInitialized);
        }

        string JsFor_SetMessageBoxText(string text)
        {
            string res = "";
            //Используется для отправки стикеров.
            string jsScriptSwitch = "javascript::";
            if (text.StartsWith(jsScriptSwitch) )
            {
                res = text.Remove(0, jsScriptSwitch.Length);
            }
            else
            {
                text = text.Replace("\n", "{newline}").Replace("\t", "{tab}").Replace(@"\", @"\\").Replace("\"", "\\\"");
                res = string.Format("$('textarea#text').sendkeys(\"{0}\")", text);
            }
            return res;
        }
        string JsFor_GetMessageAt(int msgNum)
        {
            return string.Format("$('.message')[{0}].innerHTML;", msgNum) ;
        }
        string JsFor_CheckIfCompanionsMessageAt(int msgNum)
        {
            string formatStr = string.Format("CheckIfCompanionsMessageAt({0});", msgNum);
            return formatStr;
        }
        string JsFor_SetSearchSettings(CompanionSearchSettings companionSearchSettings)
        {
            return "";
        }
        
    }
}
