using Colossal.Localization;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Input;
using Game.SceneFlow;
using Game.Simulation;
using Game.UI;
using Game.UI.InGame;
using System;
using System.Globalization;
using UnityEngine.InputSystem;

namespace PerformanceMonitor
{
    /// <summary>
    /// The main system for mod's UI.
    /// </summary>
    public partial class UISystem : UISystemBase
    {
        // Game systems and managers that are obtained one-time.
        private GameScreenUISystem      _gameScreenUISystem;                // Used to get status of pause menu.
        private SimulationSystem        _simulationSystem;                  // Used to get simulation speed.
        private TimeSystem              _timeSystem;                        // Used to get current game date/time.
        private LocalizationManager     _localizationManager;               // Used to get current locale ID.

        // Variables that get one-time initialization.
        private long                    _baseDateTicks;                     // Ticks in the current real time date when the mod is created.
        
        // Current culture.
        private CultureInfo             _currentCulture;                    // Used to format numbers.

        // System control.
        private bool                    _inGame = false;                    // Whether or not application is in a game (i.e. as opposed to main menu, editor, etc.).
        private bool                    _previousPauseMenuActive = false;   // Whether or not game pause menu was active in the previous frame.

        // Timing control.
        private double                  _previousRealTimeForDeltaTime;      // Previous real time for computing delta time from previous frame.
        private double                  _previousRealTimeForOneSecond;      // Previous real time for one second timing.
        private float                   _previousSimulationSpeed;           // Previous simulation speed.  Used to detect when simulation speed changes.
        private bool                    _savingGame;                        // Whether or not a game is currently being saved.
        private int                     _savingGameCounter;                 // Frame counter while saving a game.

        // Simulation timing.
        private bool                    _simTimingChangedOnce;              // Whether or not the game minute changed once.  Used to prevent timing a partial first game minute.
        private int                     _simTimingPreviousGameMinute;       // Previous game minute.  Used to detect game minute change.
        private double                  _simTimingCurrentTotalTime;         // Current accumulated total real time spent in a game minute.
        private double                  _simTimingPreviousTotalTime;        // Previous accumulated total real time.
        
        // FPS timing.
        private int                     _fpsTimingFrameCount;               // Frame count.

        // C# to UI bindings for main panel.
        private ValueBinding<bool>      _bindingMainPanelVisible;
        private ValueBinding<int>       _bindingMainPanelPositionX;
        private ValueBinding<int>       _bindingMainPanelPositionY;

        // C# to UI bindings for data values.
        private ValueBinding<string>    _bindingCurrentGameMinute;
        private ValueBinding<string>    _bindingPreviousGameMinute;
        private ValueBinding<string>    _bindingFrameRate;
        private ValueBinding<string>    _bindingGPUUsage;
        private ValueBinding<string>    _bindingCPUUsage;
        private ValueBinding<string>    _bindingMemoryUsage;

        
        /// <summary>
        /// Mod is valid only in game, not editor.
        /// </summary>
        public override GameMode gameMode
        {
            get { return GameMode.Game; }
        }

