# TKokDiscordBot

## Demo

![Demo 1: Querying a single item by name](https://github.com/4nonym0us/TkokDiscordBot/raw/master/assets/images/demo1.png)

![Demo 2: Interactive item search](https://github.com/4nonym0us/TkokDiscordBot/raw/master/assets/images/demo2.gif)

## Requirements

 * .NET 6
 * Settings are stored in `user-secrets.json`, which should be located at executable's directory
```json
{
  "DiscordToken": "<YOUR_DISCORD_TOKEN>",
  "BotCommandsChannelId": 378620967066271769,
}
```

## Supported Commands
```
!<item name>
Find item by name. Replace <item name> with a item name, does not have to be the full name

!search <any filter> <class:ClassName> <level:XX> <skip:YY>
Search for items. All filters are optional, multiple filters can be combined. Possible values:
    * filter: any item name, boss name, item type, slot, quality, does not have to be the full name
    * level: number from 8 to 47
    * class: class name (examples: Warrior, ChaoticKnight, Shadowblade, PhantomStalker, ShadowShaman, etc.)
    * skip: number from 0 to any number (used to paginate results as Discord shows only 20 items at once)

Example usage:
    !search Ortakna VX
    !search level:35 hat
    !search hat level:35
    !s class:PhantomStalker skip:20
    !s class ranger shoulder
    !s level 35
```
