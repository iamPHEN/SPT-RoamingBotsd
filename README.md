# SPT-RoamingBotsd
Mod that makes bots roam more in SPT, so bots that spawn in farther areas of the map will attempt to roam to Lootable Locations, Exfils, Questing Locations. Makes areas that are (transit areas) that are normally empty have bots roam there on occasion.

With this mod bots will roam out of their assigned patrol zone to a POI for the "Timeout" duration, and won't re-roam until the "Cooldown" has trigered. Tweaking cooldown and Timeout to larger numbers will cause bots to roam more.

If the bot senses enemies or is engaged in combat it will disable the roaming logic. 

Yes this works will work with SAIN, Looting Bots, Questing Bots. Highly recommended you try this with those mods installed.


Built for SPT 3.11: https://hub.sp-tarkov.com/

Requires big brain: https://github.com/DrakiaXYZ/SPT-BigBrain/


# Thanks

Big shoutout to dewardiandev for helping me get into modding for SPT.

# TODO / Public Milestones

Going to keep this mod fairly simple and efficent to add to any SPT install.

## Before 1.0 Release:

- [ ] Add GHA to Put plugin into a .zip with /BepinEx/plugin to match normal plugin relases
- [ ] Tweak options so they 'feel' good out of the box
- [ ] Optimize bot memory usage
- [ ] Write better memory caching system (currently just hardcoding times eveywhere)
- [ ] Expose options to allow bots to filter for lootables (Containers vs Corpses vs Static Spawns)
- [ ] Expose options to allow bots to filter which exfils they visit
- [ ] Expose options to allow bots to vist locations more than once
- [ ] Reivew everything for spelling errors -- execpt the repo name -- I can't fix that now its too late

## For 1.1 Release:

- [ ] Add options to allow bots to stay in their new zone when the roam there -- currently the assault patrol makes them roam back to their previous zone
- [ ] Add options to keep "bot groups" together -- currently they may wander to diffrent POIs 
- [ ] Add options to add roaming logic to more bots than PMCs and Scavs (Bosses, Cultists, Followers).
