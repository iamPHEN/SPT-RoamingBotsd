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
using RoamingBots.ConsoleCommands;

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

        //This can be expensive
        private bool TryFindPOIForBot(BotComponent bot)
        {
            if (bot.BotOwner == null)
                return false;

            var EliglbePois = POICache.GetRandomPoi()
                .Where(d => !Bot.ReachedPOI.Contains(d));

            PointOfInterest RandomPoi = EliglbePois.PickRandom();

            Bot.MoveToPosition = RandomPoi;

            if (RoamingBotsPlugin.DebugMode.Value)
            {
                RoamingBotsPlugin.LogSource.LogDebug($"{bot.BotOwner.name} SprintPOI head towards: {RandomPoi.Name} {RandomPoi.Position} in {EliglbePois.Count()}/{POICache.CachedPOIs.Count()} POIs: Dist: {Vector3.Distance(RandomPoi.Position, bot.BotOwner.Position)}");
            }

            return true;
        }

        public override bool IsCurrentActionEnding()
        {
            return Bot.MoveToPosition == null;
        }
    }
}
