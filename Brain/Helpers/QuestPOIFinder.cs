using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using RoamingBots.Helpers;
using UnityEngine;

namespace RoamingBots.Brain.Helpers
{
    internal class QuestPOIFinder
    {
        public void RefreshData(List<PointOfInterest> Data, Player Player)
        {
            var World = Singleton<GameWorld>.Instance;
            if (World == null)
                return;

            IEnumerable<TriggerWithId> ItemTriggers = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<PlaceItemTrigger>();
            IEnumerable<TriggerWithId> ExpereinceTriggers = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<ExperienceTrigger>();

            foreach (var Zone in ItemTriggers)
            {
                AddRecord(Zone.name, Zone.transform.position, Data);
            }

            foreach (var Zone in ExpereinceTriggers)
            {
                AddRecord(Zone.name, Zone.transform.position, Data);
            }
        }

        private void AddRecord(string itemName, Vector3 position, List<PointOfInterest> records)
        {
            PointOfInterest POI = new();
            POI.Name = itemName;
            POI.Position = position;
            POI.Owner = nameof(QuestPOIFinder);

            records.Add(POI);
        }
    }
}
