using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using RoamingBots.Brain.Helpers;
using UnityEngine;

#nullable enable

namespace RoamingBots.Helpers
{
    public class ExfilFinder : POIFinder
    {
        public bool ShowEligible { get; set; } = RoamingBotsPlugin.EnableBotSprintPlayerExfils.Value;
        public bool ShowNotEligible { get; set; } = RoamingBotsPlugin.EnableBotSprintScavExfils.Value;

        public void RefreshData(List<PointOfInterest> OutData, Player Player)
        {
            GameWorld world = Singleton<GameWorld>.Instance;
            if (world == null)
                return;

            if (world.ExfiltrationController == null)
                return;

            Profile profile = Player.Profile;
            InfoClass? info = profile?.Info;
            if (info == null)
                return;

            EPlayerSide side = info.Side;
            ExfiltrationPoint[]? points = GetExfiltrationPoints(side, world);
            if (points == null)
                return;

            ExfiltrationPoint[]? eligiblePoints = GetEligibleExfiltrationPoints(side, world, profile!);
            if (eligiblePoints == null)
                return;

            foreach (var point in points)
            {
                if (point.IsNullOrDestroyed())
                    continue;

                Vector3 position = point.transform.position;
                bool isEligible = eligiblePoints.Contains(point);

                if (!ShowEligible && isEligible)
                    continue;

                if (!ShowNotEligible && !isEligible)
                    continue;

                PointOfInterest POI = new()
                {
                    Name = GetName(point, isEligible),
                    Position = position,
                    Owner = nameof(ExfilFinder)
                };

                OutData.Add(POI);
            }
        }

        private static string GetName(ExfiltrationPoint point, bool IsEligible)
        {
            var localizedName = point.Settings.Name.Localized();
            return !IsEligible ? localizedName : string.Format("{0} ({1})", localizedName, GetStatus(point.Status));
        }

        public static string GetStatus(EExfiltrationStatus status)
        {
            return status switch
            {
                EExfiltrationStatus.AwaitsManualActivation => EExfiltrationStatus.AwaitsManualActivation.ToString(),
                EExfiltrationStatus.Countdown => EExfiltrationStatus.Countdown.ToString(),
                EExfiltrationStatus.NotPresent => EExfiltrationStatus.NotPresent.ToString(),
                EExfiltrationStatus.Pending => EExfiltrationStatus.Pending.ToString(),
                EExfiltrationStatus.RegularMode => EExfiltrationStatus.RegularMode.ToString(),
                EExfiltrationStatus.UncompleteRequirements => EExfiltrationStatus.UncompleteRequirements.ToString(),
                _ => string.Empty
            };
        }

        public static ExfiltrationPoint[]? GetExfiltrationPoints(EPlayerSide side, GameWorld world)
        {
            ExfiltrationControllerClass ect = world.ExfiltrationController;
            // ReSharper disable once CoVariantArrayConversion
            return side == EPlayerSide.Savage ? ect.ScavExfiltrationPoints : ect.ExfiltrationPoints;
        }

        public static ExfiltrationPoint[]? GetEligibleExfiltrationPoints(EPlayerSide side, GameWorld world, Profile profile)
        {
            ExfiltrationControllerClass ect = world.ExfiltrationController;
            if (side != EPlayerSide.Savage)
                return ect.EligiblePoints(profile);

            int mask = ect.GetScavExfiltrationMask(profile.Id);
            List<ExfiltrationPoint> result = new();
            ScavExfiltrationPoint[] points = ect.ScavExfiltrationPoints;

            for (int i = 0; i < 31; i++)
            {
                if ((mask & (1 << i)) != 0)
                    result.Add(points[i]);
            }

            return [.. result];
        }
    }

}
