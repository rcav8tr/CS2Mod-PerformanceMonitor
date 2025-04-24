using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;

namespace PerformanceMonitor
{
    /// <summary>
    /// Settings for this mod.
    /// </summary>
    [FileLocation(nameof(PerformanceMonitor))]
    [SettingsUIGroupOrder(GroupGeneral, GroupAbout)]
    [SettingsUIShowGroupName(GroupGeneral, GroupAbout)]
    [SettingsUIKeyboardAction(ActivationKeyActionName)]
    public class ModSettings : ModSetting
    {
        // Group constants.
        public const string GroupGeneral = "General";
        public const string GroupAbout = "About";

        // Activation key binding action name.
        public const string ActivationKeyActionName = "ActivationKeyBinding";

        // Whether or not settings are loaded.
        private bool _loaded = false;

        public ModSettings(IMod mod) : base(mod)
        {
            LogUtil.Info($"{nameof(ModSettings)}.{nameof(ModSettings)}");

            SetDefaults();
        }
        
        /// <summary>
        /// Set a default value for every setting that has a value that can change.
        /// </summary>
        public override void SetDefaults()
        {
            // It is important to set a default for every value.
            MainPanelVisible   = ModSettingsDefaults.MainPanelVisible;
            MainPanelPositionX = ModSettingsDefaults.MainPanelPositionX;
            MainPanelPositionY = ModSettingsDefaults.MainPanelPositionY;
        }

        /// <summary>
        /// Set loaded flag.
        /// </summary>
        public void Loaded()
        {
            _loaded = true;
        }

        // Main panel visibility.
        // When this hidden setting changes, need to explicitly save settings.
        private bool _mainPanelVisible = ModSettingsDefaults.MainPanelVisible;
        [SettingsUIHidden]
        public bool MainPanelVisible
        {
            get { return _mainPanelVisible; }
            set { _mainPanelVisible = value; SaveSettings(); }
        }

        // Main panel X position (in pixels).
        // When this hidden setting changes, need to explicitly save settings.
        private int _mainPanelPositionX = ModSettingsDefaults.MainPanelPositionX;
        [SettingsUIHidden]
        public int MainPanelPositionX
        {
            get { return _mainPanelPositionX; }
            set { _mainPanelPositionX = value; SaveSettings(); }
        }

        // Main panel Y position (in pixels).
        // When this hidden setting changes, need to explicitly save settings.
        private int _mainPanelPositionY = ModSettingsDefaults.MainPanelPositionX;
        [SettingsUIHidden]
        public int MainPanelPositionY
        {
            get { return _mainPanelPositionY; }
            set { _mainPanelPositionY = value; SaveSettings(); }
        }

        // Activation key binding.  Default is Ctrl+Shift+P.
        [SettingsUIKeyboardBinding(BindingKeyboard.P, ActivationKeyActionName, ctrl: true, shift: true)]
        [SettingsUISection(GroupGeneral)]
        public ProxyBinding ActivationKeyBinding { get; set; }

        // Button to reset main panel position.
        [SettingsUIButton()]
        [SettingsUISection(GroupGeneral)]
        public bool ResetMainPanelPosition
        {
            set
            {
                // Set default position.
                MainPanelPositionX = ModSettingsDefaults.MainPanelPositionX;
                MainPanelPositionY = ModSettingsDefaults.MainPanelPositionY;

                // Move the main panel to the default position.
                Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<UISystem>().
                    SetMainPanelPosition(ModSettingsDefaults.MainPanelPositionX, ModSettingsDefaults.MainPanelPositionY);
            }
        }

        // Display mod version in settings.
        [SettingsUISection(GroupAbout)]
        public string ModVersion { get { return ModAssemblyInfo.Version; } }

        /// <summary>
        /// Save all settings.
        /// </summary>
        private async void SaveSettings()
        {
            // Settings must be loaded.
            // This prevents saving settings while defaults are being set and while settings are loading.
            if (_loaded)
            {
                // This saves settings for the game and all mods.
                await AssetDatabase.global.SaveSettings();
            }
        }
    }
}
