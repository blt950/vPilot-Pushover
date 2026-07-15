using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {

    // Shared plumbing for the HTTP-based notifiers: form-posting and error
    // reporting through NotifierConfig.OnError, so no driver can fail silently.
    internal abstract class HttpNotifierBase : INotifier {

        private Action<string> _onError;

        public void Initialize(NotifierConfig config) {
            _onError = config.OnError;
            Configure(config);
        }

        public abstract bool HasValidConfig();
        public abstract Task SendMessageAsync(string text, string title = "", int priority = 0, string source = "notification");

        // Reads the driver-specific settings out of the shared config.
        protected abstract void Configure(NotifierConfig config);

        // Turns a raw HTTP error body into a readable cause. Default: the body as-is.
        protected virtual string ExtractErrorDetail(string body) {
            return body;
        }

        protected async Task PostFormAsync(string url, Dictionary<string, string> values, string source) {
            try {
                using (var content = new FormUrlEncodedContent(values)) {
                    var response = await Http.Client.PostAsync(url, content);
                    var body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode) {
                        _onError?.Invoke($"{GetType().Name} rejected the {source} (HTTP {(int)response.StatusCode}): {ExtractErrorDetail(body)}");
                    }
                }
            } catch (Exception ex) {
                _onError?.Invoke($"{GetType().Name} failed sending the {source}: {ex.Message}");
            }
        }

    }
}
