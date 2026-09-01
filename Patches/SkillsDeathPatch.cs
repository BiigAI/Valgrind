using System;
using HarmonyLib;
using Valgrind.Logic;

namespace Valgrind.Patches
{
    [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
    public static class SkillsDeathPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Skills __instance)
        {
            if (__instance == null)
            {
                // Fall back to vanilla if skills instance is unexpectedly null
                return true;
            }

            try
            {
                SkillLossCalculator.ApplyDeathPenalty(__instance);
                // Suppress vanilla hardcoded 5% skill loss execution
                return false;
            }
            catch (Exception ex)
            {
                ValgrindPlugin.Log.LogError($"[Valgrind] Error applying dynamic death penalty: {ex}");
                // In case of an unexpected exception, fall back to vanilla to prevent game-breaking deadlocks
                return true;
            }
        }
    }
}
