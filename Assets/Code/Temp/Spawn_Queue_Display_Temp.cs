using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;

public class Spawn_Queue_Display_Temp : MonoBehaviour
{
    public Sprite male;
    public Sprite female;

    public SpriteRenderer[] displays;

    void Awake() {
        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {

            void refresh(int count) {
                int empty_to = 5 - count;
                for (int i = 0; i < empty_to; i++) {
                    displays[i].sprite = null;

                }

                for (int i = empty_to; i < 5; i++) {
                    if (i < count) {
                        displays[i].sprite = (GCQ.Static_Game_Scope.battle_scope.data.spawn_queue[i - empty_to] > 0) ? female : male;
                    }
                }
            }

            refresh(GCQ.Static_Game_Scope.battle_scope.data.spawn_queue.Count);

            GCQ.Static_Game_Scope.battle_scope.data.spawn_queue.ObserveCountChanged().Subscribe(count => {
                refresh(count);
            }); 
        }).AddTo(this);
    }
}
