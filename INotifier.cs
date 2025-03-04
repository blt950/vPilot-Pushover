using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vPilot_Pushover {

    public class NotifierConfig {
        public string settingPushoverToken { get; set; }
        public string settingPushoverUser { get; set; }
        public string settingPushoverDevice { get; set; }
        public string settingTelegramBotToken { get; set; }
        public string settingTelegramChatId { get; set; }
        public string settingGotifyUrl { get; set; }
        public string settingGotifyToken { get; set; }
    }

    internal interface INotifier {
        void init( NotifierConfig config );
        void sendMessage( String message, String title = "", int priority = 0 );
        Boolean hasValidConfig();
    }
}
