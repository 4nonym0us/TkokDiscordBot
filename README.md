# TKokDiscordBot

## Requirements

 * .NET Framework 4.6.1 or above is required.
 * Settings are stored in `user-secrets.json`, which should be stored at executable directory
```json
{
  "DiscordToken": "~",
  "EntUsername": "~",
  "EntPassword": "~",
  "EntMapId": "~",
  "DiscordBotAdmins": "~",
  "MainChannelId": 214755622711853056
}
```
## Supported Commands
```
!host <owner> [region]
Host a game on Ent. Default region: europe. Available regions: atlanta, ny, la, europe, au, jp, sg.

!<item name>
Find item by name. Replace <item name> with a item name, does not have to be the full name

!track <gamename>
Tracks a game on EntGaming.Net by game name. Lobby info will be displayed in the Topic of main channel.

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
