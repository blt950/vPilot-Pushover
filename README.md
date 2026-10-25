# vPilot Pushover
[![Github All Releases](https://img.shields.io/github/downloads/blt950/vPilot-Pushover/total.svg)](https://github.com/blt950/vPilot-Pushover/releases)

Relay [vPilot](https://vpilot.rosscarlson.dev/) and [Hoppie](https://www.hoppie.nl/acars/) messages to your mobile device via [Pushover](https://pushover.net/), [Telegram](https://telegram.org/), [Gotify](https://gotify.net/) or [Bark](https://bark.day.app/#/en-us/?id=bark), or publish them to an MQTT broker.\
Hoppie is integrated directly, meaning you can use any aircraft with this plugin.

![Image of example notification of contact me](https://github.com/blt950/vPilot-Pushover/assets/2505044/68653e8a-8bca-45d4-8220-4a38f39d68d4)

Also have a look at my other projects at my homepage: [https://blt950.com](https://blt950.com)\
I won't make an X-Plane variant of this plugin, [see reasoning here](https://github.com/blt950/vPilot-Pushover/issues/14#issuecomment-1979402032).

## Prerequisites
You need [vPilot](https://vpilot.rosscarlson.dev/) that you use to connect to VATSIM. Then choose one of the notifiers under [Settings](#settings) and follow its prerequisites.

## Installation

1. Make sure your vPilot is not running
2. Download the latest release `.zip` only from the [releases page](https://github.com/blt950/vPilot-Pushover/releases)
3. Extract the zip file and move both `vPilot-Pushover.dll` and `vPilot-Pushover.ini` to your vPilot plugin folder, usually `C:\Users\<your username>\AppData\Local\vPilot\Plugins`
4. Open `vPilot-Pushover.ini` in a text editor and configure your desired [settings](#settings)
5. When you start vPilot you should now get a "Connected. Running version x.x.x" push notification. If not, see [troubleshooting](#troubleshooting) below.

## Settings
In the `vPilot-Pushover.ini` file, you can configure the following settings.

### [General]

| Key | Description |
|---|---|
| `Driver` | Choose **one** notifier, write `pushover`, `telegram`, `gotify`, `bark` or `mqtt` in lowercase. Only one notifier can be active at a time. |

### Notifier
You can only use **one** notifier at a time. Expand the one you set as `Driver` and configure only that section; the others are not used. Each lists its prerequisites and settings.

<details>
<summary><strong>Pushover</strong></summary>

**Prerequisites**
- [Create your own Pushover API key](https://pushover.net/apps/build) which is for your personal use
- Pushover has a 30-day trial you can try out before you pay the one-time $5 for a lifetime subscription. You can use Pushover for much more than this plugin, the subscription is in no way tied to this plugin

**Settings** in the `[Pushover]` section

| Key | Description | Default |
|---|---|---|
| `UserKey` | Your Pushover user key. You can find this on the [Pushover dashboard](https://pushover.net/) | *required* |
| `ApiKey` | Your Pushover API key. You need to [create this yourself in Pushover](https://pushover.net/apps/build) | *required* |
| `Device` | The device name to send the notifications to. If you leave this blank, it will send to all devices. If you want to specify multiple devices, separate them with a comma, e.g. `iphone,nexus5` | all devices |
| `HighPriRetries` | How often, in seconds, Pushover re-alerts an unacknowledged high priority (priority `2`) notification until you acknowledge it. Minimum `30` | `30` |
| `HighPriExpire` | How long, in seconds, Pushover keeps retrying a high priority notification before giving up. Maximum `10800` | `300` (5 minutes) |

**Priority:** `-2` to `2`, see the [Pushover docs](https://pushover.net/api#priority). Priority `2` is an emergency notification that repeats using `HighPriRetries`/`HighPriExpire` until you acknowledge it.

</details>

<details>
<summary><strong>Telegram</strong> (by <a href="https://github.com/rensdebee">@rensdebee</a>)</summary>

**Prerequisites**
- [Create a Telegram bot](telegram.md) for credentials you need for the settings

**Settings** in the `[Telegram]` section

| Key | Description | Default |
|---|---|---|
| `BotToken` | Your Telegram bot API key, see [this](telegram.md) for instructions | *required* |
| `ChatId` | Your Telegram bots chat id key, see [this](telegram.md) for instructions | *required* |

**Priority:** Not used, Telegram ignores the `Priority` settings.

</details>

<details>
<summary><strong>Gotify</strong> (by <a href="https://github.com/nonenty">@nonenty</a>)</summary>

**Prerequisites**
- It's required to install a Gotify server beforehand. Check [Gotify Docs](https://gotify.net/docs/index) for more information
- Please note that only Android phones are officially supported by them. See [this](https://github.com/gotify/android)

**Settings** in the `[Gotify]` section

| Key | Description | Default |
|---|---|---|
| `Url` | Your Gotify server address. For example, `https://push.example.com` or `https://example.com/gotify`, depending on your server configuration. | *required* |
| `Token` | Your Gotify application token, see [this](https://gotify.net/docs/pushmsg) | *required* |

**Priority:** `0` to `10`, see the [Gotify docs](https://gotify.net/docs/priority).

</details>

<details>
<summary><strong>Bark</strong> (by <a href="https://github.com/PorridgePi">@PorridgePi</a>)</summary>

**Prerequisites**
- Install the iOS App from the [App Store](https://apps.apple.com/us/app/bark-custom-notifications/id1403753865)
- (Optional) Set up a self-hosted backend server using [Finb/bark-server](https://github.com/Finb/bark-server) following the [Deployment Guide](https://bark.day.app/#/en-us/deploy)

**Settings** in the `[Bark]` section

| Key | Description | Default |
|---|---|---|
| `Url` | Your Bark server address. Only change this if you self-host a backend server with a custom domain. | `https://api.day.app` |
| `Key` | Your Bark push key. Follow the [Tutorial](https://bark.day.app/#/en-us/tutorial) to obtain your key. | *required* |

**Priority:** `-1` to `2`, mapping to the `passive`, `active`, `timeSensitive` and `critical` levels, see the [Bark docs](https://bark.day.app/#/en-us/tutorial?id=request-parameters).

</details>

<details>
<summary><strong>MQTT</strong> (by <a href="https://github.com/rick2909">@rick2909</a>)</summary>

**Prerequisites**
- Use any MQTT 3.1.1-compatible broker. Messages are published at QoS 0.
- The payload is JSON with separate `title`, `message`, `priority`, and `source` properties.
- TLS is supported with broker hostname certificate validation.
- See [this MQTT guide](mqtt.md) if you intend to use MQTT with Home Assistant.

**Settings** in the `[MQTT]` section

| Key | Description | Default |
|---|---|---|
| `Host` | MQTT broker hostname or IP address | *required* |
| `Port` | MQTT broker port | `1883`, or `8883` when `UseTls=true` |
| `Topic` | MQTT topic to publish notifications to | *required* |
| `Username` | Broker username | optional |
| `Password` | Broker password | optional |
| `UseTls` | Whether to use TLS. Set to `true` or `false` | `false` |

**Priority:** Passed through unchanged as the `priority` property in the JSON payload, so you decide what it means in your automation.

</details>

### Messages
Each message type below has an `Enabled` key (`true` or `false`) and a `Priority` key. `Priority` sets how urgently the notification is delivered. The allowed range depends on your driver, see **Priority** in your driver's section above.

| Section | Relays | Default priority |
|---|---|---|
| `[RelayPrivate]` | Private messages, including contact me's | `1` |
| `[RelayRadio]` | Radio messages meant for your callsign, e.g. ATC writing to you on text | `1` |
| `[RelaySelcal]` | SELCAL alerts | `1` |
| `[Disconnect]` | A message when you are disconnected from the network | `1` |
| `[Hoppie]` | Hoppie/ACARS messages. Also requires `LogonCode`, see below | `0` |

### [Hoppie]

| Key | Description |
|---|---|
| `Enabled` | Whether or not to relay Hoppie messages. Set to `true` or `false` |
| `LogonCode` | Your [Hoppie](https://hoppie.nl) logon code |
| `Priority` | Notification priority for Hoppie/ACARS messages (see [Messages](#messages)). Default `0` |

<details>
<summary><strong>Advanced Options</strong></summary> 

### [SilentStartup]
`Enabled` = Will start vPilot silently without the startup notification. Set to `true` or `false`\
⚠️ **Warning:** Connection notification verifies that the plugin communicates with the driver when vPilot starts and alerts you immediately if there's an issue. We don't offer support if you turn this off.
</details>

## Troubleshooting
### I don't receive any connected notification
- Make sure you have placed the plugin in the correct folder, usually like this `C:\Users\<your username>\AppData\Local\vPilot\Plugins\vPilot-Pushover.dll` and `.ini` in the same folder.
- Make sure you've configured both your Pushover user key and your Pushover (or other driver) API key and that they are each surrounded by quotation marks.
- Make sure there's no `RossCarlson.Vatsim.Vpilot.Plugins.dll` and `.xml` inside the `vPilot\Plugins` folder. There's a known issue where FSLabs installer mistakenly places these file in the folder which breaks the vPilot plugin functionality.
- Your Windows might have blocked the `.dll` file. This is because the `.dll` is not [code-signed](https://en.wikipedia.org/wiki/Code_signing) as it's a hobby project and I don't have $200/yearly to purchase a verification. Before you unblock the file, check it with your antivirus or upload it to [VirusTotal](https://www.virustotal.com/gui/home/upload). If you're comfortable to proceed, right click on the `vPilot-Pushover.dll`, open Properties, unblock and apply to allow vPilot to load the plugin.

*Still having an issue? Make an issue request here on Github.*

## Contribution

Feel free to contribute by creating pull requests or issues in this Github!

### Building

The project references the vPilot SDK assembly used by the plugin API. Its location is different between developer machines, so it is not hard-coded in the project file.

1. Copy `Directory.Build.props.example` to `Directory.Build.props`.
2. Set `VpilotPluginsPath` to the full path of `RossCarlson.Vatsim.Vpilot.Plugins.dll` on your machine.
3. Run `dotnet build`.

You can also set the path for one build without creating the local props file:

```powershell
dotnet build -p:VpilotPluginsPath="C:\path\to\RossCarlson.Vatsim.Vpilot.Plugins.dll"
```

The `VPILOT_PLUGINS_DLL` environment variable is also supported.