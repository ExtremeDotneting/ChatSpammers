using System;
using System.Collections.Generic;

namespace ChatSpammers
{
    public class ChatSpammerSettings:ICloneable
    {
        public bool SaveСorrespondence { get; set; } = true;
        public ChatHandlerSpecialSettings ChatSettings1 { get; set; }
        public ChatHandlerSpecialSettings ChatSettings2 { get; set; }
        public OnNewMessagesDelegate OnNewMessages { get; set; }
        public bool UseProxy { get; set; } = false;
        public bool UseFolderCache { get; private set; } = false;
        public IBotScenario BotScenario { get; set; }
        public int MessagesLimit { get; set; } = 300;

        public ChatSpammerSettings() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BotScenario">Может быть равно null.</param>
        /// <param name="ChatSettings1">Может быть равно null.</param>
        /// <param name="ChatSettings2">Может быть равно null.</param>
        public ChatSpammerSettings(bool SaveСorrespondence,ChatHandlerSpecialSettings ChatSettings1,
            ChatHandlerSpecialSettings ChatSettings2,bool UseProxy,bool UseFolderCache ,IBotScenario BotScenario, int MessagesLimit)
        {
            this.SaveСorrespondence = SaveСorrespondence;
            this.ChatSettings1 = ChatSettings1;
            this.ChatSettings2 = ChatSettings2;
            this.UseProxy = UseProxy;
            this.UseFolderCache = UseFolderCache;
            this.BotScenario = BotScenario;
            this.MessagesLimit = MessagesLimit;
        }
        public object Clone()
        {
            return new ChatSpammerSettings(
                SaveСorrespondence,
                ChatSettings1,
                ChatSettings2,
                UseProxy,
                UseFolderCache,
                BotScenario,
                MessagesLimit
                );
        }
        
       
    }

    /// <summary>
    /// </summary>
    /// <param name="chatNum">Может быть 1 или 2.</param>
    public delegate void OnNewMessagesDelegate(List<ChatMessage> newMessages, string dialogHandlerIdentifier, int dialogNum,int chatNum);

}
