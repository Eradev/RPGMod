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
- 3.0.0 (MAJOR UPDATE): Completely remade the mod, the mod currently only has individual quests per user, losing many older features. Old features may be re-implemented in the future.
- 3.0.1: Fixed mod fully breaking after death and continuing (issue #29)
- 3.0.2: Fixed mod breaking when using other mods that import assetbundles with the "assetbundle" name (issue #33)
- 3.0.3 (URGENT UPDATE): Fixed issues #34 and #35. Cleaned up UI Code
- 3.0.4: Added hudscale override config, Attempt shrine quest, Ensured quests are achievable before sending them, Announcements now queue up
```

## Issues?

This mod is **WORK IN PROGRESS**, meaning that there is a good chance that there are issues.

If you find any issues with my mod please create a new issue on the github, you can find existing issues here:

**Please include information for replicating the issue and preferably a log file.**
https://github.com/ghasttear1/RPGMod/issues

I may not be able to be very quick to fixing issues due to lack of time but I will try.

## Contact

You can find me on the modding discord, I'm not the best at modding but I will try my best to answer any questions.
