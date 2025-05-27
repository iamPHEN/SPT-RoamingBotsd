using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using EFT.InputSystem;
using EFT.InventoryLogic;
using RoamingBots.Componets;
using RoamingBots.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;


#nullable enable

namespace RoamingBots.ConsoleCommands
{
#if DEBUG
    public class PlayerColor(Color color, Color borderColor, Color infoColor) 
    {
        public Color Color { get; set; } = color;
        public Color BorderColor { get; set; } = borderColor;
        public Color InfoColor { get; set; } = infoColor;
    }
    internal class DebugBotActions : ConsoleCommandWithoutArgument
    {
        public override string Name => nameof(DebugBotActions);
        public bool Enabled => RoamingBotsPlugin.DebugBotData.Value;
        public float BoxThickness { get; set; } = 2f;
        public class GameStateSnapshot
        {
            public Camera? Camera { get; set; }
            public Camera? MapCamera { get; set; }
            public Player? LocalPlayer { get; set; }
            public IEnumerable<Player> Hostiles { get; set; } = [];
            public bool MapMode { get; set; } = false;
        }
        public static GameStateSnapshot? Current { get; private set; }

        public float _lastUpdateTime = 0f;

        public void Update()
        {
            if(Enabled && _lastUpdateTime < Time.time)
            {
                RefreshData();
                _lastUpdateTime = Time.time + 4f;
            }
        }

        public static PlayerColor GetPlayerColors(Player player)
        {
            var hostileType = player.GetHostileType();
            return GetPlayerColors(hostileType);
        }

        public static PlayerColor GetPlayerColors(HostileType hostileType)
        {
            return hostileType switch
            {
                HostileType.Bear => new(Color.green, Color.green, Color.red),
                HostileType.Usec => new(Color.green, Color.green, Color.red),
                HostileType.Scav => new(Color.yellow, Color.yellow, Color.red),
                HostileType.Boss => new(Color.red, Color.red, Color.red),
                HostileType.Cultist => new(Color.blue, Color.red, Color.red),
                HostileType.ScavRaider => new(Color.yellow, Color.yellow, Color.red),
                HostileType.ScavAssault => new(Color.yellow, Color.yellow, Color.red),
                HostileType.Marksman => new(Color.yellow, Color.yellow, Color.red),
                HostileType.RogueUsec => new(Color.yellow, Color.yellow, Color.red),
                _ => new(Color.black, Color.black, Color.red),
            };
        }

        public void RefreshData()
        {
            var snapshot = new GameStateSnapshot();
            var world = Singleton<GameWorld>.Instance;

            if (world == null)
                return;

            var players = world
                .RegisteredPlayers?
                .OfType<Player>();

            if (players == null)
                return;

            var hostiles = new List<Player>();
            snapshot.Hostiles = hostiles;

            foreach (var player in players)
            {
                if (player.IsYourPlayer)
                {
                    snapshot.LocalPlayer = player;
                    continue;
                }

                if (!player.IsAlive())
                    continue;

                hostiles.Add(player);
            }

            snapshot.Camera = Camera.main;

            Current = snapshot;
        }

