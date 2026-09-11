# MQTT setup

Use MQTT if you want vPilot-Pushover to publish notifications to an MQTT broker and let Home Assistant handle the delivery. This is a simple way to centralize vPilot traffic in Home Assistant without relying on a specific mobile app integration.

## What this does

When the MQTT driver is enabled, vPilot-Pushover connects to your broker and publishes a JSON payload to the topic you choose. The payload contains:

- title
- message
- priority
- source

Example payload:

```json
{
  "title": "vPilot connected",
  "message": "Connected. Running version v1.2.3",
  "priority": 0,
  "source": "startup message"
}
```

---

## 1) Configure vPilot-Pushover

In your vPilot-Pushover.ini file, set the driver to MQTT and fill in the MQTT section.

```ini
[General]
Driver = mqtt

[MQTT]
Host = 192.168.1.50
Port = 1883
Topic = vPilot
Username = myuser
Password = mypassword
UseTls = false
```

### Settings

- Host: MQTT broker hostname or IP address
- Port: MQTT broker port. Usually 1883 for normal MQTT or 8883 for TLS
- Topic: MQTT topic to publish messages to
- Username: Optional broker username
- Password: Optional broker password
- UseTls: Set to true or false

If Port is left blank, vPilot-Pushover will default to 1883 or 8883 depending on TLS.

---

## 2) Home Assistant automation

You do not need to add the vPilot-Pushover app as a device in the MQTT integration. This is just a topic-based notification flow. Home Assistant only needs the MQTT integration enabled and an automation listening to the topic.

This automation listens for messages on the MQTT topic and sends them to Home Assistant's generic notification service.

```yaml
alias: Vpilot Notifications handler
description: "Listen for MQTT messages from vPilot-Pushover and forward them as Home Assistant notifications"
triggers:
  - trigger: mqtt
    topic: vPilot
conditions:
  - condition: template
    value_template: '{{ trigger.payload_json.priority | int >= 1 }}'
actions:
  - action: notify.notify
    data:
      title: "{{ trigger.payload_json.title }}"
      message: "{{ trigger.payload_json.message }}"
mode: single
```

## 3) Example with a persistent notification

```yaml
alias: Vpilot Notifications
description: "Create a persistent Home Assistant notification from vPilot MQTT messages"
triggers:
  - trigger: mqtt
    topic: vPilot
conditions:
  - condition: template
    value_template: '{{ trigger.payload_json.priority | int >= 1 }}'
actions:
  - action: persistent_notification.create
    data:
      title: "{{ trigger.payload_json.title }}"
      message: |
        {{ trigger.payload_json.message }}
        Source: {{ trigger.payload_json.source }}
        Priority: {{ trigger.payload_json.priority }}
```

---

## 4) Troubleshooting

- Check that your MQTT broker is running and reachable
- Make sure the topic matches exactly between vPilot-Pushover and Home Assistant
- Confirm the MQTT integration is enabled in Home Assistant
- Verify username/password if the broker requires authentication
- If using TLS, make sure UseTls is true and the port matches your broker configuration

---

## 5) Notes

This setup is useful if you want Home Assistant to act as your notification manager for vPilot traffic. It is simple, flexible, and works well with automations, alerts, and mobile notifications without requiring a special MQTT device definition.