using System.Threading.Tasks;

namespace vPilot_Pushover {

    public class NotifierConfig {
        public string PushoverToken { get; set; }
        public string PushoverUser { get; set; }
        public string PushoverDevice { get; set; }
        public string TelegramBotToken { get; set; }
        public string TelegramChatId { get; set; }
        public string GotifyUrl { get; set; }
        public string GotifyToken { get; set; }
    }

    internal interface INotifier {
        void Initialize(NotifierConfig config);
        Task SendMessageAsync(string message, string title = "", int priority = 0);
        bool HasValidConfig();
    }
}
