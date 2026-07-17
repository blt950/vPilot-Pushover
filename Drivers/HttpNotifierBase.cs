using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace vPilot_Pushover.Drivers {
    internal abstract class HttpNotifierBase : INotifier {
        private Action<string> _onError;
        public void Initialize(NotifierConfig config) {
            _onError = config.OnError;
            Configure(config);
        }
        public abstract bool HasValidConfig();
        public abstract Task SendMessageAsync(string text, string title = "", int priority = 0, string source = "notification");
        protected abstract void Configure(NotifierConfig config);
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
        // Posts a raw text body (not form-encoded). configureRequest lets the
        // caller add headers (e.g. Authorization) before the request is sent.
        protected async Task PostRawAsync(string url, string body, string source, Action<HttpRequestMessage> configureRequest = null) {
            try {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url)) {
                    request.Content = new StringContent(body ?? "", Encoding.UTF8);
                    configureRequest?.Invoke(request);
                    var response = await Http.Client.SendAsync(request);
                    var respBody = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode) {
                        _onError?.Invoke($"{GetType().Name} rejected the {source} (HTTP {(int)response.StatusCode}): {ExtractErrorDetail(respBody)}");
                    }
                }
            } catch (Exception ex) {
                _onError?.Invoke($"{GetType().Name} failed sending the {source}: {ex.Message}");
            }
        }
    }
}