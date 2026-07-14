using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace vPilot_Pushover {

    // Minimal read-only INI accessor backed by the Win32 profile-string API.
    internal class IniFile {

        private const int BufferSize = 255;

        private readonly string _path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string @default, StringBuilder retVal, int size, string filePath);

        public IniFile(string iniPath) {
            _path = new FileInfo(iniPath).FullName;
        }

        // Returns the value, or defaultValue when the key is missing or empty.
        public string Read(string key, string section, string defaultValue = null) {
            var buffer = new StringBuilder(BufferSize);
            GetPrivateProfileString(section, key, "", buffer, BufferSize, _path);
            return buffer.Length > 0 ? buffer.ToString() : defaultValue;
        }

    }
}
