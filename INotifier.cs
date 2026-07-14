using System.Threading.Tasks;

namespace vPilot_Pushover {

    internal interface INotifier {
        void Initialize(NotifierConfig config);
        Task SendMessageAsync(string message, string title = "", int priority = 0);
        bool HasValidConfig();
    }
}
