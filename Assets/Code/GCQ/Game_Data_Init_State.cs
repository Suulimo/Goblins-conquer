using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UniRx;
using Unity.Mathematics;
using UnityEngine;

namespace GCQ
{
    public partial class Game_Data
    {
        async UniTask Init_State_Execution() {


            // 確認需要引用的scene跟game component都已經實例化
            var w1 = UniTask.WaitUntil(() => IGame_Scope.battle_scope.battle_scene_ref != null);
            var w2 = UniTask.WaitUntil(() => IGame_Scope.battle_scope.battlefield_main_ref != null);

            await UniTask.WhenAll(w1, w2);

            var (n1, n2, n3) = IGame_Scope.battle_scope.battlefield_main_ref.Use.MakeMap(
                IGame_Scope.battle_scope.Add_Slot_Data
            );

            MessageBroker.Default.Publish(new Battle_Scope_Init_Complete_Trigger { });
            MessageBroker.Default.Publish(new Battle_Scope_Run_Trigger { });
        }
    }
}