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

        public void RefreshData(List<PointOfInterest> data, Player player)
        {
            var world = Singleton<GameWorld>.Instance;
            if (world == null)
                return;

            IEnumerable<BotZone> BotZones = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<BotZone>();

            foreach (var Zone in BotZones)
            {
                AddRecord($"SpawnZone({Zone.NameZone})", Zone.CenterOfSpawnPoints, data);
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
