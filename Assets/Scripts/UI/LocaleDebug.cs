using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            var current = LocalizationSettings.SelectedLocale;
            int index = locales.IndexOf(current);
            int next = (index + 1) % locales.Count;
            LocalizationSettings.SelectedLocale = locales[next];
            Debug.Log("Locale: " + locales[next].LocaleName);
        }
    }
}