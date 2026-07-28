# vPilot Pushover
[![Github All Releases](https://img.shields.io/github/downloads/blt950/vPilot-Pushover/total.svg)]()

Relay [vPilot](https://vpilot.rosscarlson.dev/) and [Hoppie](https://www.hoppie.nl/acars/) messages to your mobile device via [Pushover](https://pushover.net/), [Telegram](https://telegram.org/), [Gotify](https://gotify.net/) or [Bark](https://bark.day.app/#/en-us/?id=bark).\
Hoppie is integrated directly, meaning you can use any aircraft with this plugin.

![Image of example notification of contact me](https://github.com/blt950/vPilot-Pushover/assets/2505044/68653e8a-8bca-45d4-8220-4a38f39d68d4)

Have also a look at my other projects at my homepage: [https://blt950.com](https://blt950.com)\
I won't make an X-Plane variant of this plugin, [see reasoning here](https://github.com/blt950/vPilot-Pushover/issues/14#issuecomment-1979402032).

## Prerequisites
You need [vPilot](https://vpilot.rosscarlson.dev/) that you use to connect to VATSIM. Then choose one of the following notifiers.

### Pushover
- [Create your own Pushover API key](https://pushover.net/apps/build) which is for your personal use
- Pushover has a 30-day trial you can try out before you pay the one-time $5 for a lifetime subscription. You can use Pushover to much more than this plugin, the subscription is in no way tied to this plugin

### Telegram
- [Create a Telegram bot](telegram.md) for credentials you need for the settings

### Gotify
- It's required to install gotify server beforehand. Check [Gotify Docs](https://gotify.net/docs/index) for more infomation
- Please note that only Android phone is officially supported by them. See [this](https://github.com/gotify/android)

### Bark
- Install the iOS App from the [App Store](https://apps.apple.com/us/app/bark-custom-notifications/id1403753865)
- (Optional) Set up a self-hosted backend server using [Finb/bark-server](https://github.com/Finb/bark-server) following the [Deployment Guide](https://bark.day.app/#/en-us/deploy)

## Installation

1. Make sure your vPilot is not running
2. Download the latest release `.zip` only from the [releases page](https://github.com/blt950/vPilot-Pushover/releases)
3. Extract the zip file and move both `vPilot-Pushover.dll` and `vPilot-Pushover.ini` to your vPilot plugin folder, usually `C:\Users\<your username>\AppData\Local\vPilot\Plugins`
4. Open `vPilot-Pushover.ini` in a text editor and configure your desired [settings](#settings)
5. When you start vPilot you should now get an "Connected. Running version x.x.x" push notification. If not, see [troubleshooting](#troubleshooting) below.

## Settings
In the `vPilot-Pushover.ini` file, you can configure the following settings:

Several message types below take a `Priority` value that controls how urgently the notification is delivered. This is only used by **Pushover** ([`-2` to `2`](https://pushover.net/api#priority)), **Gotify** ([`0` to `10`](https://gotify.net/docs/priority)) and **Bark** (`-1` to `2`, mapping to `passive`, `active`, `timeSensitive` and `critical` levels - [see docs](https://bark.day.app/#/en-us/tutorial?id=request-parameters)); **Telegram** ignores it. For Pushover, priority `2` is an emergency notification that repeats using the `HighPriRetries`/`HighPriExpire` settings until you acknowledge it.

### [General]
`Driver` = Choose your notifier method, write `pushover`, `telegram`, `gotify` or `bark` in lowercase.

### [Pushover]
`UserKey` = Your Pushover user key. You can find this on the [Pushover dashboard](https://pushover.net/)\
`ApiKey` = Your Pushover API key. You need to [create this youself in Pushover](https://pushover.net/apps/build)\
`Device` = The device name to send the notifications to. If you leave this blank, it will send to all devices. If you want to specifify multiple devices, separate them with a comma, e.g. `iphone,nexus5`\
`HighPriRetries` = How often, in seconds, Pushover re-alerts an unacknowledged high priority (priority `2`) notification until you acknowledge it. Minimum `30`. Default `30`\
`HighPriExpire` = How long, in seconds, Pushover keeps retrying a high priority notification before giving up. Maximum `10800`. Default `300` (5 minutes)

### [Telegram]
`BotToken` = Your Telgram bot API key, see [this](telegram.md) for instructions\
`ChatId` = Your Telgram bots chat id key, see [this](telegram.md) for instructions

### [Gotify]
`Url` = Your Gotify server address. For example, `https://push.example.com`, `https://example.com/gotify`, depending on your server configuration.\
`Token` = Your Gotify application token. see [this](https://gotify.net/docs/pushmsg)

### [Bark]
`Url` = Your Bark server address. Defaults to `https://api.day.app` unless you self-host a backend server with a custom domain.\
`Key` = Your Bark push key. Follow the [Tutorial](https://bark.day.app/#/en-us/tutorial) to obtain your key.

### [Hoppie]
`Enabled` = Whether or not to relay Hoppie messages. Set to `true` or `false`\
`LogonCode` = Your [Hoppie](https://hoppie.nl) logon code.\
`Priority` = Notification priority for Hoppie/ACARS messages (see note above). Default `0`

### [RelayPrivate]
`Enabled` = Whether or not to relay private messages, including contact me's. Set to `true` or `false`\
`Priority` = Notification priority for private messages (see note above). Default `1`

### [RelayRadio]
`Enabled` = Whether or not to relay radio messages. Only sends radio messages meant for your callsign, e.g. ATC writing to you on text. Set to `true` or `false`\
`Priority` = Notification priority for radio messages (see note above). Default `1`

### [RelaySelcal]
`Enabled` = Whether or not to relay SELCAL messages. Set to `true` or `false`\
`Priority` = Notification priority for SELCAL alerts (see note above). Default `1`

### [Disconnect]
`Enabled`= Whether or not to send message when disconneted from network. Set to `true` or `false`\
`Priority` = Notification priority for disconnect messages (see note above). Default `1`

## Troubleshooting
### I don't receive any connected notification
- Make sure you have placed the plugin in the correct folder, usually like this `C:\Users\<your username>\AppData\Local\vPilot\Plugins\vPilot-Pushover.dll` and `.ini` in the same folder.
- Make sure you've configured both your Pushover user key and your Pushover (or other driver) API key and that they are each surrounded by quotation marks.
- Make sure there's no `RossCarlson.Vatsim.Vpilot.Plugins.dll` and `.xml` inside the `vPilot\Plugins` folder. There's a known issue where FSLabs installer mistakenly places these file in the folder which breaks the vPilot plugin functionality.
- Your Windows might have blocked the `.dll` file. This is because the `.dll` is not [code-signed](https://en.wikipedia.org/wiki/Code_signing) as it's a hobby project and I don't have $200/yearly to purchase a verification. Before you unblock the file, check it with your antivirus or upload it to [VirusTotal](https://www.virustotal.com/gui/home/upload). If you're comfortable to proceed, right click on the `vPilot-Pushover.dll`, open Properties, unblock and apply to allow vPilot to load the plugin.

*Still having an issue? Make an issue request here on Github.*

## Contribution

Feel free to contribute by creating pull requests or issues in this Github!
