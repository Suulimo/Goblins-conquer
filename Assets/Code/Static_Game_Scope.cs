using UnityEngine;

public static class Static_Game_Scope
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void DomainReload() {
        Debug.Log("Static_Game_Scope reset.");

        game_state = new Game_State();
    }

    public static Game_State game_state;
    public static Battle_Scope battle_scope;
}