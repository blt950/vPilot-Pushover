using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace vPilot_Pushover.Drivers
{
    internal class Mqtt : INotifier
    {
        private string _host;
        private int _port;
        private string _topic;
        private string _username;
        private string _password;
        private bool _useTls;
        private Action<string> _onError;

        public void Initialize(NotifierConfig config)
        {
            _host = config.MqttHost;
            _topic = config.MqttTopic;
            _username = config.MqttUsername;
            _password = config.MqttPassword;
            _useTls = bool.TryParse(config.MqttUseTls, out bool useTls) && useTls;
            if (!int.TryParse(config.MqttPort, out _port))
            {
                _port = _useTls ? 8883 : 1883;
            }

            _onError = config.OnError;
        }

        public bool HasValidConfig()
        {
            return !string.IsNullOrWhiteSpace(_host)
                && !string.IsNullOrWhiteSpace(_topic)
                && _port > 0
                && _port <= 65535
                && ((_username == null && _password == null)
                    || (!string.IsNullOrWhiteSpace(_username) && _password != null));
        }

        public async Task SendMessageAsync(string text, string title = "", int priority = 0, string source = "notification")
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(_host, _port);
                    using (var networkStream = client.GetStream())
                    {
                        Stream stream = networkStream;
                        if (_useTls)
                        {
                            var sslStream = new SslStream(networkStream, false);
                            await sslStream.AuthenticateAsClientAsync(_host, null, SslProtocols.Tls12, false);
                            stream = sslStream;
                        }

                        await WritePacketAsync(stream, CreateConnectPacket("vPilot-Pushover-" + Guid.NewGuid().ToString("N")));
                        byte[] connAck = await ReadBytesAsync(stream, 4);
                        if (connAck[0] != 0x20 || connAck[1] != 0x02 || connAck[3] != 0)
                        {
                            throw new InvalidOperationException($"MQTT broker rejected the connection (return code {connAck[3]}).");
                        }

                        string payload = new JavaScriptSerializer().Serialize(new
                        {
                            title,
                            message = text,
                            priority,
                            source
                        });
                        await WritePacketAsync(stream, CreatePublishPacket(payload));
                        await WritePacketAsync(stream, new byte[] { 0xE0, 0x00 });
                    }
                }
            }
            catch (Exception ex)
            {
                _onError?.Invoke($"{GetType().Name} failed sending the {source}: {ex.GetBaseException().Message}");
            }
        }

        private byte[] CreateConnectPacket(string clientId)
        {
            using (var payload = new MemoryStream())
            {
                WriteString(payload, clientId);
                if (_username != null)
                {
                    WriteString(payload, _username);
                    WriteString(payload, _password ?? string.Empty);
                }

                byte flags = 0x02;
                if (_username != null)
                {
                    flags |= 0xC0;
                }

                using (var packet = new MemoryStream())
                {
                    WriteString(packet, "MQTT");
                    packet.WriteByte(0x04);
                    packet.WriteByte(flags);
                    packet.WriteByte(0x00);
                    packet.WriteByte(0x1E);
                    payload.Position = 0;
                    payload.CopyTo(packet);
                    return AddHeader(0x10, packet.ToArray());
                }
            }
        }

        private byte[] CreatePublishPacket(string payloadText)
        {
            using (var body = new MemoryStream())
            {
                WriteString(body, _topic);
                byte[] payload = Encoding.UTF8.GetBytes(payloadText);
                body.Write(payload, 0, payload.Length);
                return AddHeader(0x30, body.ToArray());
            }
        }

        private static byte[] AddHeader(byte packetType, byte[] body)
        {
            using (var packet = new MemoryStream())
            {
                packet.WriteByte(packetType);
                WriteRemainingLength(packet, body.Length);
                packet.Write(body, 0, body.Length);
                return packet.ToArray();
            }
        }

        private static void WriteString(Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            stream.WriteByte((byte)(bytes.Length >> 8));
            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteRemainingLength(Stream stream, int length)
        {
            do
            {
                int encodedByte = length % 128;
                length /= 128;
                if (length > 0)
                {
                    encodedByte |= 128;
                }

                stream.WriteByte((byte)encodedByte);
            }
            while (length > 0);
        }

        private static async Task WritePacketAsync(Stream stream, byte[] packet)
        {
            await stream.WriteAsync(packet, 0, packet.Length);
            await stream.FlushAsync();
        }

        private static async Task<byte[]> ReadBytesAsync(Stream stream, int count)
        {
            var result = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(result, offset, count - offset);
                if (read == 0)
                {
                    throw new EndOfStreamException("The MQTT broker closed the connection.");
                }

                offset += read;
            }

            return result;
        }
    }
}