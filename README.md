# vPilot Pushover
Relay [vPilot](https://vpilot.rosscarlson.dev/) and [Hoppie](https://www.hoppie.nl/acars/) messages to your mobile device via [Pushover](https://pushover.net/).\
Hoppie is integrated directly, meaning you can use any aircraft with this plugin.

![Image of example notification of contact me](https://github.com/blt950/vPilot-Pushover/assets/2505044/68653e8a-8bca-45d4-8220-4a38f39d68d4)

Have also a look at my other projects at my homepage: [https://blt950.com](https://blt950.com)

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
