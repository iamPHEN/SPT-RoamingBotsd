using System.Collections.Generic;
using System.Linq;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.Visual;
using RoamingBots.Brain.Helpers;
using RoamingBots.Componets;
using RoamingBots.Extensions;
using RoamingBots.Helpers;
using ExilBots.Layers;
using UnityEngine;

#nullable enable

namespace RoamingBots.Brain
{
    internal class SprintToPOILayer : CustomLayer
    {
        public BotComponent Bot
        {
            get
            {
                if (_bot == null)
                {
                    _bot = BotOwner.GetPlayer.gameObject.GetOrAddComponent<BotComponent>();
                    _bot.Init(BotOwner);
                    // Disable the bot by putting it on cooldown right away.
                    WildSpawnType botType = BotOwner.Profile.Info.Settings.Role;
                    if (BotTypeUtils.IsPMC(botType) && Random.Range(0, 100) > RoamingBotsPlugin.SprintEnableBotChancePMC.Value)
                    {
                        _bot.SprintPOILastSprintAttemptCDTime = Time.time + 99999f;
                    }
                    else if (BotTypeUtils.IsScav(botType) && Random.Range(0, 100) > RoamingBotsPlugin.SprintEnableBotChanceScav.Value)
                    {
                        _bot.SprintPOILastSprintAttemptCDTime = Time.time + 99999f;
                    }
                    return _bot;
                }
                return _bot;
            }
        }

        private BotComponent? _bot;

        public SprintToPOILayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
        }

        public override string GetName()
        {
            return "SprintToPOILayer";
        }

        public override Action GetNextAction()
        {
            return new Action(typeof(SprintToPOIAction), $"Sprinting to POI.");
        }

        public override bool IsActive()
        {
            if(Bot.BotOwner == null)
                return false;

            bool isBotActive = BotOwner.BotState == EBotState.Active;
            bool isNotHealing = !BotOwner.Medecine.FirstAid.Have2Do && !BotOwner.Medecine.SurgicalKit.HaveWork;
            bool CanDoAction = !Bot.IsBotHealing && !Bot.IsInCombat && Bot.BotOwner;
            bool IsNearPlayer = RoamingBotsPlugin.SprintToMinDistancePlayer.Value > 0 && Vector3.Distance(Camera.main.transform.position, Bot.BotOwner.Position) < RoamingBotsPlugin.SprintToMinDistancePlayer.Value;
            return RoamingBotsPlugin.EnableBotSprintPOI.Value && 
                Bot
                && Bot.IsBotAlive 
                && (!Bot.SprintPOIHasSprintedRecently || Bot.SprintTimeLeft > 0)
                && isBotActive
                && !IsNearPlayer
                && (CanDoAction || RoamingBotsPlugin.ForceSprint.Value) 
                && HasGoal();
        }

        public bool HasGoal()
        {
            if (Bot.MoveToPosition == null)
            {
                if (TryFindPOIForBot(Bot))
                {
                    return true;
                }
                return false;
            }
            return Bot.MoveToPosition != null;
        }

        private float _RefreshCacheTime = 0f;
        private List<PointOfInterest> CachedPOIs = new();
        private static ExfilFinder ExfilPOIs = new();
        private static LootableContainerFinder Containers = new();
        private static SpawnPointFinder SpawnZoneFinder = new();
        private static QuestPOIFinder QuestZoneFinder = new();
        private int LookExfilChance => RoamingBotsPlugin.SprintToExfilChance.Value;
        private int LookLootableChance => RoamingBotsPlugin.SprintToLootableChance.Value;
        private int LookSpawnPointChance => RoamingBotsPlugin.SprintToSpawnPointsChance.Value;
        private int LookQuestsChance => RoamingBotsPlugin.SprintToQuestsChance.Value;

        //This can be expensive
        private bool TryFindPOIForBot(BotComponent bot)
        {
            if (bot.BotOwner == null)
                return false;

            if(_RefreshCacheTime < Time.time)
            {
                _RefreshCacheTime = Time.time + 5f;
                CachedPOIs.Clear();

                if(LookExfilChance > 0) 
                    ExfilPOIs.RefreshData(CachedPOIs, bot.BotOwner.GetPlayer);

                if(LookLootableChance > 0) 
                    Containers.RefreshData(CachedPOIs, bot.BotOwner.GetPlayer);

                if(LookSpawnPointChance > 0) 
                    SpawnZoneFinder.RefreshData(CachedPOIs, bot.BotOwner.GetPlayer);

                if(LookQuestsChance > 0) 
                    QuestZoneFinder.RefreshData(CachedPOIs, bot.BotOwner.GetPlayer);
            }

            string RandomPOIOwner = nameof(ExfilFinder);

            int totalWeight = LookExfilChance + LookLootableChance + LookSpawnPointChance + LookQuestsChance;
            int randomNumber = Random.Range(0, totalWeight);
            int RangeExfil = LookExfilChance;
            int RangeLootable = LookExfilChance + LookLootableChance;
            int RangeSpawn= LookExfilChance + LookLootableChance + LookSpawnPointChance;
            int RangeQuests = LookExfilChance + LookLootableChance + LookSpawnPointChance + LookQuestsChance;

            switch (randomNumber) 
            {
                case int n when Enumerable.Range(0, RangeExfil).Contains(n):
                    RandomPOIOwner = nameof(ExfilFinder);
                    break;
                case int n when Enumerable.Range(RangeExfil, RangeLootable).Contains(n):
                    RandomPOIOwner = nameof(LootableContainerFinder);
                    break;
                case int n when Enumerable.Range(RangeLootable, RangeSpawn).Contains(n):
                    RandomPOIOwner = nameof(SpawnPointFinder);
                    break;
                case int n when Enumerable.Range(RangeSpawn, RangeQuests).Contains(n):
                    RandomPOIOwner = nameof(QuestPOIFinder);
                    break;
            }

            var EliglbePois = CachedPOIs
                .Where(d => !Bot.ReachedPOI.Contains(d) && d.Owner == RandomPOIOwner);

            PointOfInterest RandomPoi = EliglbePois.PickRandom();

            if(RoamingBotsPlugin.DebugMode.Value)
            {
                RoamingBotsPlugin.LogSource.LogDebug($"{bot.name} SprintPOI head towards: {RandomPoi.Name} {RandomPoi.Position} in {EliglbePois.Count()}/{CachedPOIs.Count()} POIs: Dist: {Vector3.Distance(RandomPoi.Position, bot.BotOwner.Position)}");
            }

            Bot.MoveToPosition = RandomPoi;

            return true;
        }

        public override bool IsCurrentActionEnding()
        {
            return Bot.MoveToPosition == null;
        }
    }
}
