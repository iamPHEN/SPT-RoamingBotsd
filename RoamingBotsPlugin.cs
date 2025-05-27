using BepInEx;
using BepInEx.Configuration;
using DrakiaXYZ.BigBrain.Brains;
using ExilBots.Layers;
using System.Collections.Generic;
using System;
using RoamingBots.Interop;
using RoamingBots.Patch;
using BepInEx.Logging;
using RoamingBots.ConsoleCommands;
using EFT.UI;
using UnityEngine;
using RoamingBots.Brain;

namespace RoamingBots
{
    [BepInPlugin("phen.roamingbots", "phen-roamingbots", "1.0.0")]
    [BepInDependency(ModInterop.BigBrain, BepInDependency.DependencyFlags.HardDependency)]
    public class RoamingBotsPlugin : BaseUnityPlugin
    {
        public static Commands ConsoleCommands = new();

        public static ManualLogSource LogSource;

        [AutoBindConfig<int>("Layer", "SprintToPOIPriority", "Priorty for SprintToPOI Layer to appear on Roamable Scavs and PMCs", 4, 0, 100, Advanced = true)]
        public static ConfigEntry<int> SprintToPOIPriority { get; set; }

        [AutoBindBoolConfig("xDebugging", "Debug Bot Data", "Enables Bot Data ESP", false, Advanced = true)]
        public static ConfigEntry<bool> DebugBotData { get; set; }

        [AutoBindBoolConfig("xDebugging", "Debug Mode", "Enables Developer debugging and various logging", false, Advanced = true)]
        public static ConfigEntry<bool> DebugMode { get; set; }

        [AutoBindBoolConfig("xDebugging", "Force Sprint", "Forces all bots to sprint around now", false, Advanced = true)]
        public static ConfigEntry<bool> ForceSprint { get; set; }

