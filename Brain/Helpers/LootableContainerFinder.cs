using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT.Interactive;
using EFT;
using RoamingBots.Helpers;
using UnityEngine;
using Comfort.Common;
using RoamingBots.Extensions;
using EFT.InventoryLogic;

namespace RoamingBots.Brain.Helpers
{
    internal class LootableContainerFinder : POIFinder
    {
        public bool ShowContainers { get; set; } = RoamingBotsPlugin.EnableBotSprintStaticSpawn.Value;

        public bool ShowCorpses { get; set; } = RoamingBotsPlugin.EnableBotSprintCorpses.Value;
        public bool ShowLooseLoot { get; set; } = RoamingBotsPlugin.EnableBotSprintLooseLoot.Value;

        //Static Hashset for List of Container IDS
        private static readonly HashSet<string> ContainerFilter =
        [
            KnownTemplateIds.BuriedBarrelCache,
            KnownTemplateIds.GroundCache,
            KnownTemplateIds.AirDropCommon,
            KnownTemplateIds.AirDropMedical,
            KnownTemplateIds.AirDropSupply,
            KnownTemplateIds.AirDropWeapon
        ];

        public void RefreshData(List<PointOfInterest> Data, Player Player)
        {
            var World = Singleton<GameWorld>.Instance;
            if (World == null)
                return;

            if (!Player.IsValid())
                return;

            var Owners = World.ItemOwners;
            
            // Static containers
            foreach (var Owner in Owners)
            {
                var itemOwner = Owner.Key;
                var rootItem = itemOwner.RootItem;

                if (!rootItem.IsValid())
                    continue;

                if (rootItem is not { IsContainer: true })
                    continue;

                if (ShowContainers && ContainerFilter.Contains(rootItem.TemplateId))
                    AddRecord(rootItem.TemplateId.LocalizedShortName(), Owner.Value.Transform.position, Data);

                if (ShowCorpses && rootItem.TemplateId == KnownTemplateIds.DefaultInventory
                                && itemOwner is TraderControllerClass { Name: nameof(Corpse) }) // only display dead bodies
                    AddRecord(nameof(Corpse), Owner.Value.Transform.position, Data);
            }

            if (!ShowLooseLoot)
                return;

            // Outside containers and loose loot (tooboxes, tech cratates, ammo crates, etc)
            var lootItems = World.LootItems;
            for (var i = 0; i < lootItems.Count; i++)
            {
                var lootItem = lootItems.GetByIndex(i);
                if (lootItem.ItemOwner != null)
                {
                    AddRecord(lootItem.ItemOwner.ContainerName, lootItem.transform.position, Data);
                }
            }
        }

        private void AddRecord(string itemName, Vector3 position, List<PointOfInterest> records)
        {
            PointOfInterest POI = new();
            POI.Name = itemName;
            POI.Position = position;
            POI.Owner = nameof(LootableContainerFinder);

            records.Add(POI);
        }
    }
}