        /// <summary>
        /// Do one-time initialization of the system.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            try
            {
                LogUtil.Info($"{nameof(UISystem)}.{nameof(OnCreate)}");

                // Get one-time game systems and managers.
                // Getting the systems and managers once here avoids the need to get them every time they are needed.
                _gameScreenUISystem  = World.GetOrCreateSystemManaged<GameScreenUISystem>();
                _simulationSystem    = World.GetOrCreateSystemManaged<SimulationSystem  >();
                _timeSystem          = World.GetOrCreateSystemManaged<TimeSystem        >();
                _localizationManager = GameManager.instance.localizationManager;

                // Get ticks in the current real-time date.
                // Used to increase accuracy when computing delta time.
                _baseDateTicks = DateTime.Now.Date.Ticks;

                // Initialize current culture.
                _currentCulture = GetCurrentCulture();

                // Add bindings for UI to C#.
                AddBinding(new TriggerBinding          (UIEventName.GroupName, UIEventName.MainButtonClicked, MainButtonClicked));
                AddBinding(new TriggerBinding<int, int>(UIEventName.GroupName, UIEventName.MainPanelMoved,    MainPanelMoved   ));

                // Add bindings for C# to UI for main panel.
                AddBinding(_bindingMainPanelVisible   = new ValueBinding<bool  >(UIEventName.GroupName, UIEventName.MainPanelVisible,   Mod.ModSettings.MainPanelVisible  ));
                AddBinding(_bindingMainPanelPositionX = new ValueBinding<int   >(UIEventName.GroupName, UIEventName.MainPanelPositionX, Mod.ModSettings.MainPanelPositionX));
                AddBinding(_bindingMainPanelPositionY = new ValueBinding<int   >(UIEventName.GroupName, UIEventName.MainPanelPositionY, Mod.ModSettings.MainPanelPositionY));

                // Add bindings for C# to UI for data values.
                AddBinding(_bindingCurrentGameMinute  = new ValueBinding<string>(UIEventName.GroupName, UIEventName.CurrentGameMinute,  ""));
                AddBinding(_bindingPreviousGameMinute = new ValueBinding<string>(UIEventName.GroupName, UIEventName.PreviousGameMinute, ""));
                AddBinding(_bindingFrameRate          = new ValueBinding<string>(UIEventName.GroupName, UIEventName.FrameRate,          ""));
                AddBinding(_bindingGPUUsage           = new ValueBinding<string>(UIEventName.GroupName, UIEventName.GPUUsage,           ""));
                AddBinding(_bindingCPUUsage           = new ValueBinding<string>(UIEventName.GroupName, UIEventName.CPUUsage,           ""));
                AddBinding(_bindingMemoryUsage        = new ValueBinding<string>(UIEventName.GroupName, UIEventName.MemoryUsage,        ""));

                // Add bindings for C# to UI for showing data values.
                // This is the only time these binding values get set, so no need to save the bindings.
                AddBinding(new ValueBinding<bool>(UIEventName.GroupName, UIEventName.ShowGPUUsage,    Mod.ShowGPUUsage   ));
                AddBinding(new ValueBinding<bool>(UIEventName.GroupName, UIEventName.ShowCPUUsage,    Mod.ShowCPUUsage   ));
                AddBinding(new ValueBinding<bool>(UIEventName.GroupName, UIEventName.ShowMemoryUsage, Mod.ShowMemoryUsage));
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
            }
        }

        /// <summary>
        /// Get the current culture from the active locale ID.
        /// </summary>
        private CultureInfo GetCurrentCulture()
        {
            return new CultureInfo(_localizationManager.activeLocaleId);
        }

