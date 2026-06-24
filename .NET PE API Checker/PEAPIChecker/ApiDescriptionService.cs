using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace PEEXEAPIChecker
{
    internal sealed class ApiDescriptionService
    {
        private readonly Dictionary<string, string> _descriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ApiDescriptionService()
        {
            LoadDescriptions();
        }

        public string GetDescription(PeImportEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            string description;
            if (_descriptions.TryGetValue(entry.FunctionName, out description))
                return description;

            return string.Format("[{0}]", DllCategoryResolver.GetCategory(entry.DllName));
        }

        public string GetDocumentationUrl(PeImportEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (entry.FunctionName.StartsWith("Ordinal_", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(
                    "https://learn.microsoft.com/en-us/search/?terms={0}",
                    Uri.EscapeDataString(entry.DllName + " ordinal import"));
            }

            return string.Format(
                "https://learn.microsoft.com/en-us/search/?terms={0}",
                Uri.EscapeDataString(entry.FunctionName));
        }

        public string FormatImportLine(PeImportEntry entry)
        {
            return string.Format("{0} — {1}", entry.ImportName, GetDescription(entry));
        }

        private void LoadDescriptions()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiDescriptions.json");
            if (!File.Exists(path))
                return;

            try
            {
                string json = File.ReadAllText(path);
                var serializer = new JavaScriptSerializer();
                var loaded = serializer.Deserialize<Dictionary<string, string>>(json);
                if (loaded == null)
                    return;

                foreach (KeyValuePair<string, string> pair in loaded)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                        _descriptions[pair.Key] = pair.Value;
                }
            }
            catch
            {
                // Keep built-in DLL category fallback if JSON cannot be loaded.
            }
        }
    }
}
