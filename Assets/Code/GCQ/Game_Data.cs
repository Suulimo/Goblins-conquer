using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UniRx;
using Unity.Mathematics;
using UnityEngine;

namespace GCQ
{
    public partial class Game_Data
    {
        public enum State
        {
            Err = -1,
            Init = 0,
            Battle_Scene,
            Battle_Result,
        }

        State current_state = State.Err;

        UniTask state_worker = UniTask.CompletedTask;


        public BoolReactiveProperty running = new BoolReactiveProperty();

        public void FSM_Start(State start_state) {
            current_state = start_state;

            _ = First_FSM_Start();
        }

        async UniTaskVoid First_FSM_Start() {
            await Task.Yield();
            Debug.Log("IsInjectedUniTaskPlayerLoop:" + (PlayerLoopHelper.IsInjectedUniTaskPlayerLoop()));
            await Enter_State(current_state);
            _ = Execute_State(current_state);
        }

        public async UniTask<bool> Goto_State(State next) {
            bool valid = (current_state, next) switch {
                (State.Init, State.Battle_Scene) => true,
                (State.Battle_Scene, State.Battle_Result) => true,
                (State.Battle_Result, State.Battle_Scene) => true,
                _ => false,
            };
            if (!valid)
                return false;

            await Leave_State(current_state);
            await Enter_State(next);

            current_state = next;
            _ = Execute_State(current_state);

            return false;
        }

        async UniTask Enter_State(State state) {
            state_worker = (state) switch {
                _ => UniTask.CompletedTask,
            };

            if (state_worker.GetAwaiter().IsCompleted == false)
                await state_worker;
        }

        async UniTask Leave_State(State state) {
            state_worker = (state) switch {
                _ => UniTask.CompletedTask,
            };

            if (state_worker.GetAwaiter().IsCompleted == false)
                await state_worker;
        }

        async UniTask Execute_State(State state) {
            state_worker = (state) switch {
                State.Init => Init_State_Execution(),
                _ => UniTask.CompletedTask,
            };

            if (state_worker.GetAwaiter().IsCompleted == false)
                await state_worker;
        }
    }
}