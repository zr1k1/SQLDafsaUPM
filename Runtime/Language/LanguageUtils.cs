using System;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageUtils {
    private static readonly Dictionary<SystemLanguage, string> LanguageToId =
        new()
        {
            { SystemLanguage.German, "de" },
            { SystemLanguage.English, "en" },
            { SystemLanguage.Spanish, "es" },
            { SystemLanguage.French, "fr" },
            { SystemLanguage.Italian, "it" },
            { SystemLanguage.Japanese, "ja" },
            { SystemLanguage.Korean, "ko" },
            { SystemLanguage.Dutch, "nl" },
            { SystemLanguage.Portuguese, "pt" },
            { SystemLanguage.Russian, "ru" },
            { SystemLanguage.Turkish, "tr" }
        };


    private static readonly Dictionary<string, SystemLanguage> IdToLanguage =
        new()
        {
            { "de", SystemLanguage.German },
            { "en", SystemLanguage.English },
            { "es", SystemLanguage.Spanish },
            { "fr", SystemLanguage.French },
            { "it", SystemLanguage.Italian },
            { "ja", SystemLanguage.Japanese },
            { "ko", SystemLanguage.Korean },
            { "nl", SystemLanguage.Dutch },
            { "pt", SystemLanguage.Portuguese },
            { "ru", SystemLanguage.Russian },
            { "tr", SystemLanguage.Turkish }
        };


    public static string LanguageIdStringForType(SystemLanguage language) {
        return LanguageToId.TryGetValue(language, out var id) ? id : "en";
    }


    public static SystemLanguage LanguageForIdString(string idString) {
        if (string.IsNullOrEmpty(idString))
            return SystemLanguage.English;

        return IdToLanguage.TryGetValue(idString.ToLowerInvariant(), out var language) ? language : SystemLanguage.English;
    }
}
