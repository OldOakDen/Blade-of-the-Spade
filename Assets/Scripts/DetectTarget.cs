using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public string targetSound;
        
    public enum ItemPeriod
    {
        A_StoneAge, // 2.5 million years ago - 3,000 BCE
        B_BronzeAge, // 3,000 BCE - 1,200 BCE
        C_IronAge, // 1,200 BCE - 500 CE
        D_ClassicalAntiquity, // 500 BCE - 500 CE
        E_MiddleAges, // 500 CE - 1500 CE
        F_EarlyModernPeriod, // 1500 CE - 1800 CE
        G_ModernPeriod // 1800 CE - Present
    }

    public enum ItemMaterial
    {
        G_Gold,
        S_Silver,
        B_Bronze,
        C_CopperBrass,
        A_Aluminium,
        L_Lead,
        I_Iron
    }
    public enum ItemType
    {
        C_Coins,
        J_Jewelry,
        M_Militaries,
        S_Sculptures,
        U_Usual,
        T_Trash
    }

    public string itemID;
    public string itemName;
    //public string description;
    public Sprite icon; //2D ikona predmetu (nebo nahled) - ZATIM NEVYUZITO 
    public int previewResize = 25; //jak moc skejlnout objekt v preview modu aby byl pekne videt
    public int damageResistance = 5; //odolnost predmetu proti poskozeni pri vyzvednuti/vykopani (cim vetsi cislo, tim vetsi odolnost - NEJSEM SI ALE JIST => DORESIT!)
    public int rarity; //rarita predmetu (0-5?), bude vyuzita k vypoctu bonusu za predmet - ZATIM NEVYUZITO

    public ItemPeriod period; //casove obdobi, ze ktereho nalez pochazi
    public ItemMaterial material; // material predmetu
    public ItemType type; //typ predmetu
    
    private void Start()
    {
        SetMaterialSound(material);
    }

    private void SetMaterialSound(ItemMaterial material)
    {
        switch (material)
        {
            case ItemMaterial.G_Gold:
                targetSound = "MatSound_Gold";
                break;
            case ItemMaterial.S_Silver:
                targetSound = "MatSound_Silver";
                break;
            case ItemMaterial.B_Bronze:
                targetSound = "MatSound_Bronze";
                break;
            case ItemMaterial.C_CopperBrass:
                targetSound = "MatSound_Copbr";
                break;
            case ItemMaterial.L_Lead:
                targetSound = "MatSound_Lead";
                break;
            case ItemMaterial.A_Aluminium:
                targetSound = "MatSound_Aluminium";
                break;
            case ItemMaterial.I_Iron:
                targetSound = "MatSound_Iron";
                break;
            default:
                targetSound = "Unknown";
                break;
        }
    }
}