        [AutoBindBoolConfig("Sprint POI Config", "Enable Bot Roam POIs", "Enables Bots to roam between POIs. Master Enable/Disable bot logic.", true)]
        public static ConfigEntry<bool> EnableBotSprintPOI { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Full Sprint Until Timeout", "If Enabled, bots will continue roaming until the full timeout, recommend to disable to allow more intresting logic from LootingBots to takeover.", false)]
        public static ConfigEntry<bool> EnableBotRoamTillTimeout { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Sprint Corpses", "Toggles Bots to roam to corpses. May cause bots to eventually dogpile into one area.", true)]
        public static ConfigEntry<bool> EnableBotSprintCorpses { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Loose Loot", "Toggles Bots to loose loot locations.", true)]
        public static ConfigEntry<bool> EnableBotSprintLooseLoot { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Roam Static Spawns", "Toggles Bots to roam Static Containers.", true)]
        public static ConfigEntry<bool> EnableBotSprintStaticSpawn { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Roam Scav Exfils", "Toggles Bots to roam Sprint Scav Exfils.", true)]
        public static ConfigEntry<bool> EnableBotSprintScavExfils { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Roam Player Exfils", "Toggles Bots to roam Sprint Player Exfils.", true)]
        public static ConfigEntry<bool> EnableBotSprintPlayerExfils { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Roam Spawn Zones", "Toggles Bots to roam to Spawn Zones.", true)]
        public static ConfigEntry<bool> EnableBotSprintSpawnZones { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Roam SpawnZone Revist", "Toggles Bots to revist SpawnZones.", true)]
        public static ConfigEntry<bool> EnableBotSprintSpawnZonesRevist { get; set; }

        [AutoBindBoolConfig("Sprint POI Config Toggles", "Enable Roam Spawn Points", "Toggles bots to sprint to spawn points. Noisy, spawnzones are sufficent enough.", false)]
        public static ConfigEntry<bool> EnableBotSprintSpawnPoints { get; set; }

        [AutoBindConfig<float>("Sprint POI Config", "Cache POI Time", "Time in seconds we should be refreshing the POI cache. ", 4f, 1f, 3000f)]
        public static ConfigEntry<float> CachePOITime { get; set; }

        [AutoBindConfig<int>("Sprint POI Config", "Sprint PMC Enable Chance", "Chance that the bot will even try to use the roaming logic when the PMC spawns.", 80, 0, 100, ShowRangeAsPercent = true)]
        public static ConfigEntry<int> SprintEnableBotChancePMC { get; set; }

        [AutoBindConfig<int>("Sprint POI Config", "Sprint Scav Enable Chance", "Chance that the bot will even try to use the roaming logic when the Scav spawns.", 60, 0, 100, ShowRangeAsPercent = true)]
        public static ConfigEntry<int> SprintEnableBotChanceScav { get; set; }

        [AutoBindConfig<int>("Sprint POI Config", "Roam Exfil Chance", "Weight for bots to roam to Exfils, if 0 then don't look", 0, 0, 100, ShowRangeAsPercent = true)]
        public static ConfigEntry<int> SprintToExfilChance { get; set; }

        [AutoBindConfig<int>("Sprint POI Config", "Roam Looatable Chance", "Weight for bots to roam to Lootable Containers, if 0 then don't look", 100, 0, 100, ShowRangeAsPercent = true)]
        public static ConfigEntry<int> SprintToLootableChance { get; set; }

        [AutoBindConfig<int>("Sprint POI Config", "Roam Quests Chance", "Weight for bots to roam to Quests, if 0 then don't look", 80, 0, 100, ShowRangeAsPercent = true)]
        public static ConfigEntry<int> SprintToQuestsChance { get; set; }

        [AutoBindConfig<int>("Sprint POI Config", "Roam SpawnPoints Chance", "Weight for bots to roam to SpawnPoints, if 0 then don't look", 5, 0, 100, ShowRangeAsPercent = true)]
        public static ConfigEntry<int> SprintToSpawnPointsChance { get; set; }

        [AutoBindConfig<float>("Sprint POI Config", "Sprint CoolDown Secs", "Min Cooldown before this AI will be allowed to sprint again to the next point.", 300f, 0f, 3000f)]
        public static ConfigEntry<float> SprintCoolDownSecs { get; set; }

        [AutoBindConfig<float>("Sprint POI Config", "Sprint MinDistance Visit", "Min distance before the AI considered they've visted a point.", 20f, 0f, 1000f)]
        public static ConfigEntry<float> SprintToMinDistanceVisit { get; set; }

        [AutoBindConfig<float>("Sprint POI Config", "Sprint Timeout To Visit", "Min time to try going to objective before giving up on it in secs. 0 infinite time.", 120f, 0f, 3000f)]
        public static ConfigEntry<float> SprintToTimeout { get; set; }

        [AutoBindConfig<float>("Sprint POI Config", "Sprint MinDistance ActivatePlayer", "Min distance before the AI activates the sprinting POI layer. 0 means off. (Broken with FIKA)", 0f, 0f, 1000f, Advanced = true)]
        public static ConfigEntry<float> SprintToMinDistancePlayer { get; set; }

        private void SetupConfig()
        {
            try
            {
                ConfigManager.RegisterBinds(this, Config, Logger);
            }
            catch(Exception Error)
            {
                Logger.LogError(Error);
            }

            return;
        }
        public void InitExfilLayer()
        {
            List<string> pmcBrain = new List<string>()
            {
                Interop.Brain.PMC.ToString(),
                Interop.Brain.PmcUsec.ToString(),
                Interop.Brain.PmcBear.ToString(),
                Interop.Brain.Assault.ToString()
            };

            BrainManager.AddCustomLayer(typeof(SprintToPOILayer), pmcBrain, SprintToPOIPriority.Value);

            // This should be the new peacful action
            BrainManager.RemoveLayer("Utility peace", pmcBrain);
        }
        public void Awake()
        {
            LogSource = Logger;
            SetupConfig();
            new SetupComponentPatch().Enable();
            InitExfilLayer();
        }

        public void Update()
        {            
            if (PreloaderUI.Instantiated)
            {
                ConsoleCommands.RegisterCommands();
            }
        }
    }
}
