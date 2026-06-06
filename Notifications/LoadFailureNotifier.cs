using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace vPilot_Pushover.Notifications {

    // Surfaces critical plugin load failures as a blocking Windows dialog
    // with a button that opens the troubleshooting guide in the browser.
    // Used because vPilot's debug log is hidden unless the user launched
    // vPilot in debug mode, but these messages require user action.
    internal static class LoadFailureNotifier {

        private const string TroubleshootingUrl = "https://github.com/blt950/vPilot-Pushover#troubleshooting";

        public static void Show(string title, string message) {
            try {
                using (var dialog = BuildDialog(title, message)) {
                    // Matches the warning icon — Windows' standard "exclamation" ding.
                    SystemSounds.Exclamation.Play();
                    dialog.ShowDialog();
                }
            } catch {
                // Never let a notification failure crash plugin load.
            }
        }

        private static Form BuildDialog(string title, string message) {
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

            var label = new Label {
                Text = message,
                Location = new Point(54, 12),
                Size = new Size(414, 35)
            };

            var troubleshootButton = new Button {
                Text = "Open Troubleshooting Guide",
                Size = new Size(195, 25),
                Location = new Point(12, 60),
                DialogResult = DialogResult.OK
            };
            troubleshootButton.Click += (s, e) => {
                try {
                    Process.Start(TroubleshootingUrl);
                } catch {
                    // Browser launch failure shouldn't crash the dialog.
                }
            };

            var okButton = new Button {
                Text = "OK",
                Size = new Size(85, 25),
                Location = new Point(383, 60),
                DialogResult = DialogResult.OK
            };

            form.Controls.AddRange(new Control[] { iconBox, label, troubleshootButton, okButton });
            form.AcceptButton = okButton;
            form.CancelButton = okButton;
            return form;
        }

    }
}
