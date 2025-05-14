using System.Text.RegularExpressions;
using EFT.InputSystem;
using EFT.UI;

namespace RoamingBots.ConsoleCommands
{
    internal abstract class ConsoleCommand : InputNode
    {
        public abstract string Name { get; }

        internal void AddConsoleLog(string log)
        {
            if (PreloaderUI.Instantiated)
            {
                ConsoleScreen.Log(log);
                RoamingBotsPlugin.LogSource.LogInfo(log);
            }
        }

        public abstract void Register();
        public override ETranslateResult TranslateCommand(ECommand command)
        {
            return ETranslateResult.Ignore;
        }


        public override void TranslateAxes(ref float[] axes)
        {
        }


        public override ECursorResult ShouldLockCursor()
        {
            return ECursorResult.Ignore;
        }
    }


    internal abstract class ConsoleCommandWithoutArgument : ConsoleCommand
    {
        public abstract void Execute();

        public override void Register()
        {
            AddConsoleLog(string.Format("Registering {0} command...", Name));
            ConsoleScreen.Processor.RegisterCommand(Name, Execute);
        }
    }

    internal abstract class ConsoleCommandWithArgument : ConsoleCommand
    {
        public abstract string Pattern { get; }

        public abstract void Execute(Match match);

        protected const string ValueGroup = "value";
        protected const string ExtraGroup = "extra";

        protected const string RequiredArgumentPattern = $"(?<{ValueGroup}>.+)";
        protected const string OptionalArgumentPattern = $"(?<{ValueGroup}>.*)";

        public override void Register()
        {
            AddConsoleLog(string.Format("Registering {0} command with arguments...", Name));
            ConsoleScreen.Processor.RegisterCommand(Name, (string args) =>
            {
                var regex = new Regex("^" + Pattern + "$");
                if (regex.IsMatch(args))
                {
                    Execute(regex.Match(args));
                }
                else
                {
                    AddConsoleLog("Invalid arguments".Red());
                }
            });
        }
    }
}