        [UsedImplicitly]
        protected void OnGUI()
        {
            var snapshot = Current;
            if (snapshot == null)
                return;

            if (snapshot.MapMode)
                return;

            var hostiles = snapshot.Hostiles;

            var Player = snapshot.LocalPlayer;
            if (Player == null)
                return;

            var camera = snapshot.Camera;
            if (camera == null)
                return;

            if (!Enabled)
            {
                return;
            }

            foreach (var enemy in hostiles)
            {
                if (enemy == null || !enemy.IsValid())
                    continue;

                var playerColors = GetPlayerColors(enemy);
                var borderColor = playerColors.BorderColor;

                var position = enemy.Transform.position;
                var screenPosition = camera.WorldPointToVisibleScreenPoint(position);
                if (screenPosition == Vector2.zero)
                    continue;


                var playerBones = enemy.PlayerBones;
                if (playerBones == null)
                    continue;

                var headScreenPosition = 
                    camera.WorldPointToVisibleScreenPoint(playerBones.Head.position);
                var leftShoulderScreenPosition =
                    camera.WorldPointToVisibleScreenPoint(playerBones.LeftShoulder.position);

                if (headScreenPosition == Vector2.zero || leftShoulderScreenPosition == Vector2.zero)
                    continue;

                var heightOffset = Mathf.Abs(headScreenPosition.y - leftShoulderScreenPosition.y);

                var boxHeight = Mathf.Abs(headScreenPosition.y - screenPosition.y) + heightOffset * 3f;
                var boxWidth = boxHeight * 0.62f;

                var boxPositionX = screenPosition.x - boxWidth / 2f;
                var boxPositionY = headScreenPosition.y - heightOffset * 2f;

                Render.DrawBox(boxPositionX, boxPositionY, boxWidth, boxHeight, BoxThickness, borderColor);

                BotComponent? bot = enemy?.GetComponent<BotComponent>();
                if (bot != null && bot?.MoveToPosition != null)
                {
                    var MoveToPos = camera.WorldPointToVisibleScreenPoint(bot.MoveToPosition.Value.Position);
                    Render.DrawLine(headScreenPosition, MoveToPos, 1, Color.yellow);
                    Render.DrawCircle(MoveToPos, bot.MinDistanceToExtract, Color.yellow, 1, 10);
                    //Gizmos.color = Color.yellow;
                    //Gizmos.DrawWireSphere(MoveToPos, bot.MinDistanceToExtract);
                }

                // Surpresses a nullable warning here.
                if(enemy != null)
                {
                    RenderDebugTextWindowData(enemy, boxPositionX, boxPositionY);
                }
            }
        }

        private void RenderDebugTextWindowData(Player Enemy, float boxPositionX, float boxPositionY)
        {

            var snapshot = Current;
            var camera = snapshot?.Camera;
            if (camera == null)
                return;

            if (Enemy == null || Enemy.AIData == null)
                return;

            var ennemyHealthController = Enemy.HealthController;
            var ennemyHandController = Enemy.HandsController;

            if (ennemyHealthController is not { IsAlive: true })
                return;

            var playerColors = GetPlayerColors(Enemy);
            var position = Enemy.Transform.position;


            var weaponText = ennemyHandController != null && ennemyHandController.Item is Weapon weapon ? weapon.ShortName.Localized() : string.Empty;
            var bodyPartHealth = ennemyHealthController.GetBodyPartHealth(EBodyPart.Common);
            var currentPlayerHealth = bodyPartHealth.Current;
            var maximumPlayerHealth = bodyPartHealth.Maximum;

            var distance = Mathf.Round(Vector3.Distance(camera.transform.position, position));
            var distanceText = string.Format("{0}", distance);

            var infoText = string.Format("{3}({4}) {0} {1}% [{2}]",
                    weaponText,
                    Mathf.Round(currentPlayerHealth * 100 / maximumPlayerHealth),
                    distanceText,
                    Enemy.AIData.BotOwner.name ?? Enemy.Profile.Nickname,
                    Enemy.AIData.BotOwner.Brain?.BaseBrain.ShortName() ?? "").Trim();

            BotComponent bot = Enemy.GetComponent<BotComponent>();
            if (bot != null && bot.BotOwner && Enemy)
            {
                ActorDataStruct Data = new ActorDataStruct(bot.BotOwner, 0f, Enemy);
                BotInfoDataPanel? DataPanel = Enemy.GetOrAddComponent<BotInfoDataPanel>();
                if(DataPanel != null)
                {
                    infoText += "\n" + DataPanel.GetInfoText(Data, EBotInfoMode.Behaviour, true);
                    infoText += DataPanel.GetInfoText(Data, EBotInfoMode.BattleState, true);
                    infoText += $"Goal: {bot.MoveToPosition?.Position.ToString() ?? "null"} ({bot.MoveToPosition?.Name ?? ""}) (Timeout: {bot.SprintTimeLeft.ToString("0.0")} s) CD: ({(Time.time - bot.SprintPOILastSprintAttemptCDTime).ToString("0.0")} s) ({Vector3.Distance(bot.MoveToPosition?.Position ?? Vector3.zero, bot?.BotOwner?.Transform.position ?? Vector3.zero).ToString("0.0")} m)";
                }
            }

            Render.DrawString(new Vector2(boxPositionX, boxPositionY - 20f), infoText, playerColors.InfoColor, false);
        }
        public override void Execute()
        {
            RoamingBotsPlugin.DebugBotData.Value = !Enabled;
            AddConsoleLog($"DebugBotAction Set to: {Enabled.ToString()}");
        }
    }
#endif //DEBUG
}
