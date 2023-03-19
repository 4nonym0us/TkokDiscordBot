# TKokDiscordBot

A Discord bot of the TKoK (The Kingdom of Kaliron, RPG map for Warcraft 3) community, which allows users to browse loot table, execute their own search queries and obtain detailed information about every item in the game.

## Demo

Live demo is available on [TKoK Discord Server](https://discord.gg/ES2dwYC).

![Demo 1: Querying a single item by name](https://github.com/4nonym0us/TkokDiscordBot/raw/master/assets/images/demo1.gif)
![Demo 2: Interactive item search](https://github.com/4nonym0us/TkokDiscordBot/raw/master/assets/images/demo2.png)

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
Find item by name. Replace <item name> with a item name, does not have to be the full name.

!search-wizard, !sw, !wizard
Starts a search wizard. Provides a way to search items in an easy and interactive way. Great starting point for beginners.

!search <query>, !s <query>
Search for items using full-text search. All filters are optional, multiple filters can be combined. Use double quotes to look for exact phrase match. Can search by name, slot, type, quality, boss, level, class, description or special.
│
│ Examples:
│ !search Druid - find items for Druid.
│ !search "Narith VX" – find loot from Narith UVX.
│ !search Bow - find Bows.
│ !search "Dual Dagger" - find Dual Daggers.
│ !s Accessory AND Epic - find only Epic Accessories.
│ !s Avnos Karnos (or !s Avnos OR Karnos) - find loot from Avnos & Karnos.
│ !s "Phantom Stalker" AND Chest - find chests for PS.
│ !s Warrior AND (Gloves OR Helm) - find gloves and helmets for Warrior.
│
│ Tips:
│ Use !search-wizard for interactive search experience.
│ Use !search-guide for detailed search syntax guide with more examples.
│ Not sure what are possible values for specific property? – Use !explore.

!explore
Get a list of properties and their values that can be used to search items using 'search' command.

!search-guide, !sg
Print a detailed guide with a lot of examples on querying syntax for 'search' command.
```
