using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace vPilot_Pushover.Notifications {

    // Surfaces plugin errors as a topmost dialog. Load failures get a button that
    // opens the troubleshooting guide; runtime errors skip it, since the message
    // usually carries a more specific URL of its own — any URL in the message is
    // rendered as a clickable link either way.
    internal static class ErrorNotifier {

        private const string TroubleshootingUrl = "https://github.com/blt950/vPilot-Pushover#troubleshooting";

        private static readonly Regex UrlPattern = new Regex(@"https?://\S+", RegexOptions.Compiled);

        public static void Show(string title, string message, bool showTroubleshootingGuide = true) {
            try {
                using (var dialog = BuildDialog(title, message, showTroubleshootingGuide)) {
                    SystemSounds.Exclamation.Play();
                    dialog.ShowDialog();
                }
            } catch {
                // Never let a notification failure crash the plugin.
            }
        }

        private static Form BuildDialog(string title, string message, bool showTroubleshootingGuide) {
            var form = new Form {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                TopMost = true,
                ClientSize = new Size(480, 95)
            };

            var iconBox = new PictureBox {
                Image = SystemIcons.Warning.ToBitmap(),
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(12, 14)
            };

            var okButton = new Button {
                Text = "OK",
                Size = new Size(85, 25),
                Location = new Point(383, 60),
                DialogResult = DialogResult.OK
            };

            form.Controls.AddRange(new Control[] { iconBox, BuildMessageLabel(message), okButton });

            if (showTroubleshootingGuide) {
                var troubleshootButton = new Button {
                    Text = "Open Troubleshooting Guide",
                    Size = new Size(195, 25),
                    Location = new Point(12, 60),
                    DialogResult = DialogResult.OK
                };
                troubleshootButton.Click += (s, e) => OpenUrl(TroubleshootingUrl);
                form.Controls.Add(troubleshootButton);
            }

            form.AcceptButton = okButton;
            form.CancelButton = okButton;
            return form;
        }

        // Renders the message with any contained URLs clickable.
        private static LinkLabel BuildMessageLabel(string message) {
            var label = new LinkLabel {
                Text = message,
                Location = new Point(54, 12),
                Size = new Size(414, 42),
                LinkArea = new LinkArea(0, 0)
            };

            foreach (Match match in UrlPattern.Matches(message)) {
                string url = match.Value.TrimEnd('.', ',', ';', ':', ')', ']', '"', '\'');
                label.Links.Add(match.Index, url.Length, url);
            }

            label.LinkClicked += (s, e) => OpenUrl((string)e.Link.LinkData);
            return label;
        }

        private static void OpenUrl(string url) {
            try {
                Process.Start(url);
            } catch {
                // Browser launch failure shouldn't crash the dialog.
            }
        }

    }
}
