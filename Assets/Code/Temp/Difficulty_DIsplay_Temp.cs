using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class Difficulty_DIsplay_Temp : MonoBehaviour
{
    public TMP_Text timer_text;

    void Awake() {
        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {
            UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ => {
                timer_text.text = GCQ.IGame_Scope.battle_scope.data.difficulty.ToString();
            }).AddTo(this);
        }).AddTo(this);
    }
}
