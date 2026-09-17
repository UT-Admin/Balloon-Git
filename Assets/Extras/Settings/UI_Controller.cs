using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Controller : MonoBehaviour
{
    //public UIHandler howToPlay_Panel;
    public SettingsPanelHandler settingsHandler;
    /*public InsufficientPanelHandler insufficientPanelHandler;*/
    public List<UIHandler> openedPages;
    //public GameObject LoadingPanel;
    public static UI_Controller instance;
    public GameObject Welcomepop;

    private void Awake()
    {
        instance = this;
    }

    #region Adding & Removing CurrentPages
    public void AddToOpenPages(UIHandler handler)
    {
        if (openedPages.Contains(handler))
        {
            openedPages.Remove(handler);
        }
        openedPages.Add(handler);
    }

    public void RemoveFromOpenPages(UIHandler handler)
    {
        if (openedPages.Contains(handler))
            openedPages.Remove(handler);

    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape) && openedPages.Count > 0 && openedPages.Count != 0)
        //    openedPages[openedPages.Count - 1].OnBack();
    }
    #endregion

    #region EXIT GAME
    public void ExitWebGL()
    {
        /*APIController.CloseWindow();*/
        // SettingsPanelHandler.instance.RedirectingPanel.SetActive(true);
        //InvokeRepeating(nameof(ShowCloseWindow), 1f, 5f);
        GameController.instance.RedirectionPanel.SetActive(true);
        InvokeRepeating(nameof(ShowCloseWindow), 1f, 5f);
    }
    #endregion

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void NotifyGameExit();
#else
    private static void NotifyGameExit() => Debug.Log("[SettingsPanelHandler] NotifyGameExit (editor stub)");
#endif

    void ShowCloseWindow()
    {
#if UNITY_WEBGL
        NotifyGameExit();
#endif
    }

    /*    void ShowCloseWindow()
        {
    #if UNITY_WEBGL
            APIController.CloseWindow();
    #endif
        }*/

    #region Sound Properties
    public bool IsSoundOn()
    {
        return settingsHandler.SoundToggle.isOn;
    }

    public void PlayButtonSound()
    {
        MasterAudioController.instance.StopAudio(AudioEnum.buttonClick);
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
    }
    #endregion
}
