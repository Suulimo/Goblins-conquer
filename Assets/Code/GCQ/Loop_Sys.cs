using static GCQ.IGame_Scope;

namespace GCQ
{

    public static class Loop_Sys
    {
        public static void Run_Last_Update(float dt) {

            Battle_Sys.Run(battle_scope, dt);
        }
    }
}
