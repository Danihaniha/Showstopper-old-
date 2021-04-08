using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory
{
    private List<Item> itemList;

    public Inventory()
    {
        itemList = new List<Item>();

        AddItem(new Item { itemType = Item.ItemType.WineScrew, amount = 1 });
        AddItem(new Item { itemType = Item.ItemType.Nail, amount = 1 });
        AddItem(new Item { itemType = Item.ItemType.HighHeel, amount = 1 });
    }

    public void AddItem(Item item)
    {
        itemList.Add(item);
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }
}
