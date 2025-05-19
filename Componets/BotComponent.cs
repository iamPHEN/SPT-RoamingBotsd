using System;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using RoamingBots.Brain.Helpers;
using RoamingBots.Extensions;
using RoamingBots.Helpers;
using ExilBots.Layers;
using UnityEngine;

namespace RoamingBots.Componets
{
    public class BotComponent : MonoBehaviour
    {
        public BotOwner BotOwner { get; internal set; }

        // List of POIs the bot shouldn't revist.
        public HashSet<PointOfInterest> ReachedPOI = new();
        public PointOfInterest? MoveToPosition { get; set; } = null;

        public float SprintPOICoolDown => RoamingBotsPlugin.SprintCoolDownSecs.Value;

        public float SprintPOILastSprintAttemptCDTime = 0f;
        public float SprintPOIMinDistanceToConsiderVistedPOI => RoamingBotsPlugin.SprintToMinDistanceVisit.Value;
        public bool SprintPOIHasSprintedRecently => Time.time < SprintPOILastSprintAttemptCDTime;


        // Exfil Varaibles
        public float MinDistanceToExtract { get; set; } = 10f;

        public void Init(BotOwner botOwner)
        {
            BotOwner = botOwner;
        }
        public void LogExtractionOfBot(BotOwner bot, Vector3 point, string reason, ExfiltrationPoint exfil)
        {
            RoamingBotsPlugin.LogSource.LogInfo($"{bot.name} Extracted because {reason} at {point} for extract {exfil.Settings.Name} at {DateTime.UtcNow}");
        }

        public bool IsBotAllowedToExfil()
        {
            if (BotOwner == null)
                return false;

            WildSpawnType botType = BotOwner.Profile.Info.Settings.Role;
            if (!BotTypeUtils.IsPMC(botType) && !BotTypeUtils.IsScav(botType))
            {
                return false;
            }
            return true && IsBotAlive;
        }

        public bool IsInCombat => BotOwner != null && (BotOwner.Memory.IsUnderFire || HasActiveThreat);
        public bool HasActiveThreat => BotOwner != null && !(BotOwner.Memory.HaveEnemy || (Time.time - BotOwner.Memory.LastEnemyTimeSeen) > 30f);
        public bool IsBotAlive => BotOwner != null && BotOwner.GetPlayer.IsAlive() && BotOwner.BotState == EBotState.Active;
        public bool IsBotHealing => BotOwner != null && BotOwner.Medecine.FirstAid.Have2Do && BotOwner.Medecine.SurgicalKit.HaveWork;
        public float SprintTimeLeft { get; set; }
    }
}