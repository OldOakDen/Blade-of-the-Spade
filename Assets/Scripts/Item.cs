using UnityEngine;
using System;

public enum ItemType
{
    Coins,
    Jewelry,
    Militaries,
    Sculptures,
    Usual,
    Trash
}

public enum ItemMaterial
{
    Gold,
    Silver,
    Bronze,
    CopperBrass,
    Aluminium,
    Lead,
    Iron
}

public enum ItemPeriod
{
    StoneAge,
    BronzeAge,
    IronAge,
    ClassicalAntiquity,
    MiddleAges,
    EarlyModernPeriod,
    ModernPeriod
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item")]
public class Item : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string description;
    public GameObject prefab;
    public Sprite icon;
    public int points;

    public ItemType type;
    public ItemMaterial material;
    public ItemPeriod period;
}
