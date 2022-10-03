using UnityEngine;

namespace GCQ
{
    public partial interface IGame_Scope
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void DomainReload() {
            Debug.Log("Static_Game_Scope reset.");

            battle_scope = new Battle_Scope();
            battle_scope.Init_Scope();

            game_data = new Game_Data();
            game_data.FSM_Start(Game_Data.State.Init);
        }

        public static void Restart() {
            Debug.Log("Static_Game_Scope reset.");

            battle_scope = new Battle_Scope();
            battle_scope.Init_Scope();

            game_data = new Game_Data();
            game_data.FSM_Start(Game_Data.State.Init);
        }

        // data
        public static Game_Data game_data;
        public static Battle_Scope battle_scope;

    }
}