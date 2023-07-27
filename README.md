# vPilot Pushover
Relay vPilot and Hoppie messages to your phone or tablet via [Pushover](https://pushover.net/). Requires [vPilot](https://vpilot.rosscarlson.dev/).
![Image of example notification of contact me](https://github.com/blt950/vPilot-Pushover/assets/2505044/9d18ed9d-2a9f-4044-af08-47d6ae33ede3)

## Installation

1. Make sure your vPilot is not running
2. Download the latest release `.zip` only from the [releases page](https://github.com/blt950/vPilot-Pushover/releases)
3. Extract the zip file and move both `vPilot-Pushover.dll` and `vPilot-Pushover.ini` to your vPilot plugin folder, usually `C:\Users\<your username>\AppData\Local\vPilot\Plugins`
4. Open `vPilot-Pushover.ini` in a text editor and configure your desired settings

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

## Contribution

Feel free to contribute by creating pull requests or issues!
