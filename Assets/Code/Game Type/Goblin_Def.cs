using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goblin_Data
{
    public string id;
    public int rank;

    public Goblin_Other_Data other;
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
    public int attack_power_base;
    public float attack_growth_rate = 1.1f;
    public string ability_id;
    public int hp_base;
    public float hp_growth_rate = 1.1f;
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
                attack_power_base = 3,
                ability_id = "GOBLIBILITY_000",
                hp_base = 50,
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
                attack_power_base = 5,
                ability_id = "GOBLIBILITY_000",
                hp_base = 50,
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
                attack_power_base = 10,
                ability_id = "GOBLIBILITY_000",
                hp_base = 200,
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
                attack_power_base = 15,
                ability_id = "GOBLIBILITY_000",
                hp_base = 200,
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
                attack_power_base = 15,
                ability_id = "GOBLIBILITY_000",
                hp_base = 300,
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
                attack_power_base = 20,
                ability_id = "GOBLIBILITY_000",
                hp_base = 250,
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
                attack_power_base = 20,
                ability_id = "GOBLIBILITY_000",
                hp_base = 300,
            },
        },
    };
}
