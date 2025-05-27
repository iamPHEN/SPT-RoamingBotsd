using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using RoamingBots.Brain.Helpers;
using RoamingBots.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoamingBots.ConsoleCommands
{
    internal class POICache : ConsoleCommandWithoutArgument
    {
        public override string Name => "RefreshPOICache";
        public bool Enabled => true;

        private float _RefreshCacheTime = 0f;
        public static List<PointOfInterest> CachedPOIs = new();

        private static ExfilFinder ExfilPOIs = new();
        private static LootableContainerFinder Containers = new();
        private static SpawnPointFinder SpawnZoneFinder = new();
        private static QuestPOIFinder QuestZoneFinder = new();
        private float CacheRefreshTime => RoamingBotsPlugin.CachePOITime.Value;

        private static int LookExfilChance => RoamingBotsPlugin.SprintToExfilChance.Value;
        private static int LookLootableChance => RoamingBotsPlugin.SprintToLootableChance.Value;
        private static int LookSpawnPointChance => RoamingBotsPlugin.SprintToSpawnPointsChance.Value;
        private static int LookQuestsChance => RoamingBotsPlugin.SprintToQuestsChance.Value;
        public void Update()
        {
            RefreshData();
        }

        public static IEnumerable<PointOfInterest> GetRandomPoi()
        {
            string RandomPOIOwner = nameof(ExfilFinder);

            int totalWeight = LookExfilChance + LookLootableChance + LookSpawnPointChance + LookQuestsChance;
            int randomNumber = Random.Range(0, totalWeight);
            int RangeExfil = LookExfilChance;
            int RangeLootable = LookExfilChance + LookLootableChance;
            int RangeSpawn = LookExfilChance + LookLootableChance + LookSpawnPointChance;
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

            return POICache.CachedPOIs
                .Where(d => d.Owner == RandomPOIOwner);
        }

        public void RefreshData()
        {

            GameWorld world = Singleton<GameWorld>.Instance;
            if (world == null)
                return;

            if (world.RegisteredPlayers.Count <= 0)
                return;

            if (_RefreshCacheTime < Time.time)
            {
                Player player = world
                    .RegisteredPlayers?
                    .OfType<Player>().Where(p => p.IsYourPlayer).Random();

                if(player == null || player.IsNullOrDestroyed())
                {
                    return;
                }

                _RefreshCacheTime = Time.time + CacheRefreshTime;
                CachedPOIs.Clear();

                if (LookExfilChance > 0)
                    ExfilPOIs.RefreshData(CachedPOIs, player);

                if (LookLootableChance > 0)
                    Containers.RefreshData(CachedPOIs, player);

                if (LookSpawnPointChance > 0)
                    SpawnZoneFinder.RefreshData(CachedPOIs, player);

                if (LookQuestsChance > 0)
                    QuestZoneFinder.RefreshData(CachedPOIs, player);
            }
        }
        public override void Execute()
        {
            RefreshData();
            AddConsoleLog($"Cache refreshed");
        }
    }
}
