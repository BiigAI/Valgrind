using System;
using BepInEx.Configuration;
using Jotunn.Utils;
using Jotunn.Configs;
using UnityEngine;

namespace Valgrind.Configuration
{
    public enum CalculationMode
    {
        TieredBrackets,
        ContinuousCurve,
        PerSkill
    }

    public static class ModConfig
    {
        // ── General Settings ───────────────────────────────────────────────────
        public static ConfigEntry<CalculationMode> CalculationModeEntry { get; private set; }
        public static ConfigEntry<bool> UseTopNSkillsOnly { get; private set; }
        public static ConfigEntry<int> TopNSkillsCount { get; private set; }
        public static ConfigEntry<bool> ResetAccumulatorOnDeath { get; private set; }
        public static ConfigEntry<bool> EnableDebugLogging { get; private set; }

        // ── Tiered Brackets Settings ───────────────────────────────────────────
        public static ConfigEntry<float> EarlyGameLossPercent { get; private set; }
        public static ConfigEntry<float> MidGameLossPercent { get; private set; }
        public static ConfigEntry<float> LateGameLossPercent { get; private set; }
        public static ConfigEntry<float> EndgameLossPercent { get; private set; }

        // ── Continuous Curve Settings ──────────────────────────────────────────
        public static ConfigEntry<float> CurveMaxLossPercent { get; private set; }
        public static ConfigEntry<float> CurveMinLossPercent { get; private set; }

        public static void Initialize(ConfigFile config)
        {
            // ── Section 1: General ─────────────────────────────────────────────
            CalculationModeEntry = config.Bind(
                "1 - General",
                "CalculationMode",
                CalculationMode.TieredBrackets,
                new ConfigDescription(
                    "Method used to calculate dynamic skill loss on death:\n" +
                    "- TieredBrackets: Discrete loss % based on overall average skill level.\n" +
                    "- ContinuousCurve: Smooth linear/curved scaling between Max and Min loss % based on average skill.\n" +
                    "- PerSkill: Calculates loss percentage for each individual skill based on its own level.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            UseTopNSkillsOnly = config.Bind(
                "1 - General",
                "UseTopNSkillsOnly",
                false,
                new ConfigDescription(
                    "If true, average skill level is computed using only the player's top N highest skills (reflecting primary build). If false, averages all discovered skills (level > 0).",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            TopNSkillsCount = config.Bind(
                "1 - General",
                "TopNSkillsCount",
                5,
                new ConfigDescription(
                    "Number of top skills to factor into the average when UseTopNSkillsOnly is enabled.",
                    new AcceptableValueRange<int>(1, 20),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            ResetAccumulatorOnDeath = config.Bind(
                "1 - General",
                "ResetAccumulatorOnDeath",
                true,
                new ConfigDescription(
                    "If true, partial XP progress toward the next skill level is wiped to 0 on death (vanilla behavior). If false, progress toward the next level is preserved.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            EnableDebugLogging = config.Bind(
                "1 - General",
                "EnableDebugLogging",
                false,
                new ConfigDescription(
                    "Enable verbose logging of skill loss calculations to the BepInEx console.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = false }
                )
            );

            // ── Section 2: Tiered Brackets ─────────────────────────────────────
            EarlyGameLossPercent = config.Bind(
                "2 - Tiered Brackets",
                "EarlyGameLossPercent",
                8.0f,
                new ConfigDescription(
                    "Skill loss % for skill/average levels < 25. (Vanilla baseline is 5.0%)",
                    new AcceptableValueRange<float>(0f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            MidGameLossPercent = config.Bind(
                "2 - Tiered Brackets",
                "MidGameLossPercent",
                5.0f,
                new ConfigDescription(
                    "Skill loss % for skill/average levels between 25 and 50. (Vanilla baseline is 5.0%)",
                    new AcceptableValueRange<float>(0f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            LateGameLossPercent = config.Bind(
                "2 - Tiered Brackets",
                "LateGameLossPercent",
                2.5f,
                new ConfigDescription(
                    "Skill loss % for skill/average levels between 50 and 75.",
                    new AcceptableValueRange<float>(0f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            EndgameLossPercent = config.Bind(
                "2 - Tiered Brackets",
                "EndgameLossPercent",
                1.0f,
                new ConfigDescription(
                    "Skill loss % for skill/average levels > 75.",
                    new AcceptableValueRange<float>(0f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            // ── Section 3: Continuous Curve ────────────────────────────────────
            CurveMaxLossPercent = config.Bind(
                "3 - Continuous Curve",
                "CurveMaxLossPercent",
                8.0f,
                new ConfigDescription(
                    "Maximum skill loss percentage (applied at skill level 0).",
                    new AcceptableValueRange<float>(0f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );

            CurveMinLossPercent = config.Bind(
                "3 - Continuous Curve",
                "CurveMinLossPercent",
                1.0f,
                new ConfigDescription(
                    "Minimum skill loss percentage (applied at skill level 100).",
                    new AcceptableValueRange<float>(0f, 100f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }
                )
            );
        }
    }
}
