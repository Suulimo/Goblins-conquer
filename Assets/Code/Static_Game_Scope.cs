using UnityEngine;

public static class Static_Game_Scope
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void DomainReload() {
        Debug.Log("Static_Game_Scope reset.");

        battle_scope = new Battle_Scope();
    }

    public static Battle_Scope battle_scope;
}