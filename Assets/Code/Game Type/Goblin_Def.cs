using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goblin_Data
{
    public string id;
    public int rank;

    [System.NonSerialized]
    public Goblin_Other_Data other;

    [System.NonSerialized]
    public Goblin_BattleData battle;

}

// 這個是固定的資訊、數值，定義先放在本地端列表(...CatDef)，如果想放server再改
[System.Serializable]
public class Goblin_Other_Data
{
    public int beauty;
    public string name;
    public string sprite;
}

[System.Serializable]
public class Goblin_BattleData
{
    public int[] attack_power;
    public string ability_id;
}

public static class Goblin_Def
{
    public static Goblin_Data[] Default_Goblin_List = new Goblin_Data[]
    {
        new Goblin_Data() {
            id = "GOBLIN_001",
            rank = 1,
            other = new Goblin_Other_Data
            {
                beauty = 3,
                name = "GOBLIN_NAME_001",
                sprite = "Goblin (1)",
            },
            battle = new Goblin_BattleData
            {
                attack_power = new [] { 5, 10, 15, 20, 25, },
                ability_id = "GOBLIBILITY_000",
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_002",
            rank = 2,
            other = new Goblin_Other_Data
            {
                beauty = 2,
                name = "GOBLIN_NAME_001",
                sprite = "Goblin (2)",
            },
            battle = new Goblin_BattleData
            {
                attack_power = new [] { 5, 10, 15, 20, 25, },
                ability_id = "GOBLIBILITY_000",
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_003",
            rank = 3,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLIN_NAME_001",
                sprite = "Goblin (3)",
            },
            battle = new Goblin_BattleData
            {
                attack_power = new [] { 5, 10, 15, 20, 25, },
                ability_id = "GOBLIBILITY_000",
            },
        },
    };
}
