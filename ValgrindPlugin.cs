using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace Valgrind
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ValgrindPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bigai.valgrind";
        public const string PluginName = "Valgrind";
        public const string PluginVersion = "1.0.0";

        private static ConfigEntry<float> configEarlyGameLossPercent;
        private static ConfigEntry<float> configMidGameLossPercent;
        private static ConfigEntry<float> configLateGameLossPercent;
        private static ConfigEntry<float> configEndgameLossPercent;

        private readonly Harmony harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            // Set up config bindings under [DynamicDeathPenalty] section
            configEarlyGameLossPercent = Config.Bind(
                "DynamicDeathPenalty",
                "EarlyGameLossPercent",
                8.0f,
                "Skill loss percentage for players with an average skill level < 25. Default is 8.0% (Retain 0.92f)"
            );

            configMidGameLossPercent = Config.Bind(
                "DynamicDeathPenalty",
                "MidGameLossPercent",
                5.0f,
                "Skill loss percentage for players with an average skill level between 25 and 50. Default is 5.0% (Retain 0.95f - Vanilla baseline)"
            );

            configLateGameLossPercent = Config.Bind(
                "DynamicDeathPenalty",
                "LateGameLossPercent",
                2.5f,
                "Skill loss percentage for players with an average skill level between 50 and 75. Default is 2.5% (Retain 0.975f)"
            );

            configEndgameLossPercent = Config.Bind(
                "DynamicDeathPenalty",
                "EndgameLossPercent",
                1.0f,
                "Skill loss percentage for players with an average skill level > 75. Default is 1.0% (Retain 0.99f)"
            );

            // Apply Harmony patches
            harmony.PatchAll();
            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded successfully.");
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        public static class Patch_Skills_OnDeath
        {
            [HarmonyPrefix]
            public static bool Prefix(Skills __instance)
            {
                // Ensure the patch only executes on the server process
                if (ZNet.instance == null || !ZNet.instance.IsServer())
                {
                    // If not running on the server process, let vanilla (or other client-side patches) run
                    return true;
                }

                // Safety check: ensure m_skillData exists
                if (__instance.m_skillData == null)
                {
                    return true;
                }

                // 1. Calculate the player's average skill level across all active skills (level > 0)
                float totalSkillsLevel = 0f;
                int activeSkillsCount = 0;

                foreach (KeyValuePair<Skills.SkillType, Skills.Skill> kvp in __instance.m_skillData)
                {
                    Skills.Skill skill = kvp.Value;
                    if (skill != null && skill.m_level > 0f)
                    {
                        totalSkillsLevel += skill.m_level;
                        activeSkillsCount++;
                    }
                }

                float avgLevel = activeSkillsCount > 0 ? (totalSkillsLevel / (float)activeSkillsCount) : 0f;

                // 2. Determine target loss percentage from config settings
                float lossPercent;

                if (avgLevel < 25f)
                {
                    lossPercent = Mathf.Clamp(configEarlyGameLossPercent.Value, 0f, 100f);
                }
                else if (avgLevel < 50f)
                {
                    lossPercent = Mathf.Clamp(configMidGameLossPercent.Value, 0f, 100f);
                }
                else if (avgLevel <= 75f)
                {
                    lossPercent = Mathf.Clamp(configLateGameLossPercent.Value, 0f, 100f);
                }
                else
                {
                    lossPercent = Mathf.Clamp(configEndgameLossPercent.Value, 0f, 100f);
                }

                // Convert configured percentage to a reduction multiplier (e.g. 5.0% loss -> 0.95f multiplier)
                float multiplier = 1f - (lossPercent / 100f);

                // 3. Reduce skill levels dynamically while resetting m_accumulator to 0 (matching vanilla behavior)
                foreach (KeyValuePair<Skills.SkillType, Skills.Skill> kvp in __instance.m_skillData)
                {
                    Skills.Skill skill = kvp.Value;
                    if (skill != null)
                    {
                        skill.m_level = Mathf.Max(0f, skill.m_level * multiplier);
                        skill.m_accumulator = 0f;
                    }
                }

                // 4. Return false to suppress vanilla's hardcoded 5% execution
                return false;
            }
        }
    }
}
