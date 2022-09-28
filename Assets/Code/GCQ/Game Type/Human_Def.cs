namespace GCQ
{
    [System.Serializable]
    public class Human_Spec
    {
        public string id;
        public int rank;

        public Human_Other_Spec other;
        public Human_Combat_Spec combat;

    }

    // 這個是固定的資訊、數值，定義先放在本地端列表(...CatDef)，如果想放server再改
    [System.Serializable]
    public class Human_Other_Spec
    {
        public int beauty;
        public string name;
        public string sprite;
        public string sprite_bed;
        public int femininity; // 0 為男性， 大於0為女
    }

    [System.Serializable]
    public class Human_Combat_Spec
    {
        public float attack_cd;
        public int attack_power_base;
        public float attack_growth_rate = 1.05f;
        public string ability_id;
        public float bed_spawn_cd;
        public int hp_base;
        public float hp_growth_rate = 1.05f;
        public Item_Drop_Change[] item_drop_chance;
    }

    [System.Serializable]
    public struct Item_Drop_Change
    {
        public Battlefield_Use.Item_Test item;
        public float drop_chance;
    }

    public static class Human_Def
    {
        public static Human_Spec[] Default_Human_List = new Human_Spec[]
        {
        new Human_Spec() {
            id = "FEMALE_001",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 0,
                name = "",
                sprite = "EnemyFemale (1)",
                sprite_bed = "Bed (1)",
                femininity = 1,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 50,
                bed_spawn_cd = 20,
            },
        },
        new Human_Spec() {
        id = "FEMALE_002",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyFemale (2)",
                sprite_bed = "Bed (2)",
                femininity = 1,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 6,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 70,
                bed_spawn_cd = 20,
            },
        },
        new Human_Spec() {
            id = "FEMALE_003",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 2,
                name = "",
                sprite = "EnemyFemale (3)",
                sprite_bed = "Bed (3)",
                femininity = 1,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 7,
                ability_id = "ABILITY_000",
                hp_base = 50,
                bed_spawn_cd = 20,
            },
        },
        new Human_Spec() {
            id = "FEMALE_004",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 3,
                name = "",
                sprite = "EnemyFemale (4)",
                sprite_bed = "Bed (4)",
                femininity = 1,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 4,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 50,
                bed_spawn_cd = 20,
            },
        },
        new Human_Spec() {
            id = "FEMALE_005",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 4,
                name = "",
                sprite = "EnemyFemale (5)",
                sprite_bed = "Bed (5)",
                femininity = 1,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 6,
                ability_id = "ABILITY_000",
                hp_base = 60,
                bed_spawn_cd = 20,
            },
        },
        new Human_Spec() {
            id = "FEMALE_006",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 5,
                name = "",
                sprite = "EnemyFemale (6)",
                sprite_bed = "Bed (6)",
                femininity = 1,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 4.5f,
                attack_power_base = 6,
                ability_id = "ABILITY_000",
                hp_base = 50,
                bed_spawn_cd = 20,
            },
        },
        };
        public static Human_Spec[] Default_Male_Human_List = new Human_Spec[]
        {
        new Human_Spec() {
            id = "MALE_001",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (1)",
                sprite_bed = "",
                femininity = 0,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 60,
            },
        },
        new Human_Spec() {
            id = "MALE_002",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (2)",
                sprite_bed = "",
                femininity = 0,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 60,
            },
        },
        new Human_Spec() {
            id = "MALE_003",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (3)",
                sprite_bed = "",
                femininity = 0,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 60,
            },
        },
        new Human_Spec() {
            id = "MALE_004",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (4)",
                sprite_bed = "",
                femininity = 0,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 60,
            },
        },
        new Human_Spec() {
            id = "MALE_005",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (5)",
                sprite_bed = "",
                femininity = 0,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 60,
            },
        },
        new Human_Spec() {
            id = "MALE_006",
            rank = 1,
            other = new Human_Other_Spec
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (6)",
                sprite_bed = "",
                femininity = 0,
            },
            combat = new Human_Combat_Spec
            {
                attack_cd = 5,
                attack_power_base = 5,
                ability_id = "ABILITY_000",
                hp_base = 60,
            },
        },
        };
    }
}