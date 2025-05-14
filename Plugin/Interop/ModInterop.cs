using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Bootstrap;

namespace RoamingBots.Interop
{
    static class ModInterop
    {
        public const string LootingBotsKey = "me.skwizzy.lootingbots";
        public const string QuestingBotsKey = "com.DanW.QuestingBots";
        public const string BigBrain = "xyz.drakia.bigbrain";

        public static bool IsModLoaded(string ModKey)
        {
            return Chainloader.PluginInfos.ContainsKey(ModKey);
        }
    }
}
