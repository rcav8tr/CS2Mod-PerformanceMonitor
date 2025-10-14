using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Unity.Entities;

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

        // Other systems.
        private readonly UISystem _uiSystem;

        public ModSettings(IMod mod) : base(mod)
        {
            Mod.log.Info($"{nameof(ModSettings)}.{nameof(ModSettings)}");

            // Get other systems.
            _uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<UISystem>();

            // Set defaults.
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
        [SettingsUIHidden]
        public bool MainPanelVisible { get; set; }

        // Main panel position (in pixels).
        [SettingsUIHidden]
        public int MainPanelPositionX { get; set; }
        [SettingsUIHidden]
        public int MainPanelPositionY { get; set; }

        // Activation key binding.  Default is Ctrl+Shift+P.
        private ProxyBinding _activationKeyBinding;
        [SettingsUIKeyboardBinding(BindingKeyboard.P, ActivationKeyActionName, ctrl: true, shift: true)]
        [SettingsUISection(GroupGeneral)]
        public ProxyBinding ActivationKeyBinding
        {
            get { return _activationKeyBinding; }
            set { _activationKeyBinding = value; UpdateMainPanelUISettingsIfLoaded(); }
        }

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

                // Update main panel UI settings.
                UpdateMainPanelUISettingsIfLoaded();
            }
        }

        // Display mod version in settings.
        [SettingsUISection(GroupAbout)]
        public string ModVersion { get { return ModAssemblyInfo.Version; } }

        /// <summary>
        /// Update main panel settings in UI if settings are loaded.
        /// </summary>
        private void UpdateMainPanelUISettingsIfLoaded()
        {
            // Settings must be loaded.
            // This prevents updating UI while initial defaults are being set and while settings are loading.
            if (_loaded)
            {
                _uiSystem.UpdateMainPanelUISettings();
            }
        }
    }
}
