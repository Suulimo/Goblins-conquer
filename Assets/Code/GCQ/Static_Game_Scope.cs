using UnityEngine;

namespace GCQ
{
    public static class Static_Game_Scope
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void DomainReload() {
            Debug.Log("Static_Game_Scope reset.");

            game_scope_data = new Game_Scope_Data();
            game_scope_data.FSM_Start(Game_Scope_Data.State.Init);
        }

        public static void Restart() {
            Debug.Log("Static_Game_Scope reset.");

            game_scope_data = new Game_Scope_Data();
            game_scope_data.FSM_Start(Game_Scope_Data.State.Init);
        }

        // data
        public static Game_Scope_Data game_scope_data;
        public static Battle_Scope battle_scope;

        // unity presenter
        [ClearOnReload] public static Battle_Scene battle_scene_ref;
        [ClearOnReload] public static Battlefield_Main_Monobe battlefield_main_ref;
    }
}