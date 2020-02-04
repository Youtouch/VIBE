using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BuffData : SerializedScriptableObject
{
    public Stats statToImpact;
    public BuffStyle methodImpact;
    public float value;
    public Sprite sprite;
}

public enum Stats
{
    Speed,
    Throw,
    Jump
}

public enum BuffStyle
{
    multiply,
    add
}