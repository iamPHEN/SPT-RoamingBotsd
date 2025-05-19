using System.Text;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using RoamingBots;
using UnityEngine;
using UnityEngine.AI;

namespace ExilBots.Layers
{
    internal class SprintToPOIAction : CombatAction
    {

        static SprintToPOIAction()
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public SprintToPOIAction(BotOwner bot) : base(bot, "SprintToPOI")
        {
        }

        public override void Start()
        {
            Toggle(true);
            Bot.SprintPOILastSprintAttemptCDTime = Time.time + Bot.SprintPOICoolDown;

            float TimeoutTime = RoamingBotsPlugin.SprintToTimeout.Value;
            if (TimeoutTime > 0)
                Bot.SprintTimeLeft = RoamingBotsPlugin.SprintToTimeout.Value;
            else
                Bot.SprintTimeLeft = 99999f; //Pratically the entire raid.
        }

        public override void Stop()
        {
            Toggle(false);
            Bot.SprintPOILastSprintAttemptCDTime = Time.time + Bot.SprintPOICoolDown;
        }

        public override void Update(CustomLayer.ActionData data)
        {
            if (BotOwner == null)
                return;

            Vector3 point = (Vector3)Bot.MoveToPosition.Value.Position;
            float distance = (point - BotOwner.Position).sqrMagnitude;

            if (distance < Bot.SprintPOIMinDistanceToConsiderVistedPOI)
            {
                if (RoamingBotsPlugin.DebugMode.Value)
                {
                    RoamingBotsPlugin.LogSource.LogDebug($"{Bot.BotOwner.name} SprintPOI reached their goal End!");
                }
                Bot.ReachedPOI.Add(Bot.MoveToPosition.Value);
                Bot.MoveToPosition = null;
            }

            float stamina = BotOwner.GetPlayer.Physical.Stamina.NormalValue;
            // Environment id of 0 means a bot is outside.
            if (BotOwner.AIData.EnvironmentId != 0)
            {
                shallSprint = false;
            }
            else if (stamina > 0.75f)
            {
                shallSprint = true;
            }
            else if (stamina < 0.2f)
            {
                shallSprint = false;
            }

            if (!BotOwner.GetPlayer.MovementContext.CanSprint)
            {
                shallSprint = false;
            }

            Bot.SprintTimeLeft -= Time.deltaTime;

            if (Bot.SprintTimeLeft <= 0f)
            {
                if (RoamingBotsPlugin.DebugMode.Value)
                {
                    RoamingBotsPlugin.LogSource.LogDebug($"{Bot.BotOwner.name} Cancled thier goal due to timeout.");
                }
                Bot.ReachedPOI.Add(Bot.MoveToPosition.Value);
                Bot.MoveToPosition = null;
            }

            if (distance < 8f)
            {
                shallSprint = false;
            }

            
            SprintToPoint(distance, point);
            BotOwner.Mover.SetPose(1f);
            BotOwner.Mover.SetTargetMoveSpeed(1f);
        }

        private bool shallSprint;

        public float ReCalcPathTimer { get; private set; }

        private void SprintToPoint(float distance, Vector3 point)
        {
            if (BotOwner.Mover == null)
            {
                return;
            }

            BotOwner.Mover.Sprint(shallSprint);

            if (ReCalcPathTimer < Time.time)
            {
                ReCalcPathTimer = Time.time + 4f;
                NavMeshPathStatus pathStatus = BotOwner.Mover.GoToPoint(point, true, -1, false, false);
                var pathController = BotOwner.Mover._pathController;
                if (pathController?.CurPath != null)
                {
                    float distanceToEndOfPath = Vector3.Distance(BotOwner.Position, pathController.CurPath.LastCorner());
                    bool reachedEndOfIncompletePath = (pathStatus == NavMeshPathStatus.PathPartial) && (distanceToEndOfPath < Bot.SprintPOIMinDistanceToConsiderVistedPOI);

                    // We got there!
                    if ((pathStatus == NavMeshPathStatus.PathInvalid) || reachedEndOfIncompletePath)
                    {
                        if (RoamingBotsPlugin.DebugMode.Value)
                        {
                            RoamingBotsPlugin.LogSource.LogDebug($"{Bot.BotOwner.name} SprintPOI reached their goal End?({reachedEndOfIncompletePath}), PathStatus: {pathStatus.ToString()}.");
                        }
                        Bot.ReachedPOI.Add(Bot.MoveToPosition.Value);
                        Bot.MoveToPosition = null;
                    }
                }
                else
                {
                    if (RoamingBotsPlugin.DebugMode.Value)
                    {
                        RoamingBotsPlugin.LogSource.LogDebug($"{Bot.BotOwner.name} SprintPOI reached their goal or at least gave up going there. PathStatus: {pathStatus.ToString()}.");
                    }
                    Bot.ReachedPOI.Add(Bot.MoveToPosition.Value);
                    Bot.MoveToPosition = null;
                }
            }
        }
        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Some Data: Thing");
            stringBuilder.AppendLabeledValue("Label", "Data", Color.yellow, Color.red, true);
        }
    }
}