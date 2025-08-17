using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Item", menuName ="Create Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
}

public enum ItemType
{
    coal,
    emptyBottle,
    paper,
    smallRedPotion,
    mediumRedPotion,
    largeRedPotion,
    beer,
    wine,
    spirit,
    smallGreenPotion,
    mediumGreenPotion,
    largeGreenPotion,
    smallBluePotion,
    mediumBluePotion,
    largeBluePotion,
    cheese,
    apple,
    bread,
    fishSteak,
    ham,
    meat,
    waterBottle,
    leatherBoots,
    ironBoots,
    obsidian,
    crystal,
    ruby,
    woodenSword,
    ironSword,
    goldenSword,
    bluntAxe,
    bluntShovel,
    bluntPickaxe,
    woodenArmour,
    leatherArmour,
    ironArmour,
    magicWand,
    woodenStaff,
    emeraldStaff,
    candle,
    lantern,
    torch,
    scroll,
    runeStone,
    book
}