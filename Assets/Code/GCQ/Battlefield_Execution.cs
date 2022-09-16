using UnityEngine;

namespace GCQ
{
    public static class Battlefield_Execution
    {

        public static void On_Spawn_Goblin(Goblin_Spawned args) {
            var to_slot = Static_Game_Scope.battlefield_main_ref.Use.Get_Slot_Monobe(Slot_Type.A, args.slot_id);
            if (to_slot != null) {
                var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

                var render = p.gameObj.GetComponent<SpriteRenderer>();
                render.sprite = Data_Manager.data_manager.share_pic.goblin_pic[args.goblin_data.other.sprite];
                render.flipX = true;

                var pawn_control = p.gameObj.GetComponent<Pawn_Monobe>();
                if (pawn_control != null) {
                    pawn_control.Set_Goblin_Pawn(to_slot.Data.goblin);
                }

                to_slot.Set_Pawn_Object(p.gameObj);
            }
        }

        public static void On_Spawn_Human(Human_Spawned args) {
            var to_slot = Static_Game_Scope.battlefield_main_ref.Use.Get_Slot_Monobe(Slot_Type.B, args.slot_id);
            if (to_slot != null) {
                var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

                var render = p.gameObj.GetComponent<SpriteRenderer>();
                render.sprite = Data_Manager.data_manager.share_pic.human_pic[args.human_data.other.sprite];
                render.flipX = false;

                var pawn_control = p.gameObj.GetComponent<Pawn_Monobe>();
                if (pawn_control != null) {
                    pawn_control.Set_Human_Pawn(to_slot.Data.human);
                }

                to_slot.Set_Pawn_Object(p.gameObj);
            }

        }

        public static void On_Spawn_Bed(Bed_Spawned args) {
            var to_slot = Static_Game_Scope.battlefield_main_ref.Use.Get_Slot_Monobe(Slot_Type.C, args.slot_id);
            if (to_slot != null) {
                var p = SpawnPrefabSystem.Rent("Temp Slot Display", to_slot.transform, false);

                var render = p.gameObj.GetComponent<SpriteRenderer>();
                render.sprite = Data_Manager.data_manager.share_pic.bed_pic[args.human_data.other.sprite_bed];
                render.flipX = false;

                var slot_display_component = p.gameObj.GetComponent<Slot_Display_Monobe>();
                if (slot_display_component != null) {
                    slot_display_component.Set_Bed_Pawn(to_slot.Data.bed);
                }

                to_slot.Set_Slot_Display_Object(p.gameObj);
            }
        }
    }
}