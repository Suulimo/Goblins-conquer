using UnityEngine;

public class Speed_Button_Temp : MonoBehaviour
{
    public float time_scale;

    private void OnMouseUpAsButton() {
        Time.timeScale = time_scale;
    }
}