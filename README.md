# vPilot Pushover
[![Github All Releases](https://img.shields.io/github/downloads/blt950/vPilot-Pushover/total.svg)]()

Relay [vPilot](https://vpilot.rosscarlson.dev/) and [Hoppie](https://www.hoppie.nl/acars/) messages to your mobile device via [Pushover](https://pushover.net/).\
Hoppie is integrated directly, meaning you can use any aircraft with this plugin.

![Image of example notification of contact me](https://github.com/blt950/vPilot-Pushover/assets/2505044/68653e8a-8bca-45d4-8220-4a38f39d68d4)

Have also a look at my other projects at my homepage: [https://blt950.com](https://blt950.com)\
I won't make an X-Plane variant of this plugin, [see reasoning here](https://github.com/blt950/vPilot-Pushover/issues/14#issuecomment-1979402032).

## Prerequisites
1. [vPilot](https://vpilot.rosscarlson.dev/) that you use to connect to VATSIM
2. [Pushover](https://pushover.net/) app on your device and subscription
3. [Creating your own Pushover API key](https://pushover.net/apps/build) which is for your personal use

*Pushover has a 30-day trial you can try out before you pay the one-time $5 for a lifetime subscription. You can use Pushover to much more than this plugin, the subscription is in no way tied to this plugin*

## Installation

1. Make sure your vPilot is not running
2. Download the latest release `.zip` only from the [releases page](https://github.com/blt950/vPilot-Pushover/releases)
3. Extract the zip file and move both `vPilot-Pushover.dll` and `vPilot-Pushover.ini` to your vPilot plugin folder, usually `C:\Users\<your username>\AppData\Local\vPilot\Plugins`
4. Open `vPilot-Pushover.ini` in a text editor and configure your desired settings
5. When you start vPilot you should now get an "Connected. Running version x.x.x" push notification. If not, see troubleshooting below.

## Troubleshooting
### I don't receive any connected notification
- Make sure you have placed the plugin in the correct folder, usually like this `C:\Users\<your username>\AppData\Local\vPilot\Plugins\vPilot-Pushover.dll` and `.ini` in the same folder.
- Make sure you've configured both your Pushover user key and your Pushover API key and that they are each surrounded by quotation marks.
- Your Windows might have blocked the `.dll` file. This is because the `.dll` is not [code-signed](https://en.wikipedia.org/wiki/Code_signing) as it's a hobby project and I don't have $200/yearly to purchase a verification. Before you unblock the file, check it with your antivirus or upload it to [VirusTotal](https://www.virustotal.com/gui/home/upload). If you're comfortable to proceed, right click on the `vPilot-Pushover.dll`, open Properties, unblock and apply to allow vPilot to load the plugin.

*Still having an issue? Make an issue request here on Github.*

## Settings
In the `vPilot-Pushover.ini` file, you can configure the following settings:

### [Pushover]
`UserKey` = Your Pushover user key. You can find this on the [Pushover dashboard](https://pushover.net/)\
`ApiKey` = Your Pushover API key. You need to [create this youself in Pushover](https://pushover.net/apps/build)\
`Device` = The device name to send the notifications to. If you leave this blank, it will send to all devices. If you want to specifify multiple devices, separate them with a comma, e.g. `iphone,nexus5`

### [Hoppie]
`Enabled` = Whether or not to relay Hoppie messages to Pushover. Set to `true` or `false`\
`LogonCode` = Your [Hoppie](https://hoppie.nl) logon code.

### [RelayPrivate]
`Enabled` = Whether or not to relay private messages to Pushover. Set to `true` or `false`

### [RelayRadio]
`Enabled` = Whether or not to relay radio messages to Pushover. Only sends radio messages meant for your callsign, e.g. ATC writing to you on text. Set to `true` or `false`

### [EnableSelcal]
`Enabled` = Whether or not to relay SELCAL messages to Pushover. Set to `true` or `false`

## Contribution

Feel free to contribute by creating pull requests or issues in this Github!
