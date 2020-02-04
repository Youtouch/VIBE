using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

using UnityEngine;


public class ItemData : SerializedScriptableObject
{
    public Sprite icon;
    public ItemType ItemType;
    public GameObject gameObj;
}


public enum ItemType
{
    Lens,
    HugBlock,
    Boots,
    Boost,
}