namespace RoamingBots.ConsoleCommands
{
#if DEBUG
    internal class ForceBotSprintPOI : ConsoleCommandWithoutArgument
    {
        public override string Name => "ForceBotSprintPOI";

        public override void Execute()
        {
            bool NewValue = !RoamingBotsPlugin.ForceSprint.Value;
            RoamingBotsPlugin.ForceSprint.Value = NewValue;
            AddConsoleLog($"Bot ForceBotSprintPOI Set to: {NewValue.ToString()}");
        }
    }
#endif //DEBUG
}
