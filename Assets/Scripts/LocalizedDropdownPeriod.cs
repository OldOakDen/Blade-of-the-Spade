using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizedDropdownPeriod : MonoBehaviour
{
    public TMP_Dropdown tmpDropdown;

    public enum ItemCategory
    {
        Period, Material, Type
    }

    public ItemCategory itemCategory;

    void Start()
    {
        // Pøidání novıch poloek
        //AddDropdownItem("New Item 1");
        //AddDropdownItem("New Item 2");

        // Zamìnìní existující poloky
        //ReplaceDropdownItem(0, "Replaced Item");

        // Získání lokalizovaného textu asynchronnì
        tmpDropdown.options[1].text = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("InitialTable", "dtct_hint_spacetodtct").Result; //pokus
        tmpDropdown.RefreshShownValue();
    }

    public void AddDropdownItem(string newItem)
    {
        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData();
        newOption.text = newItem;
        tmpDropdown.options.Add(newOption);
        tmpDropdown.RefreshShownValue();
    }

    public void ReplaceDropdownItem(int index, string newItem)
    {
        if (index >= 0 && index < tmpDropdown.options.Count)
        {
            tmpDropdown.options[index].text = newItem;
            tmpDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogError("Index out of range");
        }
    }
}
