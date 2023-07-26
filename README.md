# vPilot Pushover
Relay vPilot and Hoppie messages to your phone or tablet via [Pushover](https://pushover.net/). Requires [vPilot](https://vpilot.rosscarlson.dev/).
![Image of example notification of contact me](https://github.com/blt950/vPilot-Pushover/assets/2505044/6d7ce09c-ae17-4d82-9584-70a052e60268)

## Installation
1. Download the latest release `.zip` only from the [releases page](https://github.com/blt950/vPilot-Pushover/releases)
2. Extract the zip file and move both `vPilot-Pushover.dll` and `vPilot-Pushover.ini` to your vPilot plugin folder, usually `C:\Users\<your username>\AppData\Local\vPilot\Plugins`
3. Open `vPilot-Pushover.ini` in a text editor and configure your desired settings

## Settings
In the `vPilot-Pushover.ini` file, you can configure the following settings:

### [Pushover]
`UserKey` = Your Pushover user key. You can find this on the [Pushover dashboard](https://pushover.net/)\
`ApiKey` = Your Pushover API key. You need to [create this youself in Pushover](https://pushover.net/apps/build)

### [Hoppie]
`Enabled` = Whether or not to relay Hoppie messages to Pushover. Set to `true` or `false`\
`LogonCode` = Your [Hoppie](https://hoppie.nl) logon code.

### [RelayPrivate]
`Enabled` = Whether or not to relay private messages to Pushover. Set to `true` or `false`

### [RelayRadio]
`Enabled` = Whether or not to relay radio messages to Pushover. Only sends radio messages meant for your callsign, e.g. ATC writing to you on text. Set to `true` or `false`
