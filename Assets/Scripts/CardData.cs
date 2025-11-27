using UnityEngine;
using System;

[Serializable]
public class CardData
{
    public int id;
    public string name;
    public int cost;
    public int power;
    public AbilityData ability;
}

[Serializable]

public class AbilityData
{
    public string type;
    public int value;
}

[Serializable]
public class CardCollection
{
    public CardData[] cards;
}
