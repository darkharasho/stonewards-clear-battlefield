using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ClearBattlefield.Patches;
using HarmonyLib;

namespace ClearBattlefield
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.darkharasho.stonewards.clearbattlefield";
        public const string PluginName = "ClearBattlefield";
        public const string PluginVersion = "0.2.0";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> RequireConfirmation;
        internal static ConfigEntry<KeyboardShortcut> ClearKey;
        internal static ConfigEntry<float> MinDropAgeSeconds;
        internal static ConfigEntry<bool> ClearScrap;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            // Display names and order are for the in-game ModSettings menu; they don't change the .cfg.
            RequireConfirmation = Config.Bind("General", "RequireConfirmation", true, new ConfigDescription(
                "Ask before removing anything. Applies to the pause menu button and the keybind.", null,
                new ConfigurationManagerAttributes { DispName = "Ask for confirmation", Order = 20 }));
            MinDropAgeSeconds = Config.Bind("General", "MinDropAgeSeconds", 0f, new ConfigDescription(
                "Enemy drops more recent than this many seconds are left on the ground. 0 clears every enemy drop. Scrap ignores this.",
                new AcceptableValueRange<float>(0f, 600f),
                new ConfigurationManagerAttributes { DispName = "Minimum drop age (seconds)", Order = 10 }));
            ClearScrap = Config.Bind("General", "ClearScrap", false, new ConfigDescription(
                "Also remove scrap (such as scrap wood from digging) that is still on the ground. It could have come from any player's digging.", null,
                new ConfigurationManagerAttributes { DispName = "Also clear scrap", Order = 5 }));
            ClearKey = Config.Bind("Controls", "ClearKey", KeyboardShortcut.Empty, new ConfigDescription(
                "Key that clears the battlefield. With confirmation on, it opens the pause menu on the confirmation. Host only; none by default.", null,
                new ConfigurationManagerAttributes { DispName = "Clear battlefield" }));

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo($"{PluginName} {PluginVersion} loaded. Only the host needs it.");
        }

        private void Update()
        {
            if (Hotkey.WasPressed(ClearKey.Value))
                PauseMenuButton.OnHotkey();
            PauseMenuButton.Update();
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
