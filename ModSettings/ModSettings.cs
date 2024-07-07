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

        // Main panel visibility and position (in pixels).
        // These are hidden because they are not directly adjustable by player.
        [SettingsUIHidden]
        public bool MainPanelVisible { get; set; }
        [SettingsUIHidden]
        public int MainPanelPositionX { get; set; }
        [SettingsUIHidden]
        public int MainPanelPositionY { get; set; }

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
                // Save default position.
                MainPanelPositionX = ModSettingsDefaults.MainPanelPositionX;
                MainPanelPositionY = ModSettingsDefaults.MainPanelPositionY;
                ApplyAndSave();

                // Move the main panel to the default position.
                Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<UISystem>().
                    SetMainPanelPosition(ModSettingsDefaults.MainPanelPositionX, ModSettingsDefaults.MainPanelPositionY);
            }
        }

        // Display mod version in settings.
        [SettingsUISection(GroupAbout)]
        public string ModVersion { get { return ModAssemblyInfo.Version; } }
    }
}
