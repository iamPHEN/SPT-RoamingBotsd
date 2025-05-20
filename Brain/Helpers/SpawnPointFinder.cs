using System.Collections.Generic;
using System.Linq;
using EFT.Interactive;
using EFT;
using RoamingBots.Helpers;
using UnityEngine;
using Comfort.Common;
using RoamingBots.Extensions;
using EFT.Game.Spawning;

namespace RoamingBots.Brain.Helpers
{
    public class SpawnPointFinder : POIFinder
    {
        private int _refreshCounter = 0;

        public void RefreshData(List<PointOfInterest> data, Player player)
        {
            var world = Singleton<GameWorld>.Instance;
            if (world == null)
                return;

            IEnumerable<BotZone> BotZones = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<BotZone>();

            foreach (var Zone in BotZones)
            {
                if(RoamingBotsPlugin.EnableBotSprintSpawnZonesRevist.Value)
                {
                    //Sprinkle in a counter so bots will likely revist this zone.
                    AddRecord($"SpawnZone({Zone.NameZone}_{_refreshCounter++})", Zone.CenterOfSpawnPoints, data);
                    if(_refreshCounter > 100)
                    {
                        _refreshCounter = 0;
                    }
                }
                else
                {
                    AddRecord($"SpawnZone({Zone.NameZone})", Zone.CenterOfSpawnPoints, data);
                }
                if (RoamingBotsPlugin.EnableBotSprintSpawnZones.Value)
                {
                    foreach (var Spawn in Zone.SpawnPoints)
                    {
                        AddRecord($"SpawnPoint({Zone.NameZone})", Spawn.Position, data);
                    }
                }
            }
        }

        private void AddRecord(string itemName, Vector3 position, List<PointOfInterest> records)
        {
            PointOfInterest POI = new();
            POI.Name = itemName;
            POI.Position = position;
            POI.Owner = nameof(SpawnPointFinder);

            records.Add(POI);
        }
    }
}
