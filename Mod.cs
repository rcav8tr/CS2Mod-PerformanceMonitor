using Colossal.IO.AssetDatabase;
using Game;
using Game.Modding;
using Game.SceneFlow;
using System;

namespace PerformanceMonitor
{
    /// <summary>
    /// The mod.
    /// </summary>
    public class Mod : IMod
    {
        // The one and only global settings for this mod.
        public static ModSettings ModSettings;

        // Whether or not to show data values.
        public static bool ShowGPUUsage    = false;
        public static bool ShowCPUUsage    = false;
        public static bool ShowMemoryUsage = false;
        
        /// <summary>
        /// One-time mod loading.
        /// </summary>
        public void OnLoad(UpdateSystem updateSystem)
        {
            LogUtil.Info($"{nameof(Mod)}.{nameof(OnLoad)}");

            try
            {
                // Register and load mod settings.
                ModSettings = new ModSettings(this);
                ModSettings.RegisterInOptionsUI();
                ModSettings.RegisterKeyBindings();
                AssetDatabase.global.LoadSettings(nameof(PerformanceMonitor), ModSettings, new ModSettings(this));
                ModSettings.Loaded();

                // Set up all locales.
                foreach (string languageCode in Translation.instance.LanguageCodes)
                {
                    GameManager.instance.localizationManager.AddSource(languageCode, new Locale(languageCode));
                }

                // Check if operating system is Windows.
                if (IsWindows())
                {
                    // CPU and memory usage are shown only for Windows.
                    ShowCPUUsage = true;
                    ShowMemoryUsage = true;

                    // GPU usage is shown only for Windows and only if successfully initialized.
                    ShowGPUUsage = InitializeGPUUsage();
                }

                // Create and activate this mod's systems.
                updateSystem.UpdateAt<UISystem>(SystemUpdatePhase.UIUpdate);

                #if DEBUG
                // Create UI constant files.
                // Uncomment this only when the UI constant files need to be created or recreated.
                // Then run the mod once in the game to create the constant files.
                // Then comment this again.  The UI constants are now available to use.
                //UIConstantFiles.Create();
                #endif
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
            }

            LogUtil.Info($"{nameof(Mod)}.{nameof(OnLoad)} complete.");
        }

        /// <summary>
        /// One-time mod disposing.
        /// </summary>
        public void OnDispose()
        {
            LogUtil.Info($"{nameof(Mod)}.{nameof(OnDispose)}");

            // Unregister mod settings.
            ModSettings?.UnregisterInOptionsUI();
            ModSettings = null;

            // Shutdown GPU usage.
            if (IsWindows())
            {
                ShutdownGPUUsage();
            }
        }

        /// <summary>
        /// Get whether or not operating system is Windows.
        /// This is a separate function to avoid a reference to the Windows-only CheckWindows class in the OnLoad and OnDispose functions.
        /// </summary>
        private bool IsWindows()
        {
            try
            {
                return CheckWindows.IsWindows();
            }
            catch
            { 
                return false;
            }
        }

        /// <summary>
        /// Initialize GPU usage.
        /// This is a separate function to avoid a reference to the Windows-only GPUUsage class in the OnLoad function.
        /// </summary>
        private bool InitializeGPUUsage()
        {
            try
            {
                return GPUUsage.Initialize();
            }
            catch
            { 
                return false;
            }
        }

        /// <summary>
        /// Shut down GPU usage.
        /// This is a separate function to avoid a reference to the Windows-only GPUUsage class in the OnDispose function.
        /// </summary>
        private void ShutdownGPUUsage()
        {
            try
            {
                GPUUsage.Shutdown();
            }
            catch
            {
                // Ignore exception.
            }
        }
    }
}
