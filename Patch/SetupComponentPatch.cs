using System.Reflection;
using EFT;
using RoamingBots.Componets;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace RoamingBots.Patch
{
    internal class SetupComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.Init));
        }

        [PatchPostfix]
        private static void PatchPostfix(BotsController __instance)
        {
            __instance.BotSpawner.OnBotRemoved += botOwner =>
            {
                if (botOwner.GetPlayer.TryGetComponent<BotComponent>(out var component))
                {
                    UnityEngine.Object.Destroy(component);
                }
            };
        }
    }
}