        /// <summary>
        /// Called by the game when a GameMode is about to be loaded.
        /// </summary>
        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);

            // If currently in a game, then deinitialize.
            if (_inGame)
            {
                Deinitialize();
            }
        }

        /// <summary>
        /// Called by the game when a GameMode is done being loaded.
        /// </summary>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
	        base.OnGameLoadingComplete(purpose, mode);

            // If currently in a game, then deinitialize.
            if (_inGame)
            {
                Deinitialize();
            }

            // If started a game, then initialize.
            if (mode == GameMode.Game)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize this mod to get everything ready for a newly started game.
        /// </summary>
        private void Initialize()
        {
            try
            {
                LogUtil.Info($"{nameof(UISystem)}.{nameof(Initialize)}");

                // Initialize current culture.
                _currentCulture = GetCurrentCulture();

                // Initialize system control variables.
                _previousPauseMenuActive = _gameScreenUISystem.isMenuActive;    // Game normally starts not in pause menu.

                // Initialize timing control variables.
                _previousRealTimeForDeltaTime = _previousRealTimeForOneSecond = GetCurrentRealTime();
                _previousSimulationSpeed = _simulationSystem.selectedSpeed;     // Could be paused (i.e. 0).
                _savingGame = false;
                _savingGameCounter = 0;

                // Initialize simulation timing variables.
                _simTimingChangedOnce = false;
                _simTimingPreviousGameMinute = _timeSystem.GetCurrentDateTime().Minute;
                _simTimingCurrentTotalTime = 0d;
                _simTimingPreviousTotalTime = 0d;

                // Initialize FPS timing variables.
                _fpsTimingFrameCount = 0;

                // Initialize CPU usage.
                if (Mod.ShowCPUUsage)
                {
                    InitializeCPUUsage();
                }

                // Clear all data values on the main panel that might be left over from a previous game.
                _bindingCurrentGameMinute .Update("");
                _bindingPreviousGameMinute.Update("");
                _bindingFrameRate         .Update("");
                _bindingGPUUsage          .Update("");
                _bindingCPUUsage          .Update("");
                _bindingMemoryUsage       .Update("");

                // Enable activation key.
                ProxyAction activationKeyAction = Mod.ModSettings.GetAction(ModSettings.ActivationKeyActionName);
                activationKeyAction.shouldBeEnabled = true;
                activationKeyAction.onInteraction += ActivationKeyInteraction;

                // Listen for language change events.
                _localizationManager.onActiveDictionaryChanged += LocalizationManager_onActiveDictionaryChanged;

                // Listen for game save events.
                GameManager.instance.onGameSaveLoad += GameManager_onGameSaveLoad;

                // In a game.  Set this last because this allows OnUpdate to run and everything else should be initialized before then.
                _inGame = true;
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
            }
        }

        /// <summary>
        /// Deinitialize this mod to cleanup from a game just ended.
        /// </summary>
        private void Deinitialize()
        {
            try
            {
                LogUtil.Info($"{nameof(UISystem)}.{nameof(Deinitialize)}");

                // Not in a game.  Set this first to stop OnUpdate from running while everything is being cleaned up.
                _inGame = false;

                // Stop listening for language change events.
                _localizationManager.onActiveDictionaryChanged -= LocalizationManager_onActiveDictionaryChanged;

                // Stop listening for game save events.
                GameManager.instance.onGameSaveLoad -= GameManager_onGameSaveLoad;

                // Disable activation key.
                ProxyAction activationKeyAction = Mod.ModSettings.GetAction(ModSettings.ActivationKeyActionName);
                activationKeyAction.shouldBeEnabled = false;
                activationKeyAction.onInteraction -= ActivationKeyInteraction;
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
            }
        }

        /// <summary>
        /// Event callback when game saving is started or ended.
        /// </summary>
        private void GameManager_onGameSaveLoad(string saveName, bool start)
        {
            // Check for start or end of game save.
            if (start)
            {
                _savingGameCounter = 0;
                _savingGame = true;
            }
            else
            {
                _savingGame = false;
            }
        }

        /// <summary>
        /// Get the current real time in seconds and fractions of a second since the base date.
        /// </summary>
        private double GetCurrentRealTime()
        {
            // Subtract base date ticks to increase accuracy for computing delta time.
            return (DateTime.Now.Ticks - _baseDateTicks) / (double)TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// Called every frame, even when at the main menu.
        /// </summary>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            // If not in a game, do nothing.
            if (!_inGame)
            {
                return;
            }

            try
            {
                // Compute delta time from previous frame.
                double currentRealTime = GetCurrentRealTime();
                double deltaRealTime = currentRealTime - _previousRealTimeForDeltaTime;
                _previousRealTimeForDeltaTime = currentRealTime;

                // Check for change in pause menu active status.
                bool pauseMenuActive = _gameScreenUISystem.isMenuActive;
                if (pauseMenuActive != _previousPauseMenuActive)
                {
                    // Pause menu changed.  Check change direction.
                    if (pauseMenuActive)
                    {
                        // Pause menu is now active.
                        // Nothing to do here.
                    }
                    else
                    {
                        // Pause menu is now inactive.
                        // Initialize things related to one second timing.
                        _previousRealTimeForOneSecond = currentRealTime;
                        _fpsTimingFrameCount = 0;
                        if (Mod.ShowCPUUsage)
                        {
                            InitializeCPUUsage();
                        }
                    }

                    // Save pause menu active status for use in the next frame.
                    _previousPauseMenuActive = pauseMenuActive;
                }

                // Do performance monitoring only when not in pause menu.
                if (!pauseMenuActive)
                {
                    // For simulation timing purposes, game save lasts only 2 frames, even though the onGameSaveLoad takes more frames.
                    if (_savingGame)
                    {
                        if (_savingGameCounter++ == 2)
                        {
                            _savingGame = false;
                        }
                    }

                    // Do simulation timing only while simulation is running and not saving a game.
                    float simulationSpeed = _simulationSystem.selectedSpeed;
                    if (simulationSpeed > 0 && !_savingGame)
                    {
                        // Check for change in simulation speed.
                        if (simulationSpeed != _previousSimulationSpeed)
                        {
                            // Save new simulation speed.
                            _previousSimulationSpeed = simulationSpeed;

                            // Restart timing by waiting again for first change.
                            _simTimingChangedOnce = false;

                            // Reset simulation timings.
                            _simTimingCurrentTotalTime  = 0d;
                            _simTimingPreviousTotalTime = 0d;

                            // Clear simulation timings from the panel.
                            _bindingCurrentGameMinute .Update("");
                            _bindingPreviousGameMinute.Update("");
                        }
                        else
                        {
                            // Accumulate total time.
                            _simTimingCurrentTotalTime += deltaRealTime;

                            // Check if game time value has changed once.
                            if (_simTimingChangedOnce)
                            {
                                // Show current total time.
                                if (Mod.ModSettings.MainPanelVisible)
                                {
                                    _bindingCurrentGameMinute.Update(FormatSimTimingCurrent());
                                }
                            }

                            // Check for game minute change.
                            int currentGameMinute = _timeSystem.GetCurrentDateTime().Minute;
                            if (currentGameMinute != _simTimingPreviousGameMinute)
                            {
                                // Game minute has changed.
                                // On second and subsequent game minute changes, display current total time as previous time.
                                if (_simTimingChangedOnce)
                                {
                                    _simTimingPreviousTotalTime = _simTimingCurrentTotalTime;
                                    if (Mod.ModSettings.MainPanelVisible)
                                    {
                                        _bindingPreviousGameMinute.Update(FormatSimTimingPrevious());
                                    }
                                }

                                // Reset for next minute.
                                _simTimingChangedOnce = true;
                                _simTimingPreviousGameMinute = currentGameMinute;
                                _simTimingCurrentTotalTime = 0d;
                            }
                        }
                    }

                    // Count frames for FPS timing.
                    _fpsTimingFrameCount++;

                    // Detect when at least one second has elapsed.
                    double elapsedTime = currentRealTime - _previousRealTimeForOneSecond;
                    if (elapsedTime >= 1d)
                    {
                        // Save current real time.
                        _previousRealTimeForOneSecond = currentRealTime;

                        // Display values once per second only if main panel is visible.
                        if (Mod.ModSettings.MainPanelVisible)
                        {
                            // Always display frames per second.
                            double framesPerSecond = _fpsTimingFrameCount / elapsedTime;
                            _bindingFrameRate.Update(FormatFramesPerSecond(framesPerSecond));

                            // Display GPU usage.
                            if (Mod.ShowGPUUsage)
                            {
                                DisplayGPUUsage();
                            }

                            // Display CPU usage.
                            if (Mod.ShowCPUUsage)
                            {
                                DisplayCPUUsage();
                            }

                            // Display memory usage.
                            if (Mod.ShowMemoryUsage)
                            {
                                DisplayMemoryUsage();
                            }
                        }

                        // Reset FPS frame counter.
                        _fpsTimingFrameCount = 0;
                    }
                }
            }
            catch (Exception ex) 
            {
                LogUtil.Exception(ex);
            }
        }

        /// <summary>
        /// Return formatted current total time.
        /// </summary>
        private string FormatSimTimingCurrent()
        {
            // Display blank for zero.
            if (_simTimingCurrentTotalTime == 0d)
            {
                return "";
            }

            // Format the time.
            if (_simTimingCurrentTotalTime < 10d)
            {
                // For less than 10 seconds, use 1 decimal place truncated down to 0.1.
                double numberToFormat = Math.Floor(_simTimingCurrentTotalTime * 10d) / 10d;
                return numberToFormat.ToString("N1", _currentCulture);
            }
            else
            {
                // For 10 seconds or more, use 0 decimal places.
                double numberToFormat = Math.Floor(_simTimingCurrentTotalTime);
                return numberToFormat.ToString("N0", _currentCulture);
            }
        }

        /// <summary>
        /// Return formatted last total time.
        /// </summary>
        private string FormatSimTimingPrevious()
        {
            // Display blank for zero.
            if (_simTimingPreviousTotalTime == 0d)
            {
                return "";
            }

            // Always 2 decimal places.
            return _simTimingPreviousTotalTime.ToString("N2", _currentCulture);
        }

        /// <summary>
        /// Return formatted frames per second.
        /// </summary>
        private string FormatFramesPerSecond(double framesPerSecond)
        {
            // Less than 10 FPS gets 1 decimal place.
            // 10 seconds or more gets 0 decimal places.
            string format = (framesPerSecond < 10d ? "N1" : "N0");
            return framesPerSecond.ToString(format, _currentCulture);
        }

        /// <summary>
        /// Initialize CPU usage.
        /// This is a separate function to avoid a reference to the Windows-only CPUUsage class in the Initialize and OnUpdate functions.
        /// </summary>
        private void InitializeCPUUsage()
        {
            // If CPUUsage fails to initialize, then do not show CPU usage.
            Mod.ShowCPUUsage = CPUUsage.Initialize();
        }

        /// <summary>
        /// Display GPU usage value.
        /// This is a separate function to avoid a reference to the Windows-only GPUUsage class in the OnUpdate function.
        /// </summary>
        private void DisplayGPUUsage()
        {
            // Display GPU usage percent.
            // No special formatting needed because this is an integer percent.
            _bindingGPUUsage.Update(GPUUsage.GetGPUUsage().ToString());
        }

        /// <summary>
        /// Display CPU usage value.
        /// This is a separate function to avoid a reference to the Windows-only CPUUsage class in the OnUpdate function.
        /// </summary>
        private void DisplayCPUUsage()
        {
            // Display CPU usage percent.
            // No special formatting needed because this is an integer percent.
            _bindingCPUUsage.Update(CPUUsage.GetCPUUsage().ToString());
        }

        /// <summary>
        /// Display memory usage value.
        /// This is a separate function to avoid a reference to the Windows-only MemoryUsage class in the OnUpdate function.
        /// </summary>
        private void DisplayMemoryUsage()
        {
            // Display memory usage percents.
            // No special formatting needed because this is an integer percent.
            _bindingMemoryUsage.Update(MemoryUsage.GetMemoryUsage().ToString());
        }

        /// <summary>
        /// Handle localization dictionary (i.e. language) change.
        /// </summary>
        private void LocalizationManager_onActiveDictionaryChanged()
        {
            // Get current culture.
            _currentCulture = GetCurrentCulture();

            // If main panel is visible, immediately update or clear values.
            if (Mod.ModSettings.MainPanelVisible)
            {
                // Update current and previous sim timings to use new culture in formatting.
                _bindingCurrentGameMinute .Update(FormatSimTimingCurrent());
                _bindingPreviousGameMinute.Update(FormatSimTimingPrevious());

                // Clear other data values, which will update on their own within one second.
                _bindingFrameRate  .Update("");
                _bindingGPUUsage   .Update("");
                _bindingCPUUsage   .Update("");
                _bindingMemoryUsage.Update("");
            }
        }

        /// <summary>
        /// Handle activation key interaction.
        /// </summary>
        private void ActivationKeyInteraction(ProxyAction action, InputActionPhase phase)
        {
            // Activation key performed is same as main button clicked.
            if (phase == InputActionPhase.Performed)
            {
                MainButtonClicked();
            }
        }
        
        /// <summary>
        /// Event callback for main button clicked.
        /// </summary>
        private void MainButtonClicked()
        {
            // Toggle and save main panel visibility.
            bool newVisible = !_bindingMainPanelVisible.value;
            _bindingMainPanelVisible.Update(newVisible);
            Mod.ModSettings.MainPanelVisible = newVisible;
            Mod.ModSettings.ApplyAndSave();

            // When panel is newly visible, immediately update current and previous sim timings.
            // Other data values will update on their own within one second.
            if (newVisible)
            {
                _bindingCurrentGameMinute .Update(FormatSimTimingCurrent());
                _bindingPreviousGameMinute.Update(FormatSimTimingPrevious());
            }
        }

        /// <summary>
        /// Event callback for main panel moved.
        /// </summary>
        private void MainPanelMoved(int positionX, int positionY)
        {
            // Save main panel position.
            Mod.ModSettings.MainPanelPositionX = positionX;
            Mod.ModSettings.MainPanelPositionY = positionY;
            Mod.ModSettings.ApplyAndSave();

            // Send position back to UI.
            SetMainPanelPosition(positionX, positionY);
        }

        /// <summary>
        /// Set main panel position on UI.
        /// </summary>
        public void SetMainPanelPosition(int positionX, int positionY)
        {
            _bindingMainPanelPositionX.Update(positionX);
            _bindingMainPanelPositionY.Update(positionY);
        }
    }
}
