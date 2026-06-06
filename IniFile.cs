using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace vPilot_Pushover {
    internal class IniFile {

        private const int BufferSize = 255;

        private readonly string _path;
        private readonly string _defaultSection = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string @default, StringBuilder retVal, int size, string filePath);

        public IniFile(string iniPath = null) {
            _path = new FileInfo(iniPath ?? _defaultSection + ".ini").FullName;
        }

        public string Read(string key, string section = null) {
            var buffer = new StringBuilder(BufferSize);
            GetPrivateProfileString(section ?? _defaultSection, key, "", buffer, BufferSize, _path);
            return buffer.ToString();
        }

        public string Read(string key, string section, string defaultValue) {
            return KeyExists(key, section) ? Read(key, section) : defaultValue;
        }

        public void Write(string key, string value, string section = null) {
            WritePrivateProfileString(section ?? _defaultSection, key, value, _path);
        }

        public void DeleteKey(string key, string section = null) {
            Write(key, null, section ?? _defaultSection);
        }

        public void DeleteSection(string section = null) {
            Write(null, null, section ?? _defaultSection);
        }

        public bool KeyExists(string key, string section = null) {
            return Read(key, section).Length > 0;
        }
    }
}
