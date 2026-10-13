using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitToMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        InvokeRepeating("Exit", 0.5f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void NotifyGameExit();
#else
    private static void NotifyGameExit() => Debug.Log("[ExitToMainMenu] NotifyGameExit (editor stub)");
#endif

    public void Exit()
    {
        NotifyGameExit();
    }
}
