using Colossal;
using System.Collections.Generic;
using System.Reflection;

namespace PerformanceMonitor
{
    public class Locale : IDictionarySource
    {
        private Dictionary<string, string> _locale = new Dictionary<string, string>();

        /// <summary>
        /// Constructor for a locale ID.
        /// </summary>
        public Locale(string localeID)
        {
            LogUtil.Info($"{nameof(Locale)}.{nameof(Locale)} localeID=[{localeID}]");

            // Initialize this locale by getting the translated text for every translation key.
            Translation translationInstance = Translation.instance;
            foreach (FieldInfo field in typeof(UITranslationKey).GetFields())
            {
                _locale.Add((string)field.GetValue(null), translationInstance.Get(field.Name, localeID));
            }
        }

        /// <summary>
        /// Return the previously loaded locale entries.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return _locale;
        }

        /// <summary>
        /// Implement required Unload function.
        /// </summary>
        public void Unload()
        {
            // Nothing to do here.
        }
    }
}
