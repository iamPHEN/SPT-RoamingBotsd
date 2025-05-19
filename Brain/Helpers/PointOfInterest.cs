using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace RoamingBots.Brain.Helpers
{
    public struct PointOfInterest
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public Vector3 Position { get; set; }

        public override int GetHashCode()
        {
            // Allow overflow
            unchecked
            {
                int hash = 17;
                hash = hash * 11 + Name.GetHashCode();
                hash = hash * 19 + (Owner?.GetHashCode() ?? 0);
                hash = hash * 97 + Position.GetHashCode();
                return hash;
            }
        }
    }

    public interface POIFinder
    {
        public void RefreshData(List<PointOfInterest> Data, Player Player);
    }
}
