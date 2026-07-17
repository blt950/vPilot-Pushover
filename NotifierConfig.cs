using System;
namespace vPilot_Pushover {
    public class NotifierConfig {
        public string PushoverToken { get; set; }
        public string PushoverUser { get; set; }
        public string PushoverDevice { get; set; }
        public string PushoverHighPriRetries { get; set; }
        public string PushoverHighPriExpire { get; set; }
        public string TelegramBotToken { get; set; }
        public string TelegramChatId { get; set; }
        public string GotifyUrl { get; set; }
        public string GotifyToken { get; set; }
        public string NtfyUrl { get; set; }
        public string NtfyToken { get; set; }
        public Action<string> OnError { get; set; }
    }
}