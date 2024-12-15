1. Message BotFather in Telegram with `/newbot`
2. Give your bot a name by replying (name must end in bot) for example with `vPilot_bot`
3. This will generate a reply with a BotToken and link to join the chat with your bot, join this chat
4. Send a message to you bot for example `Hello world`
5. In a browser navigate to `https://api.telegram.org/bot<API-token>/getUpdates` (Replace <API-token> with the token obtainend in step 3)
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
