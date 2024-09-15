# RPGMod

This mod is configurable and allows you to have quests in singleplayer and with friends.

![Image of questing](https://i.imgur.com/oog8BE9.jpg)

## Questing

This mod offers a networked questing system that is synced with others who have the mod installed (host and clients must have it installed). Questing, UI and settings are customisable within the config.

**Note:** The mod may lack balancing due to time constraints for testing, however the config should allow for some adjustments.

## Version 3.1.0+

A new maintainer has taken over the original mod starting from version 3.1.0. If you have any issues, please create an issue at <https://github.com/Eradev/RPGMod/issues>.

Huge thanks to the original creator, ghasttear1, for this wonderful mod.

## Installation

Using a mod manager such as r2modman is strongly recommended.

### Manual installation

1. Make sure BepInEx is installed
2. Move `RPGMod.dll` into `Risk of Rain 2\BepInEx\plugins` folder
3. Run the game and enjoy.

### Uninstalling

Simply delete the `RPGMod.dll` file in your plugins folder.

### Updating

When updating simply replace the mod files and run the game, the config may change from time to time so always make a backup if you have changes you want to keep!

## Configuration

This mod has a config with various options to tailor it to your playgroup! Compatible with Risk of Options if you wish to set it up directly in-game!

## Changelog

```text
- 3.7.0
 * Rewrote all options and added support for Risk of Options.
 * Added options to blacklist enemy types and elite buffs.

- 3.6.2
 * Fixed a bug in mission generation.
 
- 3.6.1
 * Fixed a bug in mission generation.
 
- 3.6.0
 * Added a dynamic Kill specific enemies mission.
 * Added an option to disable Collect gold mission.
 * Added options to specify the number of missions to generate per item tier.
 * Added new announcement messages.
 * Removed all hardcoded kill enemies with EliteBuff missions with a dynamic one.
 * Removed kill any buffed enemies mission.

- 3.5.1
 * Fixed quest check when timer runs out.

- 3.5.0
 * Added a configurable timer for quests.
 
- 3.4.0
 * Added options to disable sending announcements. (Host)
 * Fixed some options' description. Please check your settings if you have changed the default values.
 * Fixed mission Kill any buffed enemy to take account that they can have multiple buffs.

- 3.3.0
 * Updated for RoR2 1.3.2.
 * Added 4 new missions: Kill any enemy, Kill any buffed enemy, Kill Gilded enemy (DLC2), and Kill Twisted enemy (DLC2).
 * Fixed some options' description. Please check your settings if you have changed the default values.
 * Removed Kill enemies with a backstab mission.

- 3.2.0
 * Quest rewards now unlock logbook entries, and display notification for new items.
 * Added an option to blacklist items.

- 3.1.1
 * Added missions for other Elite buffs, including the two from DLC1.
 * Added Kill enemies with a backstab mission.
 * Added an option to disable certain missions in the config file.
 * Fixed some typos in the config file. Please check your settings if you have changed the default values.

- 3.1.0:
 * Updated for RoR2 1.2.4.
 * Added Kill champion enemies, Kill flying enemies, Kill Celestine enemies, and Kill Malachite enemies missions.
 * Added missions scaling as you progress through the stages.
 * Added an option to position the announcer on the Y axis.
 * Quests are now cleared between stages.
 * Minion kills now count toward missions progress.
 * Removed Attempt Chance Shrine mission.

- 3.0.4:
 * Added hudscale override config.
 * Attempt shrine quest.
 * Ensured quests are achievable before sending them.
 * Announcements now queue up.

- 3.0.3: 
 * Fixed issues #34 and #35.
 * Cleaned up UI Code

- 3.0.2
 * Fixed mod breaking when using other mods that import assetbundles with the "assetbundle" name (issue #33)

- 3.0.1
 * Fixed mod fully breaking after death and continuing (issue #29)

- 3.0.0
 * Completely remade the mod, the mod currently only has individual quests per user, losing many older features. Old features may be re-implemented in the future.
```
