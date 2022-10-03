using UnityEngine;
using Cysharp.Threading.Tasks;

public class Restart_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        UniTask.Void(async () => {
            await UniTask.SwitchToMainThread();
            Game_Control.game_control.SafeCancellationDispose();
            Data_Manager.data_manager.Force_Return_Pool();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle Scene");
            GCQ.IGame_Scope.Restart();
        });
    }

}


