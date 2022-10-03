using UnityEngine;

namespace GCQ
{
    [System.Serializable]
    public class Goblin_Spec
    {
        public string id;
        public int rank;

        public Goblin_Other_Spec other;
        public Goblin_Combat_Spec combat;

    }

    // 這個是固定的資訊、數值，定義先放在本地端列表(...CatDef)，如果想放server再改
    [System.Serializable]
    public class Goblin_Other_Spec
    {
        public int beauty;
        public string name;
        public string sprite;
        public string baba;
        public string mama;
    }

    [System.Serializable]
    public class Goblin_Combat_Spec
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
        public static Goblin_Spec[] Default_Goblin_List = new Goblin_Spec[]
        {
        new Goblin_Spec() {
            id = "GOBLIN_001",
            rank = 1,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLIN",
                sprite = "Goblin ("+Random.Range(1, 3)+")",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 3,
                attack_power_base = 3,
                ability_id = "GOBLIBILITY_000",
                hp_base = 50,
            },
        },
        new Goblin_Spec() {
            id = "GOBLIN_002",
            rank = 1,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLINP",
                sprite = "GoblinP ("+Random.Range(1, 3)+")",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 3,
                attack_power_base = 5,
                ability_id = "GOBLIBILITY_000",
                hp_base = 50,
            },
        },
        new Goblin_Spec() {
            id = "GOBLIN_003",
            rank = 3,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLINHOB",
                sprite = "GoblinHob",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 4,
                attack_power_base = 10,
                ability_id = "GOBLIBILITY_000",
                hp_base = 200,
            },
        },
        new Goblin_Spec() {
            id = "GOBLIN_004",
            rank = 1,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLINCHAMPION",
                sprite = "GoblinChampion",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 3,
                attack_power_base = 15,
                ability_id = "GOBLIBILITY_000",
                hp_base = 200,
            },
        },
        new Goblin_Spec() {
            id = "GOBLIN_005",
            rank = 1,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLINPALADIN",
                sprite = "GoblinPaladin",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 3,
                attack_power_base = 15,
                ability_id = "GOBLIBILITY_000",
                hp_base = 300,
            },
        },
        new Goblin_Spec() {
            id = "GOBLIN_006",
            rank = 1,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLINSHAMAN",
                sprite = "GoblinShaman",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 3,
                attack_power_base = 20,
                ability_id = "GOBLIBILITY_000",
                hp_base = 250,
            },
        },
        new Goblin_Spec() {
            id = "GOBLIN_007",
            rank = 1,
            other = new Goblin_Other_Spec
            {
                beauty = 1,
                name = "GOBLINLORD",
                sprite = "GoblinLord",
            },
            combat = new Goblin_Combat_Spec
            {
                attack_cd = 3,
                attack_power_base = 20,
                ability_id = "GOBLIBILITY_000",
                hp_base = 300,
            },
        },
        };
    }
}