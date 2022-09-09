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
    public float attack_cd;
    public int attack_power;
    public string ability_id;
    public int hp;
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
                beauty = 1,
                name = "GOBLIN",
                sprite = "Goblin ("+Random.Range(1, 3)+")",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 3,
                attack_power = 3,
                ability_id = "GOBLIBILITY_000",
                hp = 50,
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_002",
            rank = 1,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLINP",
                sprite = "GoblinP ("+Random.Range(1, 3)+")",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 3,
                attack_power = 5,
                ability_id = "GOBLIBILITY_000",
                hp = 50,
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_003",
            rank = 3,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLINHOB",
                sprite = "GoblinHob",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 4,
                attack_power = 10,
                ability_id = "GOBLIBILITY_000",
                hp = 200,
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_004",
            rank = 1,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLINCHAMPION",
                sprite = "GoblinChampion",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 3,
                attack_power = 15,
                ability_id = "GOBLIBILITY_000",
                hp = 200,
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_005",
            rank = 1,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLINPALADIN",
                sprite = "GoblinPaladin",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 3,
                attack_power = 15,
                ability_id = "GOBLIBILITY_000",
                hp = 300,
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_006",
            rank = 1,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLINSHAMAN",
                sprite = "GoblinShaman",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 3,
                attack_power = 20,
                ability_id = "GOBLIBILITY_000",
                hp = 250,
            },
        },
        new Goblin_Data() {
            id = "GOBLIN_007",
            rank = 1,
            other = new Goblin_Other_Data
            {
                beauty = 1,
                name = "GOBLINLORD",
                sprite = "GoblinLord",
            },
            battle = new Goblin_BattleData
            {
                attack_cd = 3,
                attack_power = 20,
                ability_id = "GOBLIBILITY_000",
                hp = 300,
            },
        },
    };
}
