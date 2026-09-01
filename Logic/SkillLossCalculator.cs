using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valgrind.Configuration;

namespace Valgrind.Logic
{
    public static class SkillLossCalculator
    {
        /// <summary>
        /// Applies the configured dynamic death penalty to the provided Skills instance.
        /// </summary>
        public static void ApplyDeathPenalty(Skills skills)
        {
            if (skills == null)
            {
                return;
            }

            List<Skills.Skill> skillList = skills.GetSkillList();
            if (skillList == null || skillList.Count == 0)
            {
                return;
            }

            CalculationMode mode = ModConfig.CalculationModeEntry.Value;
            bool resetAccumulator = ModConfig.ResetAccumulatorOnDeath.Value;
            bool debug = ModConfig.EnableDebugLogging.Value;

            if (mode == CalculationMode.PerSkill)
            {
                ApplyPerSkillDeathPenalty(skillList, resetAccumulator, debug);
            }
            else
            {
                ApplyPlayerWideDeathPenalty(skillList, mode, resetAccumulator, debug);
            }
        }

        /// <summary>
        /// Calculates player-wide average skill level and applies a uniform dynamic multiplier to all skills.
        /// </summary>
        private static void ApplyPlayerWideDeathPenalty(List<Skills.Skill> skillList, CalculationMode mode, bool resetAccumulator, bool debug)
        {
            float avgLevel = CalculateAverageSkillLevel(skillList);
            float lossPercent;

            if (mode == CalculationMode.ContinuousCurve)
            {
                lossPercent = GetCurveLossPercent(avgLevel);
            }
            else // CalculationMode.TieredBrackets
            {
                lossPercent = GetTierLossPercent(avgLevel);
            }

            float multiplier = Mathf.Clamp01(1f - (lossPercent / 100f));

            if (debug)
            {
                ValgrindPlugin.Log.LogInfo(
                    $"[SkillLossCalculator] Mode={mode}, AvgLevel={avgLevel:F2}, LossPercent={lossPercent:F2}%, Multiplier={multiplier:F4}, ResetAccumulator={resetAccumulator}"
                );
            }

            foreach (Skills.Skill skill in skillList)
            {
                if (skill != null && skill.m_level > 0f)
                {
                    float oldLevel = skill.m_level;
                    skill.m_level = Mathf.Max(0f, skill.m_level * multiplier);

                    if (resetAccumulator)
                    {
                        skill.m_accumulator = 0f;
                    }

                    if (debug)
                    {
                        ValgrindPlugin.Log.LogInfo(
                            $"[SkillLossCalculator] Skill {skill.m_info?.m_skill.ToString() ?? "Unknown"}: {oldLevel:F2} -> {skill.m_level:F2}"
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates and reduces each skill independently based on its own level.
        /// </summary>
        private static void ApplyPerSkillDeathPenalty(List<Skills.Skill> skillList, bool resetAccumulator, bool debug)
        {
            if (debug)
            {
                ValgrindPlugin.Log.LogInfo($"[SkillLossCalculator] Applying Per-Skill death penalty. ResetAccumulator={resetAccumulator}");
            }

            foreach (Skills.Skill skill in skillList)
            {
                if (skill != null && skill.m_level > 0f)
                {
                    float lossPercent = GetTierLossPercent(skill.m_level);
                    float multiplier = Mathf.Clamp01(1f - (lossPercent / 100f));
                    float oldLevel = skill.m_level;

                    skill.m_level = Mathf.Max(0f, skill.m_level * multiplier);

                    if (resetAccumulator)
                    {
                        skill.m_accumulator = 0f;
                    }

                    if (debug)
                    {
                        ValgrindPlugin.Log.LogInfo(
                            $"[SkillLossCalculator] Skill {skill.m_info?.m_skill.ToString() ?? "Unknown"} (Level {oldLevel:F2}): Loss {lossPercent:F2}% -> New Level {skill.m_level:F2}"
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the player's average skill level across active/discovered skills or top N skills.
        /// </summary>
        public static float CalculateAverageSkillLevel(List<Skills.Skill> skillList)
        {
            if (skillList == null || skillList.Count == 0)
                return 0f;

            var activeLevels = new List<float>();

            foreach (Skills.Skill skill in skillList)
            {
                if (skill != null && skill.m_level > 0f)
                {
                    activeLevels.Add(skill.m_level);
                }
            }

            if (activeLevels.Count == 0)
                return 0f;

            if (ModConfig.UseTopNSkillsOnly.Value)
            {
                int topCount = Mathf.Clamp(ModConfig.TopNSkillsCount.Value, 1, activeLevels.Count);
                return activeLevels.OrderByDescending(l => l).Take(topCount).Average();
            }

            return activeLevels.Average();
        }

        /// <summary>
        /// Returns the loss percentage for a given skill level based on configured discrete brackets.
        /// </summary>
        public static float GetTierLossPercent(float level)
        {
            if (level < 25f)
            {
                return Mathf.Clamp(ModConfig.EarlyGameLossPercent.Value, 0f, 100f);
            }
            if (level < 50f)
            {
                return Mathf.Clamp(ModConfig.MidGameLossPercent.Value, 0f, 100f);
            }
            if (level <= 75f)
            {
                return Mathf.Clamp(ModConfig.LateGameLossPercent.Value, 0f, 100f);
            }

            return Mathf.Clamp(ModConfig.EndgameLossPercent.Value, 0f, 100f);
        }

        /// <summary>
        /// Returns the loss percentage along a continuous linear/curve between Max and Min loss.
        /// </summary>
        public static float GetCurveLossPercent(float level)
        {
            float maxLoss = Mathf.Clamp(ModConfig.CurveMaxLossPercent.Value, 0f, 100f);
            float minLoss = Mathf.Clamp(ModConfig.CurveMinLossPercent.Value, 0f, 100f);

            // Normalized level between 0 and 100
            float t = Mathf.Clamp01(level / 100f);

            // Smooth linear interpolation from MaxLoss at level 0 down to MinLoss at level 100
            return Mathf.Lerp(maxLoss, minLoss, t);
        }
    }
}
