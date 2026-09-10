# MQTT setup

This guide explains how to configure vPilot-Pushover to publish notifications to an MQTT broker, and how to consume those messages in Home Assistant.

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