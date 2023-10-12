using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;
using models;
using System.Linq;
using helpers.Interfaces;

namespace helpers.Engine
{
    public class LanguageHelper : ILanguageHelper
    {
        private readonly List<LanguageInfo> _inMemoryLanguageSet;
        public LanguageHelper(IConfiguration config)
        {
            var langDir = config.GetValue("Utility:LangDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Langs"));
            _inMemoryLanguageSet = SetupInMemoryCategoryList(langDir);
        }

        public string GetLanguage(string lang, string key, string defaultText = null)
        {
            var langInfo = _inMemoryLanguageSet.FirstOrDefault(x => x.Lang.ToLower() == lang.ToLower() && x.Key.ToLower() == key.ToLower());
            if (langInfo == null || string.IsNullOrWhiteSpace(langInfo.Text)) return $"{defaultText}";

            return langInfo.Text;
        }

        public string GetLanguage(string lang, string key, List<(string Placeholder, string Value)> replacers)
        {
            var language = GetLanguage(lang, key);
            if (!replacers.Any())
            {
                for (int i = 0; i < replacers.Count; i++)
                {
                    language = language.Replace(replacers[i].Item1, replacers[i].Item2);
                }
            }
            
            return language;
        }

        private List<LanguageInfo> SetupInMemoryCategoryList(string langDir)
        {
            List<LanguageInfo> categories = new List<LanguageInfo>();
            var dir = Directory.GetFiles(langDir, "*.json");
            for (int i = 0; i < dir.Length; i++)
            {
                using (StreamReader r = new StreamReader(dir[i]))
                {
                    string json = r.ReadToEnd();
                    var categoryContent = json.ParseObject<List<LanguageInfo>>();
                    categories.AddRange(categoryContent);
                }
            }
            return categories;
        }
    }
}
