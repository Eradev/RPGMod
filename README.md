# RPGMod

This mod is configurable and allows you to have quests in singleplayer and with friends.

![Image of questing](https://i.imgur.com/oog8BE9.jpg)

## Questing

This mod offers a networked questing system that is synced with others who have the mod installed (host must have it installed). Questing, UI and settings are customisable within the config.

**Note:** The mod may lack balancing due to time constraints for testing, however the config should allow for some adjustments.

## Installation

1. Make sure BepInEx is installed
2. Move `RPGMod.dll` into `Risk of Rain 2\BepInEx\plugins` folder
3. Run the game and enjoy.

**Uninstalling:** Simply delete the `RPGMod.dll` file in your plugins folder.

## Updating

When updating simply replace the mod files and run the game, the config may change from time to time so always make a backup if you have changes you want to keep!

## Configuration

This mod has a config with many values to change to your desire with the ability to reload on the go by pressing **F6**.

**Steps on how to configure:**

1. Navigate to the `Risk of Rain 2\BepInEx\config` folder
2. Open the `com.ghasttear1.rpgmod.cfg` file using your preferred text editor
3. Edit the values as you wish.
4. Importantly, don't forget to save the file. If ingame press **F6** to recieve the changes, otherwise starting the game normally will load the new values.
5. Enjoy!

## Changelog

```text
- 1.0.0: Mod release!
- 1.0.1: Updated readme
- 1.0.2: Fixed glitched first quest
- 1.0.3: Fixed teleporter getting stuck, changed config reload to F6 (conflict with ROR2Cheats), added config types, fixed item drop spam, added new config for quest resetting on advancing stage, special thanks to @Targets for help debugging :)
- 1.0.4: Removed debugging messages that were left in
- 1.0.5: Small amount of code cleanup, fixed vertical UI bug and large UI bug, fixed disabling enemydrops making quest progress not work, fixed item spam when player dies or leaves, fixed moving from singleplayer game to multiplayer game, and more
- 1.1.0: Added game scaling for drops helping even out drops experienced at early and end game, added chat message for help with those playing with unmodded players, changed the way the scene placement works making disabled spawns configurable, changed config defaults, descriptions and values
- 1.1.1: Fixed the broken display caused from the latest game update
- 1.1.2: Fixed the possibility of enemy survivors been chosen for a quest target
- 1.2.0: Fixed mod to work with latest update - (had to remove chest drops from bosses as a configurable option as it broke)
- 1.2.1: Reimplemented a fix that was previously in the mod hopefully fixing a problem with quests and particular enemies
- 1.2.2: Fixed problem with multiple drops from a quest
- 1.2.3: Fixed issue where depending on the computers localisation the config values would improperly be loaded
- 1.2.4: Engineer turrets now count towards kills thanks @iDeathHD, quests no longer stop counting progress.
- 1.2.5: No longer any turret issues with other mods modifying turrets, fixed items no longer dropping after some time.
- 1.2.6: Had to rebuild mod to work with new game update and updated readme.
- 2.0.0 (MAJOR UPDATE): Code for questing and more has been completely redone with support for multiple quests at a time, different questing types, a completely new user interface, redone config, turret fixes, better code and much more.
- 2.0.1: Embedded asset bundle into the mod dll, new option for enemy drops to be added to your inventory directly, new option for each reward tier to have the quest objective scale be adjusted.
- 3.0.0 (MAJOR UPDATE): Completely remade the mod, the mod currently only has individual quests per user, losing many older features. Old features may be re-implemented in the future.
- 3.0.1: Fixed mod fully breaking after death and continuing
```

## Issues?

This mod is **WORK IN PROGRESS**, meaning that there is a good chance that there are issues.

If you find any issues with my mod please create a new issue on the github, you can find existing issues here:
https://github.com/ghasttear1/RPGMod/issues

I may not be able to be very quick to fixing issues due to lack of time but I will try.

## Contact

You can find me on the modding discord, I'm new to modding but I will try my best to answer any questions.
