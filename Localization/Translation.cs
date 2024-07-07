using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PerformanceMonitor
{
    /// <summary>
    /// Get translated text.
    /// </summary>
    public class Translation
    {
        // Use singleton pattern:  there can be only one Translation in the mod.
        private static readonly Translation _instance = new Translation();
        public static Translation instance { get { return _instance; } }
        private Translation() { Initialize(); }

        // Translations are in the Translation.csv file.
        // The translation file is an embedded resource in the mod DLL so that a separate file does not need to be downloaded with the mod.
        // The translation file was created and maintained using LibreOffice Calc.

        // Translation file format:
        //      Line 1: blank,language code 1,language code 2,...,language code n
        //      Line 2: translation key 1,translated text 1,translated text 2,...,translated text n
        //      Line 3: translation key 2,translated text 1,translated text 2,...,translated text n
        //      ...
        //      Line m: translation key m-1,translated text 1,translated text 2,...,translated text n

        // Translation file notes:
        //      The first line in the file must contain language codes and therefore cannot be blank or a comment.
        //      The file must contain translations for the default language code.
        //      The file should contain translations for every language code supported by the base game.
        //      The file may contain translations for additional language codes.
        //      Language codes in the file may be in any order, except that the default language code must be first.
        //      A language code may not be duplicated.
        //      A blank line is skipped.
        //      A line with a blank translation key is skipped (except the first line).
        //      A line with a translation key that starts with the character (#) is considered a comment and is skipped.
        //      The file should contain a line for every translation key constant name (not value) in UITranslationKey.
        //      Translations keys are case sensitive.
        //      A translation key cannot be duplicated.
        //      The file must not contain blank columns.
        //      Each language code, translation key, and translated text may or may not be enclosed in double quotes ("text").
        //      Spaces around the comma separators will be included in the translated text.
        //      To include a comma in the translated text, the translated text must be enclosed in double quotes ("te, xt").
        //      To include a double quote in the translated text, use two consecutive double quotes inside the double quoted translated text ("te""xt").
        //      Translated text that begins with "@@" will use the translated text from the translation key after the "@@".
        //      The translation key referenced with "@@" must have been read prior to the line with the "@@".
        //      Translated text cannot be blank for the default language.
        //      Blank translated text in a non-default language will use the translated text for the default language.

        // Default language code.
        public const string DefaultLanguageCode = "en-US";

        // Translation keys from UITranslationKey.
        private static string[] _translationKeys = new string[0];

        // Translations for a single language.
        // Dictionary key is the translation key.
        // Dictionary value is the translated text for that translation key.
        private class SingleLanguage : Dictionary<string, string>
        {
            public SingleLanguage()
            {
                // Initialize the language with default values for all translation keys.
                foreach (string translationKey in _translationKeys)
                {
                    Add(translationKey, translationKey);
                }
            }
        }
        
        // Translations for all languages in the file.
        // Dictionary key is the language code.
        // Dictionary value contains the translations for that single language code.
        private class Languages : Dictionary<string, SingleLanguage> { }
        private Languages _languages = new Languages();
        public string[] LanguageCodes => _languages.Keys.ToArray();

        /// <summary>
        /// Initialize the translations from the translation file.
        /// </summary>
        private void Initialize()
        {
            try
            {
                LogUtil.Info($"{nameof(Translation)}.{nameof(Initialize)}");

                // Translation keys are the constant names from UITranslationKey.
                FieldInfo[] fields = typeof(UITranslationKey).GetFields();
                _translationKeys = new string[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    _translationKeys[i] = fields[i].Name;
                }

                // Initialize with just the default language.
                _languages.Clear();
                _languages.Add(DefaultLanguageCode, new SingleLanguage());

                // Make sure the translation CSV file exists.
                string translationFile = $"{ModAssemblyInfo.Name}.Localization.Translation.csv";
                if (!Assembly.GetExecutingAssembly().GetManifestResourceNames().Contains(translationFile))
                {
                    LogUtil.Error($"Translation file [{translationFile}] does not exist in the assembly.");
                    return;
                }

                // Read the lines from the translation CSV file.
                string[] lines;
                using (Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(translationFile))
                {
                    using (StreamReader fileReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        lines = fileReader.ReadToEnd().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
                    }
                }

                // First line cannot be blank or a comment.
                if (lines[0].Trim().Length == 0 || lines[0].StartsWith("#"))
                {
                    LogUtil.Error("Translation file first line is blank or comment. Expecting language codes on the first line.");
                    return;
                }

                // Initialize count of language codes.
                Dictionary<string, int> languageCodeCount = new Dictionary<string, int>
                {
                    { DefaultLanguageCode, 0 }
                };

                // Read the language codes from the first line.
                using (StringReader reader = new StringReader(lines[0]))
                {
                    // Read and ignore the first value, which should be blank.
                    ReadCSVValue(reader);

                    // Read language codes.
                    int count = 0;
                    string languageCode = ReadCSVValue(reader);
                    while (languageCode.Length != 0)
                    {
                        // Check if language code already exists.
                        if (_languages.ContainsKey(languageCode))
                        {
                            // Count language code occurrences.
                            languageCodeCount[languageCode]++;
                        }
                        else
                        {
                            // Add the new language code with initial count of 1.
                            _languages.Add(languageCode, new SingleLanguage());
                            languageCodeCount[languageCode] = 1;
                        }

                        // Check that first language code in the file is the default language code.
                        if (count == 0 && languageCode != DefaultLanguageCode)
                        {
                            LogUtil.Warn($"Translation file must have default language code [{DefaultLanguageCode}] defined first.");
                        }

                        // Get next language code.
                        count++;
                        languageCode = ReadCSVValue(reader);
                    }
                }

                // Each language code must be defined exactly once.
                foreach (string languageCode in languageCodeCount.Keys)
                {
                    if (languageCodeCount[languageCode] != 1)
                    {
                        LogUtil.Warn($"Translation file defines language code [{languageCode}] {languageCodeCount[languageCode]} times.  Expecting 1 time.");
                    }
                }

                // Initialize count of translation keys.
                Dictionary<string, int> translationKeyCount = new Dictionary<string, int>();
                foreach (string translationKey in _translationKeys)
                {
                    translationKeyCount.Add(translationKey, 0);
                }

                // Process each subsequent line.
                string[] languageCodes = _languages.Keys.ToArray();
                for (int i = 1; i < lines.Length; i++)
                {
                    // Do only non-blank lines.
                    string line = lines[i];
                    if (line.Trim().Length > 0)
                    {
                        // Create a string reader on the line.
                        using (StringReader reader = new StringReader(line))
                        {
                            // The first value in the line is the translation key.
                            // Do only non-blank, non-comment translation keys.
                            string keyFromFile = ReadCSVValue(reader);
                            if (keyFromFile.Length != 0 && !keyFromFile.StartsWith("#"))
                            {
                                // Check key from file.
                                if (!_translationKeys.Contains(keyFromFile))
                                {
                                    LogUtil.Warn($"Translation file contains translation key [{keyFromFile}], which does not have a corresponding key in the program.");
                                }
                                else
                                {
                                    // Count translation key occurrences.
                                    translationKeyCount[keyFromFile]++;

                                    // Do each language code.
                                    foreach (string languageCode in languageCodes)
                                    {
                                        // Check for blank translated text.
                                        string translatedText = ReadCSVValue(reader);
                                        if (string.IsNullOrEmpty(translatedText))
                                        {
                                            // Check for default language.
                                            if (languageCode == DefaultLanguageCode)
                                            {
                                                // For default language, use the key as the translated text.
                                                LogUtil.Warn($"Translation for key [{keyFromFile}] must be defined for default language code [{DefaultLanguageCode}].");
                                                translatedText = keyFromFile;
                                            }
                                            else
                                            {
                                                // For other than default language, use translated text from default language.
                                                translatedText = _languages[DefaultLanguageCode][keyFromFile];
                                            }
                                        }
                                        // Check for @@ reference.
                                        else if (translatedText.StartsWith("@@"))
                                        {
                                            // Get the translation key referenced after the @@.
                                            string atAtKey = translatedText.Substring(2);
                                            if (string.IsNullOrEmpty(atAtKey) || !_languages[languageCode].Keys.Contains(atAtKey))
                                            {
                                                LogUtil.Warn($"Translation for key [{keyFromFile}] for language [{languageCode}] has invalid @@ reference to key [{atAtKey}].");
                                                // Leave the invalid @@ reference in the translated text.
                                            }
                                            else
                                            {
                                                // Use translated text from the @@ key.
                                                translatedText = _languages[languageCode][atAtKey];
                                            }
                                        }

                                        // Save the translated text.
                                        _languages[languageCode][keyFromFile] = translatedText;
                                    }
                                }
                            }
                        }
                    }
                }

                // Each translation key must be defined exactly once.
                foreach (string translationkey in _translationKeys)
                {
                    if (translationKeyCount[translationkey] != 1)
                    {
                        LogUtil.Warn($"Translation file defines translation key [{translationkey}] {translationKeyCount[translationkey]} times.  Expecting 1 time.");
                    }
                }
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
            }
        }

        /// <summary>
        /// Read a CSV value.
        /// </summary>
        private string ReadCSVValue(StringReader reader)
        {
            // The value to return
            StringBuilder value = new StringBuilder();

            // Read until non-quoted comma or end-of-string is reached.
            bool inQuotes = false;
            int currentChar = reader.Read();
            while (currentChar != -1)
            {
                // Check for double quote char.
                if (currentChar == '\"')
                {
                    // Check whether or not already in double quotes.
                    if (inQuotes)
                    {
                        // Already in double quotes, check next char.
                        if (reader.Peek() == '\"')
                        {
                            // Next char is double quote.
                            // Consume the second double quote and replace the two consecutive double quotes with one double qoute.
                            reader.Read();
                            value.Append((char)currentChar);
                        }
                        else
                        {
                            // Next char is not double quote.
                            // This double quote is the end of a quoted string, don't append the double quote.
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        // Not already in double quotes.
                        // This double quote is the start of a quoted string, don't append the double quote.
                        inQuotes = true;
                    }
                }
                else
                {
                    // A comma not in double quotes ends the value, don't append the comma.
                    if (currentChar == ',' && !inQuotes)
                    {
                        break;
                    }

                    // All other cases, append the char.
                    value.Append((char)currentChar);
                }

                // Get next char.
                currentChar = reader.Read();
            }

            // Return the value.
            return value.ToString();
        }

        /// <summary>
        /// Get the translation of the key using the current active language code.
        /// </summary>
        public string Get(string translationKey)
        {
            return Get(translationKey, GameManager.instance.localizationManager.activeLocaleId);
        }

        /// <summary>
        /// Get the translation of the key using the specified language code.
        /// </summary>
        public string Get(string translationKey, string languageCode)
        {
            // If language code is not supported, then use default language code.
            // This can happen if a language in the base game is not defined in the translation file.
            // This can happen if a mod adds a language to the game and that language is not defined in the translation file.
            if (!_languages.ContainsKey(languageCode))
            {
                languageCode = DefaultLanguageCode;
            }
            
            // Return translated text for the language code and translation key.
            return _languages[languageCode][translationKey];
        }
    }
}
