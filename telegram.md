# Creating Telegram Bot
1. Send the message `/newbot` to the BotFather contact in Telegram
2. Give your bot a display name, this name can be anything you like, but somehting like vPilot Bot is most informative
3. Give your bot a user name, this name must be unique and in bot for example `vPilotbot`
4. This will generate a reply with a API token and link to join the chat with your bot, join this chat
5. Send a message to you bot for example `Hello world`
6. In a browser navigate to `https://api.telegram.org/bot<API-token>/getUpdates` (Replace <API-token> with the token obtainend in step 3)
4. In the browser you will see your message object from which you can copy the ChatId, an example of how such a message looks like is shown below 
```
{
    "update_id": 8393,
    "message": {
        "message_id": 3,
        "from": {
            "id": 7474,
            "first_name": "AAA"
        },
        "chat": {
            "id": <ChatId>,
            "title": "<Group name>"
        },
        "date": 25497,
        "new_chat_participant": {
            "id": 71, 
            "first_name": "NAME",
            "username": "YOUR_BOT_NAME"
        }
    }
```
