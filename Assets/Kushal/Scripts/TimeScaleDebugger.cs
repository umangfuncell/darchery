using UnityEngine;

public class TimeScaleDebugger : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 30), "Time.timeScale: " + Time.timeScale);
    }
}
