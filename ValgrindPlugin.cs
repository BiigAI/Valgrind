using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using Valgrind.Configuration;

namespace Valgrind
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class ValgrindPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bigai.valgrind";
        public const string PluginName = "Valgrind";
        public const string PluginVersion = "1.1.0";

        public static ValgrindPlugin Instance { get; private set; } = null!;
        public static ManualLogSource Log { get; private set; } = null!;

        private Harmony? _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            try
            {
                Log.LogInfo($"══════════════════════════════════════════");
                Log.LogInfo($"  {PluginName} v{PluginVersion} loading...");
                Log.LogInfo($"══════════════════════════════════════════");

                // 1. Initialize server-synced configuration
                ModConfig.Initialize(Config);

                // 2. Apply Harmony patches
                _harmony = new Harmony(PluginGUID);
                _harmony.PatchAll(Assembly.GetExecutingAssembly());

                Log.LogInfo($"[{PluginName}] All Harmony patches applied successfully.");
                Log.LogInfo($"[{PluginName}] Ready. Dynamic death penalty active with Jötunn ServerSync.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Failed to initialize: {ex}");
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Log.LogInfo($"[{PluginName}] Unloaded.");
        }
    }
}
