using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;

namespace vPilot_Pushover
{
    public class vPilotPushover : IPlugin
    {
        private IBroker _broker;
        public string Name => "vPilot Pushover Plugin";
        private static readonly HttpClient client = new HttpClient();


        public void Initialize(IBroker broker)
        {
            // Your plugin initialization code here
            _broker = broker;

            // Send a debug message when connected
            SendPushover("Connected to vPilot");

            // Subscribe to NotConnectedException
            _broker.ControllerAdded += OnControllerAddedHandler;

        }

        private async void SendPushover(String text)
        {

            var values = new Dictionary<string, string>
            {
                { "token", "aoicqcihrzne4xswpwbud4n885f5tt" },
                { "user", "u658a3in92esewftpuyqhyaej7hew6" },
                { "message", text }
            };

            var response = await client.PostAsync("https://api.pushover.net/1/messages.json", new FormUrlEncodedContent(values));

            var responseString = await response.Content.ReadAsStringAsync();
            _broker.PostDebugMessage(responseString);

        }

        private void OnControllerAddedHandler(object sender, ControllerAddedEventArgs e)
        {
            // Access the Callsign property from the ControllerAddedEventArgs
            string controllerCallsign = e.Callsign;

            // You can now use the controllerCallsign as needed
            // For example, you can post a debug message or perform other actions
            _broker.PostDebugMessage($"ControllerAdded - Callsign: {controllerCallsign}");
        }

    }
}