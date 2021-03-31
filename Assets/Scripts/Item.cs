using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public enum ItemType
    {
        WineScrew,
        Nail,
        HighHeel,
    }

    public ItemType itemType;
    public int amount;
}
