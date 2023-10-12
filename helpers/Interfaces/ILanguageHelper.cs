using System.Collections.Generic;

namespace helpers.Interfaces
{
    public interface ILanguageHelper
    {
        string GetLanguage(string lang, string key, string defaultText = null);
        string GetLanguage(string lang, string key, List<(string Placeholder, string Value)> replacers);
    }
}