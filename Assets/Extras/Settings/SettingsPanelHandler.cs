using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IQPlay.GameClient;

public class SettingsPanelHandler : UIHandler
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    public static SettingsPanelHandler instance;
    [SerializeField] RectTransform PanelTransform;
    [SerializeField] float XOffPos = 700;
    [SerializeField] Button ExitBtn;
    public Toggle SoundToggle;
    [SerializeField] UnityAction SwapSpriteSequence;
    [SerializeField] Button emptySpaceBtn;
    [SerializeField] GameObject emptySpaceObj;
    public GameObject HTP;
    [SerializeField] private Button betHistory_Btn;
    [SerializeField] private GameObject betHistory;
    [SerializeField] string playerName;
    public TMP_Text playerNameTxt;
    public Button howtoPlayBtn;
    [SerializeField] Toggle MusicToggle;
    public GameObject RedirectingPanel;
    public Button settingsBtn;
    public Button settingsCloseBtn;
    [SerializeField] private Button gameLimits_Btn;
    public GameObject GameLimits;
    [SerializeField] Button addCashBtn;
    #endregion  ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    private void Awake()
    {
        instance = this;
        ExitBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); OnExitBtnClick(); });
        emptySpaceBtn.onClick.AddListener(() =>
        {
            HideMe();
        });
        SoundToggle.onValueChanged.AddListener((state) => { ToggleSound(state); });
        MusicToggle.onValueChanged.AddListener((state) => { SetMusicVolume(state); });
        gameLimits_Btn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); ShowGameLimitScreen(); });
        betHistory_Btn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); ShowBetHistoryScreen(); });
        addCashBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); ShowDeposit(); });
    }
    private void Start()
    {
        /*APIController.instance.OnAuthDataUpdate += OnUserDetailsUpdate;*/
        SessionEvents.OnUserDetailsUpdate += OnUserDetailsUpdate;
    }
    public void ShowBetHistoryScreen()
    {
        betHistory.SetActive(true);
        HideMe();
        settingsBtn.gameObject.SetActive(true);
        settingsCloseBtn.gameObject.SetActive(false);
    }
    public void ToggleSound(bool value)
    {
        SoundToggle.isOn = value;
        DebugHelper.Log("Sound volume --> " + value);
        if (SoundToggle.isOn)
            MasterAudioController.instance.SetVolumeUnMute();
        else
            MasterAudioController.instance.SetVolumeMute();

        if (SoundToggle.isOn)
            MasterAudioController.instance.PlayAudio(AudioEnum.toggle);

        CancelInvoke(nameof(UpdateMusic));
        Invoke(nameof(UpdateMusic), 0.5f);
    }
    public void SetMusicVolume(bool _state)
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.toggle);
        MusicToggle.isOn = _state;
        MasterAudioController.instance.BackgroundAudio.SetBgmSoundStatus(MusicToggle.isOn);
        CancelInvoke(nameof(UpdateMusic));
        Invoke(nameof(UpdateMusic), 0.5f);
    }
    public override void ShowMe()
    {
        base.ShowMe();
        PanelTransform.DOKill();
        PanelTransform.gameObject.SetActive(true);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        PanelTransform.DOAnchorPosX(25f, 0.1f);
        emptySpaceObj.SetActive(true);
    }
    public override void HideMe()
    {
        DebugHelper.Log("Settings Panel Open ");
        UI_Controller.instance.PlayButtonSound();
        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-1000f, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { gameObject.SetActive(false); PanelTransform.gameObject.SetActive(false); });
        SwapSpriteSequence?.Invoke();
        emptySpaceObj.SetActive(false);
        DebugHelper.Log("SettingPanel__Close :");
        settingsCloseBtn.gameObject.SetActive(false);
        settingsBtn.gameObject.SetActive(true);
    }
    public void Welcomebtn_OFF()
    {
        DebugHelper.Log("Welcome button OFF ");
        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-1000f, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { gameObject.SetActive(false); PanelTransform.gameObject.SetActive(false); });
        SwapSpriteSequence?.Invoke();
        emptySpaceObj.SetActive(false);
    }
    public void OnExitBtnClick()
    {
        UI_Controller.instance.ExitWebGL();
        UI_Controller.instance.settingsHandler.HideMe();
    }
    public void CallingSettingPanel()
    {
        if (!UI_Controller.instance.settingsHandler.gameObject.activeSelf)
        {
            ShowMe();
            UI_Controller.instance.PlayButtonSound();
            DebugHelper.Log("SettingPanel__Open :");
            settingsBtn.gameObject.SetActive(false);
            settingsCloseBtn.gameObject.SetActive(true);
        }
        else
            HideMe();
    }
    public void ShowDeposit()
    {
#if UNITY_WEBGL
        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (!success)
                return;
            else
            {
                if (!GameController.instance.demo)
                    APIController.instance.OnClickDepositBtn();
            }
        });
#endif
    }
    private void UpdateMusic()
    {
        /*APIController.instance.authentication.sound = SoundToggle.isOn;
        APIController.instance.authentication.music = MusicToggle.isOn;
        LocalStorage.Save("Lootrix_sound", APIController.instance.authentication.sound ? "true" : "false");
        LocalStorage.Save("Lootrix_music", APIController.instance.authentication.music ? "true" : "false");*/
        SessionState.sound = SoundToggle.isOn;
        SessionState.music = MusicToggle.isOn;
        LocalStorage.Save("Lootrix_sound", SessionState.sound ? "true" : "false");
        LocalStorage.Save("Lootrix_music", SessionState.music ? "true" : "false");
        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (success)
            {
                LocalStorage.Save("Lootrix_sound", SessionState.sound ? "true" : "false");
                LocalStorage.Save("Lootrix_music", SessionState.music ? "true" : "false");
                /*APIController.instance.UpdateAudioSettings();*/
            }
            else
                Invoke(nameof(UpdateMusic), 0.5f);
        });
    }
    bool Updatetoggle = false;
    public void SetToggleValueFromAPI(bool sound, bool music)
    {
        SoundToggle.isOn = sound;
        MusicToggle.isOn = music;
    }
    public void ShowGameLimitScreen()
    {
        GameLimits.SetActive(true);
        HideMe();
        settingsBtn.gameObject.SetActive(true);
        settingsCloseBtn.gameObject.SetActive(false);
    }
    void OnUserDetailsUpdate()
    {
        string sounds = LocalStorage.Load("Lootrix_sound");
        string musics = LocalStorage.Load("Lootrix_music");
        bool sound = false;
        bool music = false;

        if (string.IsNullOrEmpty(sounds) && string.IsNullOrEmpty(musics))
        {
            sound = true;
            music = false;
        }
        else
        {
            sound = sounds == "true";
            music = musics == "true";
        }
        DebugHelper.Log("Updating SOundddd" + sounds + "Music" + music);
        /*APIController.instance.authentication.sound = sound;
        APIController.instance.authentication.music = music;*/
        SessionState.sound = sound;
        SessionState.music = music;
        SetToggleValueFromAPI(sound, music);
    }
}
