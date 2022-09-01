using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Human_Data
{
    public string id;
    public int rank;

    [System.NonSerialized]
    public Human_Other_Data other;

    [System.NonSerialized]
    public Human_Battle_Data battle;

}

// 這個是固定的資訊、數值，定義先放在本地端列表(...CatDef)，如果想放server再改
[System.Serializable]
public class Human_Other_Data
{
    public int beauty;
    public string name;
    public string sprite;
}

[System.Serializable]
public class Human_Battle_Data
{
    public int[] attack_power;
    public string ability_id;
}

public static class Human_Def
{
    public static Human_Data[] Default_Human_List = new Human_Data[]
    {
        new Human_Data() {
            id = "HUMAN_001",
            rank = 1,
            other = new Human_Other_Data
            {
                beauty = 3,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (1)",
            },
            battle = new Human_Battle_Data
            {
                attack_power = new [] { 5, 10, 15, 20, 25, },
                ability_id = "ABILITY_000",
            },
        },
        new Human_Data() {
            id = "HUMAN_002",
            rank = 2,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (2)",
            },
            battle = new Human_Battle_Data
            {
                attack_power = new [] { 5, 10, 15, 20, 25, },
                ability_id = "ABILITY_000",
            },
        },
        new Human_Data() {
            id = "HUMAN_003",
            rank = 3,
            other = new Human_Other_Data
            {
                beauty = 1,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (3)",
            },
            battle = new Human_Battle_Data
            {
                attack_power = new [] { 5, 10, 15, 20, 25, },
                ability_id = "ABILITY_000",
            },
        },
    };
}
