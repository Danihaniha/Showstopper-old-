using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.WineScrew:     return ItemAssets.Instance.wineScrewSprite;
            case ItemType.Nail:          return ItemAssets.Instance.nailSprite;
            case ItemType.HighHeel:      return ItemAssets.Instance.highHeelSprite;
        }
    }
}
