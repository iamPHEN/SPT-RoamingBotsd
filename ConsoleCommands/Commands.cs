using System;
using System.Collections.Generic;
using System.Linq;
using EFT.UI;
using RoamingBots.Componets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RoamingBots.ConsoleCommands
{
    public class Commands
    {
        private bool Registered { get; set; } = false;
        public void RegisterCommands()
        {
            if (Registered)
                return;

            // Dynamically register commands
            int count = 0;
            foreach (var Command in GetCommands())
            {
                count++;
                Command.Register();
                HookObjectSatic.GetOrAddComponent(Command.GetType());
            }

            ConsoleScreen.Log($"Register ExfilBots called with {count} commands.");
            RoamingBotsPlugin.LogSource.LogDebug($"Register ExfilBots called with {count} commands.");

            // Load default configuration
            Registered = true;
            ConsoleScreen.IAmDevShowMeLogs = true;
        }

        private IEnumerable<ConsoleCommand> GetCommands()
        {
            var types = GetType()
                .Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ConsoleCommand)) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

            foreach (var type in types)
            {
                yield return (ConsoleCommand)Activator.CreateInstance(type);
            }
        }
    }
}
