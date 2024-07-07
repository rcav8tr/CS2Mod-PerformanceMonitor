// This entire file is only for creating UI constant files when in DEBUG.
#if DEBUG

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PerformanceMonitor
{
    /// <summary>
    /// Create the two files that hold constants used by both C# and UI.
    /// The two files are created from data to ensure the constants are the same in both places.
    /// </summary>
    public static class UIConstantFiles
    {
        // Shortcut for UI constants dictionary.
        // Dictionary key is the constant name.
        // Dictionary value is the constant value.
        private class UIConstants : Dictionary<string, string> { }

        // Shortcut for translation keys list.
        // Entry is used for constant name and constant value suffix.
        private class TranslationKeys : List<string> { }

        // Define constants for event names.
        private const string EventsClassName = "UIEventName";
        private const string EventsGroupComment = "Events group name.";
        private static readonly UIConstants _eventsGroupName = new UIConstants()
        {
            { "GroupName",          ModAssemblyInfo.Name    },
        };

        private const string EventsUItoCSComment = "Events from UI to C#.";
        private static readonly UIConstants _eventsUItoCS = new UIConstants()
        {
            { "MainButtonClicked",  "mainButtonClicked"     },
            { "MainPanelMoved",     "mainPanelMoved"        },
        };
        
        private const string EventsCStoUIMainPanelComment = "Events from C# to UI for main panel.";
        private static readonly UIConstants _eventsCStoUIMainPanel = new UIConstants()
        {
            { "MainPanelVisible",   "mainPanelVisible"      },
            { "MainPanelPositionX", "mainPanelPositionX"    },
            { "MainPanelPositionY", "mainPanelPositionY"    },
        };
        
        private const string EventsCStoUIDataValuesComment = "Events from C# to UI for data values.";
        private static readonly UIConstants _eventsCStoUIDataValues = new UIConstants()
        {
            { "CurrentGameMinute",  "currentGameMinute"     },
            { "PreviousGameMinute", "previousGameMinute"    },
            { "FrameRate",          "frameRate"             },
            { "GPUUsage",           "gpuUsage"              },
            { "CPUUsage",           "cpuUsage"              },
            { "MemoryUsage",        "memoryUsage"           },
        };
        
        private const string EventsCStoUIShowValuesComment = "Events from C# to UI for whether or not to show data values.";
        private static readonly UIConstants _eventsCStoUIShowValues = new UIConstants()
        {
            { "ShowGPUUsage",       "showGPUUsage"          },
            { "ShowCPUUsage",       "showCPUUsage"          },
            { "ShowMemoryUsage",    "showMemoryUsage"       },
        };

        // Define constants for translation keys.
        private const string TranslationKeyClassName = "UITranslationKey";
        private const string TranslationKeyModInfoComment = "Mod title and description.";
        private static readonly TranslationKeys _translationKeyModInfo = new TranslationKeys()
        {
            "Title",
            "Description",
        };
        
        private const string TranslationKeySettingsComment = "Settings.";
        private static readonly UIConstants _translationKeySettings = new UIConstants()
        {
            { "SettingTitle",                      Mod.ModSettings.GetSettingsLocaleID()                                              },
            { "SettingBindingMap",                 Mod.ModSettings.GetBindingMapLocaleID()                                            },
            
            { "SettingGroupGeneral",               Mod.ModSettings.GetOptionGroupLocaleID(ModSettings.GroupGeneral)                   },
            { "SettingActivationKeyBinding",       Mod.ModSettings.GetBindingKeyLocaleID (nameof(ModSettings.ActivationKeyBinding))   },
            { "SettingActivationKeyBindingLabel",  Mod.ModSettings.GetOptionLabelLocaleID(nameof(ModSettings.ActivationKeyBinding))   },
            { "SettingActivationKeyBindingDesc",   Mod.ModSettings.GetOptionDescLocaleID (nameof(ModSettings.ActivationKeyBinding))   },
            { "SettingResetWindowPositionLabel",   Mod.ModSettings.GetOptionLabelLocaleID(nameof(ModSettings.ResetMainPanelPosition)) },
            { "SettingResetWindowPositionDesc",    Mod.ModSettings.GetOptionDescLocaleID (nameof(ModSettings.ResetMainPanelPosition)) },
            
            { "SettingGroupAbout",                 Mod.ModSettings.GetOptionGroupLocaleID(ModSettings.GroupAbout)                     },
            { "SettingModVersionLabel",            Mod.ModSettings.GetOptionLabelLocaleID(nameof(ModSettings.ModVersion))             },
            { "SettingModVersionDesc",             Mod.ModSettings.GetOptionDescLocaleID (nameof(ModSettings.ModVersion))             },
        };
        
        private const string TranslationKeyMeasurementRowLabelsComment = "Measurement row labels.";
        private static readonly TranslationKeys _translationKeyMeasurementRowLabels = new TranslationKeys()
        {
            "RowLabelCurrentGameMinute",
            "RowLabelPreviousGameMinute",
            "RowLabelFrameRate",
            "RowLabelGPUUsage",
            "RowLabelCPUUsage",
            "RowLabelMemoryUsage",
        };
        
        private const string TranslationKeyMeasurementToolTipsComment = "Measurement tool tips.";
        private static readonly TranslationKeys _translationKeyMeasurementToolTips = new TranslationKeys()
        {
            "ToolTipCurrentGameMinute",
            "ToolTipPreviousGameMinute",
            "ToolTipFrameRate",
            "ToolTipGPUUsage",
            "ToolTipCPUUsage",
            "ToolTipMemoryUsage",
        };

        // Define constants for setting default values.
        private const string SettingDefaultsClassName = "ModSettingsDefaults";
        private const string SettingDefaultsComment = "Defaults for mod settings.";
        private const bool MainPanelVisible = false;
        private const int MainPanelPositionX = 175;
        private const int MainPanelPositionY = 55;

        /// <summary>
        /// Create the two UI constant files.
        /// One file for C# and one file for UI.
        /// </summary>
        public static void Create()
        {
            // Write the C# file to the same folder as this source code file.
            string contentsCS = ConstructFileContents(true);
            string sourceCodePath = GetSourceCodePath();
            File.WriteAllText(Path.Combine(sourceCodePath, "UIConstants.cs"), contentsCS);

            // Write the UI file to the UI/src/types folder.
            // Assumes this source code file is in a folder, so first need to go up one directory.
            string contentsUI = ConstructFileContents(false);
            string uiPath = Path.Combine(Directory.GetParent(sourceCodePath).FullName, "UI", "src", "types");
            File.WriteAllText(Path.Combine(uiPath, "uiConstants.ts"), contentsUI);
        }

        /// <summary>
        /// Construct the contents of the C# (CS) or UI file.
        /// </summary>
        private static string ConstructFileContents(bool typeCS)
        {
            StringBuilder sb = new StringBuilder();

            // Include do not modify instructions.
            sb.AppendLine($"// DO NOT MODIFY THIS FILE.");
            sb.AppendLine($"// This entire file was automatically generated by class {nameof(UIConstantFiles)}.");
            sb.AppendLine($"// Make any needed changes in class {nameof(UIConstantFiles)}.");
            sb.AppendLine();

            // Include namespace for C# file.
            if (typeCS)
            {
                sb.AppendLine($"namespace {ModAssemblyInfo.Name}");
                sb.AppendLine("{");
            }

            // Include class for event names.
            sb.Append(StartClass(typeCS, EventsClassName));
            sb.Append(GetConstantsContent(typeCS, EventsGroupComment,               _eventsGroupName));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, EventsUItoCSComment,              _eventsUItoCS));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, EventsCStoUIMainPanelComment,     _eventsCStoUIMainPanel));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, EventsCStoUIDataValuesComment,    _eventsCStoUIDataValues));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, EventsCStoUIShowValuesComment,    _eventsCStoUIShowValues));
            sb.Append(EndClass(typeCS));

            // Include translation keys.
            sb.AppendLine();
            sb.Append(StartClass(typeCS, TranslationKeyClassName));
            sb.Append(GetConstantsContent(typeCS, TranslationKeyModInfoComment,                 _translationKeyModInfo));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, TranslationKeySettingsComment,                _translationKeySettings));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, TranslationKeyMeasurementRowLabelsComment,    _translationKeyMeasurementRowLabels));
            sb.AppendLine();
            sb.Append(GetConstantsContent(typeCS, TranslationKeyMeasurementToolTipsComment,     _translationKeyMeasurementToolTips));
            sb.Append(EndClass(typeCS));

            // Include class for setting defaults.
            sb.AppendLine();
            sb.Append(StartClass(typeCS, SettingDefaultsClassName));
            if (typeCS)
            {
                sb.AppendLine($"        // {SettingDefaultsComment}");
                sb.AppendLine($"        public const bool {nameof(MainPanelVisible)} = {MainPanelVisible.ToString().ToLower()};");
                sb.AppendLine($"        public const int {nameof(MainPanelPositionX)} = {MainPanelPositionX};");
                sb.AppendLine($"        public const int {nameof(MainPanelPositionY)} = {MainPanelPositionY};");
            }
            else
            {
                sb.AppendLine($"    // {SettingDefaultsComment}");
                sb.AppendLine($"    public static {nameof(MainPanelVisible)}: boolean = {MainPanelVisible.ToString().ToLower()};");
                sb.AppendLine($"    public static {nameof(MainPanelPositionX)}: number = {MainPanelPositionX};");
                sb.AppendLine($"    public static {nameof(MainPanelPositionY)}: number = {MainPanelPositionY};");
            }
            sb.Append(EndClass(typeCS));

            // End namespace for C# file.
            if (typeCS)
            {
                sb.AppendLine("}");
            }

            // Return the contents.
            return sb.ToString();
        }

        /// <summary>
        /// Get the content for the start of a class.
        /// </summary>
        private static string StartClass(bool typeCS, string className)
        {
            StringBuilder sb = new StringBuilder();
            if (typeCS)
            {
                sb.AppendLine("    public class " + className);
                sb.AppendLine("    {");
            }
            else
            {
                sb.AppendLine("export class " + className);
                sb.AppendLine("{");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the content for the end of a class.
        /// </summary>
        private static string EndClass(bool typeCS)
        {
            StringBuilder sb = new StringBuilder();
            if (typeCS)
            {
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the content for a comment and constants.
        /// </summary>
        private static string GetConstantsContent(bool typeCS, string comment, UIConstants constants)
        {
            string indentation = (typeCS ? "        " : "    ");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{indentation}// {comment}");
            foreach (var key in constants.Keys)
            {
                if (typeCS)
                {
                    sb.AppendLine($"{indentation}public const string {key} = \"{constants[key]}\";");
                }
                else
                {
                    sb.AppendLine($"{indentation}public static {key}: string = \"{constants[key]}\";");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the translation key content for a comment and constants.
        /// </summary>
        private static string GetConstantsContent(bool typeCS, string comment, TranslationKeys keys)
        {
            string indentation = (typeCS ? "        " : "    ");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{indentation}// {comment}");
            foreach (var key in keys)
            {
                if (typeCS)
                {
                    sb.AppendLine($"{indentation}public const string {key} = \"{ModAssemblyInfo.Name}.{key}\";");
                }
                else
                {
                    sb.AppendLine($"{indentation}public static {key}: string = \"{ModAssemblyInfo.Name}.{key}\";");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the full path of this C# source code file.
        /// </summary>
        private static string GetSourceCodePath([System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "")
        {
            return Path.GetDirectoryName(sourceFile);
        }
    }
}

#endif
