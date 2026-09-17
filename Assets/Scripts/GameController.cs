using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Spine.Unity;
using System.Xml.Linq;
using UnityEditor;
using IQPlay.GameClient;

public class GameController : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    [Header("Float")]
    [SerializeField] public float betAmount = 1f;  // Initial bet amount
    public float Multiplier = 0.00f;  // Initial multiplier value
    public string Mstring;  // Initial multiplier value in String
    [SerializeField] float incrementRate = 1.01f;  // Increment rate
    [SerializeField] float incrementInterval = 0.2f;  // Time interval between increments in seconds
    [SerializeField] float timeSinceLastIncrement = 0f;   // Timer to track time since last increment
    [SerializeField] float holdHeight = 2f; // Minimum time to hold the button
    [SerializeField] string timeHold;   // Timer to track time 
    public float TakeCash;  // TakeCash
    [SerializeField] double TotalAmount = 250.00f;  // Total Amount
    public float Difference;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Script")]
    [SerializeField] KeyBoardHandler keyBoard;
    [SerializeField] SettingsPanelHandler settingsPanelHandler;
    [SerializeField] SkeletonAnimation skeletonAnimation;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Buttons")]
    [SerializeField] Button HeatBtn;
    [SerializeField] Button takeCashbutton;
    [SerializeField] Button holdButton;
    [SerializeField] TMP_Text[] unSelectedBtnTxt;
    [SerializeField] TMP_Text[] SelectedBtnTxt;
    [SerializeField] Button menuButton;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI bet Buttons")]
    [SerializeField] Button button_1;
    [SerializeField] Button button_2;
    [SerializeField] Button button_5;
    [SerializeField] Button button_10;
    [SerializeField] List<Button> setected_Buttons = new List<Button>();
    [SerializeField] List<Button> pressed_Buttons = new List<Button>();
    [SerializeField] List<Button> unclicked_Buttons = new List<Button>();
    [SerializeField] Button plusButton, minusButton;
    [SerializeField] TMP_Text[] btnAmtTxt;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI GameObjects")]
    [SerializeField] GameObject numPadButton;
    [SerializeField] GameObject takeCashObj;
    [SerializeField] GameObject menuBtn;
    [SerializeField] GameObject menuCancelBtn;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI GameObjects")]
    [SerializeField] GameObject BetArea_numPad;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("TakeCash TextMeshProUGUI")]
    [SerializeField] TextMeshProUGUI takeCashtxt;
    [SerializeField] TextMeshProUGUI takeCashWintxt;
    [SerializeField] TextMeshProUGUI takeCurrencytxt;
    [SerializeField] TextMeshPro ballonCashTxt; // TextMeshProUGUI
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI bet Buttons Images")]
    [SerializeField] Image plusButtomImg;
    [SerializeField] Image minusButtonImg;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Boolean")]
    public bool isAutoPlay;
    public bool numPad;
    public bool demo;
    public bool numBool;
    public bool bonusCount;
    public bool netCheck;
    [SerializeField] bool isCreateMatchSucceess = false;
    // private
    [SerializeField] public bool startGame;
    [SerializeField] public bool _balanceUpdate = false;
    [SerializeField] public bool onClick;
    [SerializeField] bool isChecked;
    [SerializeField] bool makeLose;
    [SerializeField] bool isPrediction = false;
    [SerializeField] bool btnPressed;
    [SerializeField] bool isPressed;
    [SerializeField] bool HeatBtnpress;
    [SerializeField] bool buttonPress;
    private bool isBegin;
    private bool pauseGame;
    private bool takeBetAmount;
    private bool isFire;
    private bool lost;
    private bool gameLost;
    public bool take;
    private bool isNormal;
    private bool stopper;
    private bool timerCount = true;
    private bool IsCreateMatchCalled = false;
    private bool predictionCheck = false;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("AutoPlay")]
    public List<int> setRounds = new List<int> { 9, 19, 49, 99, int.MaxValue };
    [SerializeField] public GameObject autoPlayPanel;
    [SerializeField] AutoplayInputHandler[] autoplayInputHandler;
    [SerializeField] bool autoPlayBtnPrss;
    [SerializeField] bool stopAutoPlay;
    [SerializeField] bool resetToggle, numClick;
    [SerializeField] int rounds;
    [SerializeField] float targetMultiplier, stopCashDecrease, stopSingleWin;
    [SerializeField] float check_stopCashDecrease, check_stopSingleWin;
    [SerializeField] double totalCash;
    [SerializeField] Toggle stop_CashDecrease, stop_SingleWin;
    [SerializeField] List<Button> roundSetBtns = new List<Button>();
    [SerializeField] List<Button> selectedroundBtns = new List<Button>();
    [SerializeField] Button addValBtns, subValBtns;
    [SerializeField] Button autoPlayBtn, stopAutoPlayBtn;
    [SerializeField] Button startBtn;
    [SerializeField] Image autoPlayicon;
    [SerializeField] GameObject autoCount;
    [SerializeField] Button resetBtn;
    [SerializeField] Image addValBtnsImg, subValBtnsImg, infiniteImg;
    [SerializeField] TMP_InputField targetInputField;
    [SerializeField] TMP_Text startBtn_Txt, restartBtn_Txt;
    [SerializeField] TMP_Text roundsTxt;
    [SerializeField] TMP_Text betUIstarticon_Txt, betUIPressToBet_Txt;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("TextMeshProUGUI")]
    [SerializeField] TextMeshPro multiplierTxt; // TextMeshProUGUI
    [SerializeField] TextMeshPro xTxt;
    [SerializeField] TextMeshProUGUI takeCashTxt;
    [SerializeField] TextMeshProUGUI totalAmountTxt;
    [SerializeField] TextMeshProUGUI takeCurrenyType;
    [SerializeField] TextMeshPro multiplierValue_Txt;
    // UI Bet Amount txt
    public TextMeshProUGUI betAmountTxt;
    public TextMeshProUGUI betFontTxt;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI background Cloud Image")]
    [SerializeField] RectTransform background;
    [SerializeField] float parallaxAmount = 2000f;
    [SerializeField] float smoothness = 0.05f; // Adjust this value to control smoothness
    private Vector3 initialBackgroundPosition;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Sprite background Parallex")]
    [SerializeField] Transform bgSprite;
    public float speed = 1f;
    private Vector3 initialBgPos;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("DoTween Win_Images")]
    [SerializeField] GameObject winPanel;
    [SerializeField] TextMeshProUGUI winTxt;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Insufficient Balance")]
    [SerializeField] GameObject insufficientBalance;
    public GameObject insufBal_Rumblebets;
    [SerializeField] GameObject cancelButton;
    [SerializeField] GameObject rumbleBet_cancelButton;
    [SerializeField] GameObject howToPlay;
    [SerializeField] Button demoCancelBtn;
    [SerializeField] Button rumbleCancelBtn;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("MaxBet_Reached")]
    [SerializeField] GameObject maxBet_Reached;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Audio Script")]
    [SerializeField] MasterAudioController audioController;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Panel")]
    [SerializeField]
    public GameObject RedirectionPanel;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("IDLE_TimerCount")]
    [SerializeField] float currentTime;
    [SerializeField] float startCount = 10f;
    [SerializeField] GameObject sessionTimeOut;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("GameLimits_TMP-Txt")]
    [SerializeField] private TMP_Text minBetTxt;
    [SerializeField] private TMP_Text maxBetTxt;
    [SerializeField] private TMP_Text maxWinOneBetTxt;
    [SerializeField] private TMP_Text betHistoryCurrency;
    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("API Controller")]
    private string currencyType;
    private string playerID;
    private string playerName;
    private string operatorName;
    private string gameName;
    private string lobbyName;
    private string betID;
    private int BetIndex;
    public CreateMatchResponse MatchRes;
    private bool IsInTab = true;
    public bool CanPlayAudio;
    [SerializeField] GameObject LoadingPopUp;
    [SerializeField] Image inputField;
    public bool needToCancelBet;
    public static GameController instance;
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Awake()
    {
        instance = this;
        LoadingPopUp.SetActive(true);
        audioController.muteAllAudio = true;
        AudioListener.volume = 0;
        CanPlayAudio = false;
        takeCashbutton.interactable = false;
        settingsPanelHandler.settingsBtn.onClick.AddListener(() => UI_Controller.instance.settingsHandler.CallingSettingPanel());
        settingsPanelHandler.settingsCloseBtn.onClick.AddListener(() => UI_Controller.instance.settingsHandler.HideMe());

        demoCancelBtn.onClick.AddListener(() => InsufficientCancelBtn());
        rumbleCancelBtn.onClick.AddListener(() => InsufficientCancelBtn());
        // AutoPlay
        setRounds = new List<int> { 9, 19, 49, 99, int.MaxValue };

    }
    private void Start()
    {
        // Subscribe to API events
        APIController apiController = APIController.instance;
        SessionEvents.OnUserDetailsUpdate += InitPlayerDetails;
        SessionEvents.OnUserDetailsUpdate += UpdateDefaultValues;
        SessionEvents.OnSwitchingTab += OnSwitchTab;
        BalloonColyseusManager.Instance.OnPlayerStateReceived += OnPlayerStateReceived;
        apiController.OnUserDeposit += InitUserDeposit;
        takeBetAmount = true;
        Difference = 0;
        btnPressed = false;
        /////
        multiplierValue_Txt.gameObject.SetActive(false);
        skeletonAnimation.gameObject.SetActive(false);
        /////
        takeCashObj.SetActive(false);
        // Initialize multiplier text
        string multiplierText = Multiplier.ToString("0.00");
        multiplierTxt.text = multiplierText;
        // Start coroutines
        StartCoroutine(HolidngButtons());
        // Settings Button
        settingsPanelHandler.settingsCloseBtn.gameObject.SetActive(false);
        settingsPanelHandler.settingsBtn.gameObject.SetActive(true);
        // Background position
        initialBackgroundPosition = background.localPosition;
        initialBgPos = bgSprite.position;
        // Start coroutines for IDLE_TimerCount
        currentTime = startCount;
        takeCashbutton.onClick.AddListener(() => TakeCashOut());

        for (int i = 0; i < setected_Buttons.Count; i++)
        {
            int index = i; // Capture the loop variable to avoid closure issues
            setected_Buttons[index].onClick.AddListener(() =>
                SelectBetButton((int)SessionState.entryAmountDetails.betValues[index], index));
        }

        for (int i = 0; i < pressed_Buttons.Count; i++)
        {
            int index = i; // Capture the loop variable to avoid closure issues
            pressed_Buttons[index].onClick.AddListener(() =>
                BetButtonPressed((int)SessionState.entryAmountDetails.betValues[index], index));
        }

        #region :::::::::::: { Auto Play } :::::::::::::::
        // For AutoPlay
        for (int i = 0; i < roundSetBtns.Count; i++)
        {
            int index = i;
            roundSetBtns[index].onClick.AddListener(() =>
            NumCountRounds(setRounds[index]));
        }

        HeatBtn.onClick.AddListener(() => HeatButton());

        autoPlayBtn.onClick.AddListener(() => AutoPlayBtnPress());
        stopAutoPlayBtn.onClick.AddListener(() => Stop_AutoPlayBtn());
        startBtn.onClick.AddListener(() => Start_AutoPlayBtn());
        resetBtn.onClick.AddListener(() => Reset_AutoPlayBtn());

        #endregion :::::::::::: { Auto Play } :::::::::::::::

        netCheck = false;
    }
    private void LateUpdate()
    {
        if (!isAutoPlay && HeatBtnpress)
        {

            if (keyBoard.gameObject.activeInHierarchy)
                keyBoard.OnCancelInput();

            if (!take && !lost && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf && !RedirectionPanel.activeSelf && !autoPlayPanel.activeSelf && !NetworkHandler.instance.ServerKick.activeSelf &&
                !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf &&
                !pauseGame && betAmount >= SessionState.entryAmountDetails.minBetValue && !settingsPanelHandler.GameLimits.activeSelf && !NetworkHandler.instance.SessionPopup.activeSelf)
            {
                Button_ONEnter();
                HeatButton();
            }
            else if (NetworkHandler.instance.ConnectionPanel.activeSelf && isPressed)
            {
                isPressed = false;
                Button_OFFEnter();
            }
            // Enable/Disable cancel buttons based on TotalAmount
            bool isAmountSufficient = TotalAmount >= SessionState.entryAmountDetails.minBetValue;

            // Only update if popups are already active
            if (insufficientBalance.activeSelf || insufBal_Rumblebets.activeSelf)
            {
                cancelButton.SetActive(isAmountSufficient);
                rumbleBet_cancelButton.SetActive(isAmountSufficient);
            }
        }

        if (isAutoPlay)
        {
            if (!take && !lost && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf && !RedirectionPanel.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf && !NetworkHandler.instance.ServerKick.activeSelf &&
                        !pauseGame && betAmount >= SessionState.entryAmountDetails.minBetValue && rounds >= 0 && !NetworkHandler.instance.SessionPopup.activeSelf)
            {
                Button_ONEnter();
                OnClickDown();
                Button_SwitchingOFF();
            }

            if (isAutoPlay && isPressed)
            {
                if (GetTruncatedValue_float(Multiplier) >= Difference)
                {
                    Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);
                    multiplierTxt.text = $"{Multiplier:F2}";
                    DebugHelper.Log($"CheckingForValue >>> : " + TakeCash);
                    isPressed = false;
                    timeSinceLastIncrement = 7f;
                    OnClickUp();
                    Button_OFFEnter();
                }
            }

            // Enable/Disable cancel buttons based on TotalAmount
            bool isAmountSufficient = TotalAmount >= SessionState.entryAmountDetails.minBetValue;

            // Only update if popups are already active
            if (insufficientBalance.activeSelf || insufBal_Rumblebets.activeSelf)
            {
                cancelButton.SetActive(isAmountSufficient);
                rumbleBet_cancelButton.SetActive(isAmountSufficient);
            }

            if ((rounds > -1 && rounds <= 0) && isPrediction)
            {
                Stop_AutoPlayBtn();
                rounds = -1;
            }

            if (insufficientBalance.activeSelf || insufBal_Rumblebets.activeSelf)
            {
                stopAutoPlay = false;
                Stop_AutoPlayBtn();
                TimeDelay();
            }
        }
    }
    #region { ::::::::::::::::::::::::: API ::::::::::::::::::::::::: }
    private void OnPlayerStateReceived(PlayerStateData _data)
    {
        InitAmountDetails();

        if (insufficientBalance.activeSelf || insufBal_Rumblebets.activeSelf)
        {
            InitUserDeposit();
        }
    }
    private void InitPlayerDetails()
    {
        DebugHelper.Log("UserDetails" + APIController.instance.userDetails.Id + " player ID " + playerID);
        playerID = APIController.instance.userDetails.Id;
        playerName = APIController.instance.userDetails.name;
        UI_Controller.instance.settingsHandler.playerNameTxt.text = playerName;
        operatorName = APIController.instance.userDetails.game_Id.Split("_")[0].ToString();
        gameName = APIController.instance.userDetails.game_Id.Split("_")[1].ToString();
        lobbyName = "Room : " + DateTime.UtcNow + UnityEngine.Random.Range(100, 999);
        if (APIController.instance.authentication.operatorname == "demo")
            demo = true;
        else
            demo = false;

        if (LoadingPopUp.activeSelf)
        {
            LoadingPopUp.SetActive(false);
            skeletonAnimation.gameObject.SetActive(true);
            CanPlayAudio = true;
            settingsPanelHandler.HideMe();
            audioController.BackgroundAudio.PlayAudioBGM();
        }

        for (int i = 0; i < 4; i++)
        {
            unSelectedBtnTxt[i].text = SessionState.entryAmountDetails.betValues[i].ToString();
            SelectedBtnTxt[i].text = SessionState.entryAmountDetails.betValues[i].ToString();
        }

        //////////////////////////////////////////////////////////////////////
        betAmount = SessionState.entryAmountDetails.minBetValue;
        betAmountTxt.text = $"{betAmount:F2} <size=30>{BalloonColyseusManager.Instance.Currency}</size>";
        ButtonSelect_Anim();
        ////////////////////////////////////////////////////////////////////// 

        //// Game Limits /////
        minBetTxt.text = $"{SessionState.entryAmountDetails.minBetValue.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{BalloonColyseusManager.Instance.Currency}</size>";
        maxBetTxt.text = $"{SessionState.entryAmountDetails.maxBetValue.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{BalloonColyseusManager.Instance.Currency}</size>";

        if (APIController.instance.authentication.operatorname == "demo")
            maxWinOneBetTxt.text = $"{10000.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{BalloonColyseusManager.Instance.Currency}</size>";
        else
            maxWinOneBetTxt.text = $"{1000000.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{BalloonColyseusManager.Instance.Currency}</size>";
        //// Game Limits /////

        betHistoryCurrency.text = "Bet " + BalloonColyseusManager.Instance.Currency;
        DebugHelper.Log("Player Details Subscribed");
        settingsPanelHandler.SetToggleValueFromAPI(APIController.instance.authentication.sound, APIController.instance.authentication.music);
        settingsPanelHandler.settingsCloseBtn.gameObject.SetActive(false);

        if (settingsPanelHandler.SoundToggle.isOn)
            audioController.muteAllAudio = false;
        else if (!settingsPanelHandler.SoundToggle.isOn)
            audioController.muteAllAudio = true;
    }
    public void InitAmountDetails()
    {
        TotalAmount = BalloonColyseusManager.Instance.Balance;
        string m = TotalAmount.ToString("0.00");
        TotalAmount = double.Parse(m);
        currencyType = BalloonColyseusManager.Instance.Currency;
        PassTxt(totalAmountTxt, currencyType);
        PassTxt(takeCurrenyType, currencyType);
        PassTxt(betAmountTxt, $"{betAmount:F2} <size={totalAmountTxt.fontSize - 10}>{currencyType}</size>");
        PassTxt(totalAmountTxt, $"{TotalAmount:F2} <size={totalAmountTxt.fontSize - 10}>{currencyType}</size>");
        DebugHelper.Log("Amount Details Subscribed");
    }
    private void UpdateDefaultValues()
    {
        TotalAmount = BalloonColyseusManager.Instance.Balance;
        betAmount = SessionState.entryAmountDetails.minBetValue;
        string m = TotalAmount.ToString("0.00");
        TotalAmount = double.Parse(m);
        currencyType = BalloonColyseusManager.Instance.Currency;
        PassTxt(totalAmountTxt, currencyType);
        PassTxt(takeCurrenyType, currencyType);
        PassTxt(betAmountTxt, $"{betAmount:F2} <size={totalAmountTxt.fontSize - 10}>{currencyType}</size>");
        PassTxt(totalAmountTxt, $"{TotalAmount:F2} <size={totalAmountTxt.fontSize - 10}>{currencyType}</size>");
        betAmountTxt.text = $"{betAmount:F2} <size=30>{currencyType}</size>";

        for (int i = 0; i < 4; i++)
        {
            unSelectedBtnTxt[i].text = SessionState.entryAmountDetails.betValues[i].ToString();
            SelectedBtnTxt[i].text = SessionState.entryAmountDetails.betValues[i].ToString();
        }
    }
    private void OnSwitchTab(bool isFocus)
    {
        DebugHelper.Log($"SwitchTab Status Check ********** {isFocus} || IsinFocus = {SessionState.isInFocus} || IsOnline {SessionState.isOnline}");
        if (CanPlayAudio)
        {
            AudioListener.volume = (isFocus && SessionState.isOnline && SessionState.isInFocus) ? 1 : 0;

            if (isFocus)
            {
                if (!netCheck && !NetworkHandler.instance.ConnectionPanel.activeSelf)
                {
                    if ((DateTime.Now - NetworkHandler.instance.GetLastActiveTime()).TotalSeconds >= 180)
                    {
                        DebugHelper.Log($"Session Check on Switch Tab Success");
                        if (!NetworkHandler.instance.SessionPopup.activeSelf)
                            NetworkHandler.instance.SessionPopup.SetActive(true);
                    }
                    else
                    {
                        DebugHelper.Log($"Session Check on Switch Tab Entered");
                        NetworkHandler.instance.SetDelay(180 - (DateTime.Now - NetworkHandler.instance.GetLastActiveTime()).TotalSeconds);
                        NetworkHandler.instance.StartIdleSession();
                    }
                }
            }
        }
        IsInTab = isFocus;
    }
    private void InitUserDeposit()
    {
        DebugHelper.Log("Deposit Called");
        if (insufBal_Rumblebets.activeSelf)
        {
            GameController.instance.Insufficient_OFF();
            insufBal_Rumblebets.SetActive(false);
        }
        if (insufficientBalance.activeSelf)
        {
            GameController.instance.Insufficient_OFF();
            insufficientBalance.SetActive(false);
        }
        if (!demo && TotalAmount >= SessionState.entryAmountDetails.minBetValue)
        {
            GameController.instance.Insufficient_OFF();
            if (insufBal_Rumblebets.activeSelf)
                insufBal_Rumblebets.SetActive(false);
            if (insufficientBalance.activeSelf)
                insufficientBalance.SetActive(false);
        }
    }
    private void PassTxt(TMP_Text _txt, string _passingValue)
    {
        _txt.text = _passingValue;
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    #region { ::::::::::::::::::::::::: Coroutine ::::::::::::::::::::::::: }
    IEnumerator HolidngButtons()
    {
        // Handle bet amount updates
        if (!startGame && !onClick)
            BetAmountUpdates();

        Check_AutoplayValue();
        StartBtnHadler();

        // Handle internet disconnection and button press
        if (NetworkHandler.instance.ConnectionPanel.activeSelf && isPressed)
        {
            isPressed = false;
            Button_OFFEnter();
        }
        // Disable minus button if bet amount is below minimum
        if (betAmount <= SessionState.entryAmountDetails.minBetValue)
        {
            minusButton.interactable = false;
            minusButtonImg.color = new Color32(255, 255, 255, 100);
        }
        if (startGame && !take)
        {
            Color color = betAmountTxt.color;
            color.a = 0.5f;
            betAmountTxt.color = color;
            betFontTxt.color = new Color32(255, 255, 255, 100);
            for (int i = 0; i < btnAmtTxt.Length; i++)
            {
                if (btnAmtTxt[i] != null)
                {
                    Color color1 = btnAmtTxt[i].color;
                    color1.a = 0.5f;
                    btnAmtTxt[i].color = color1;
                }
            }
        }
        // Handle take cash scenario
        if (takeBetAmount)
        {
            winPanel.SetActive(false);
            menuButton.interactable = true;
        }
        // Game Lose
        if (lost)
        {
            startGame = false;
            onClick = false;
            HeatBtnpress = false;
            skeletonAnimation.loop = false;
            audioController.StopAudio(AudioEnum.Movement);
        }
        // Taking cash
        if (take)
        {
            startGame = false;
            onClick = false;
            HeatBtnpress = false;
            audioController.StopAudio(AudioEnum.Movement);
            multiplierValue_Txt.gameObject.SetActive(false);
            skeletonAnimation.AnimationName = "Fly idle";
            skeletonAnimation.loop = true;
        }

        if (menuCancelBtn.activeSelf && !settingsPanelHandler.gameObject.activeSelf)
        {
            menuBtn.SetActive(true); menuCancelBtn.SetActive(false);
        }

        FireButton();
        if (isChecked)
            StartOfTheGame();

        if (startGame && !takeBetAmount)
        {
            if (GetTruncatedValue_float(Multiplier) >= holdHeight)
            {
                Multiplier = Mathf.Clamp(Multiplier, 0f, holdHeight);
                multiplierTxt.text = $"{Multiplier:F2}";
                gameLost = true;
                lost = true;
                skeletonAnimation.loop = false;
                skeletonAnimation.AnimationName = "Balloon blast";
                Balloon_Burt();
            }
        }
        if (startGame && isPressed && isFire && !lost)
        {
            // To move the Target pos Up at the Start
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            ParallaxEffect_();
            if (timeSinceLastIncrement >= incrementInterval)
            {
                IncrementMultiplier();
                timeSinceLastIncrement = 0f; // Reset the timer
            }
        }
        // Check if the timer has reached 7 seconds and no button is pressed
        else if (timeSinceLastIncrement >= 6f)
        {
            // Automatically take cash
            TakeCashOut();
            // Reset the timer
            timeSinceLastIncrement = 5.9f;
            isPressed = false;
        }
        if (bonusCount && APIController.instance.isOnline && !NetworkHandler.instance.ConnectionPanel.activeSelf)
        {
            TakeCashOut();
        }
        yield return null;
        StartCoroutine(nameof(HolidngButtons));
    }
    IEnumerator CoundownTimerforIdle()
    {
        if (!startGame)
        {
            if (!LoadingPopUp.activeSelf && !take && !lost && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf
                        && !autoPlayPanel.activeSelf && !settingsPanelHandler.GameLimits.activeSelf && !RedirectionPanel.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf)
            {
                currentTime -= 1 * Time.deltaTime;

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    sessionTimeOut.SetActive(true);
                }
            }
            else if (!LoadingPopUp.activeSelf && !take && !lost && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf
                        && !autoPlayPanel.activeSelf && !settingsPanelHandler.GameLimits.activeSelf && !RedirectionPanel.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf)
            {
                currentTime = 180;
                sessionTimeOut.SetActive(false);
            }

            if ((audioController != null && audioController.IsAnyAudioPlaying()))
            {
                currentTime = 180;
                sessionTimeOut.SetActive(false);
            }
        }
        else if (startGame)
        {
            currentTime = 180;
            sessionTimeOut.SetActive(false);
        }
        yield return null;
        StartCoroutine(nameof(CoundownTimerforIdle));
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    void Balloon_Burt()
    {
        ResetBets();
        audioController.PlayAudio(AudioEnum.ballonPopOut);
        takeCashbutton.interactable = false;
        timeSinceLastIncrement = 0f; // Reset the timer
        _balanceUpdate = true;
    }
    void FireButton()
    {
        if (!isFire)
        {
            audioController.StopAudio(AudioEnum.Movement);
            isPressed = false;
        }
    }
    private float GetTruncatedValue(double value)
    {
        return (float)(Math.Floor((decimal)(value) * 100) / 100);
    }
    private float GetTruncatedValue_float(float value)
    {
        return (float)(Math.Floor((decimal)(value) * 100) / 100);
    }
    private double GetTruncatedValue_WinAmout(float value)
    {
        return (double)(Math.Floor((decimal)(value) * 10000) / 10000);
    }
    void StartOfTheGame()
    {
        timeHold = GetTruncatedValue_float(Multiplier).ToString("F2");
        Mstring = GetTruncatedValue_float(Multiplier).ToString("F2");
        takeCashWintxt.text = GetTruncatedValue_float(TakeCash).ToString("F2");
        ballonCashTxt.text = GetTruncatedValue_float(TakeCash).ToString("F2") + "<size=3.5>" + currencyType + "</size>";

        if (startGame && Multiplier <= incrementRate)
        {
            stopper = true;
        }
        if (startGame && !lost)
        {
            if (isAutoPlay)
                Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);

            Mstring = GetTruncatedValue_float(Multiplier).ToString("F2");
            DebugHelper.Log($"Check_BetAmount : {betAmount} , {Mstring}");
            TakeCash = (betAmount * float.Parse(Mstring));
            takeCashWintxt.text = GetTruncatedValue_float(TakeCash).ToString("F2");
            ballonCashTxt.text = GetTruncatedValue_float(TakeCash).ToString("F2") + "<size=3.5>" + currencyType + "</size>";
            DebugHelper.Log($"Multipler1 TakeCash==>>> : {TakeCash}");
        }
        if (stopper)
        {
            stopper = false;
            Multiplier += Time.deltaTime;
            Multiplier = Mathf.Min(Multiplier);

            if (isAutoPlay)
                Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);

            string s = GetTruncatedValue_float(Multiplier).ToString("F2");
            multiplierTxt.text = s;
            balloon_Objs();
        }
        else if (isPressed && startGame && Multiplier >= 1.01f)
        {
            if (isAutoPlay)
                Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);

            string s = GetTruncatedValue_float(Multiplier).ToString("F2");
            multiplierTxt.text = s;

            DebugHelper.Log("CheckingTakeCash Value : " + GetTruncatedValue(TakeCash));
            takeCashTxt.text = GetTruncatedValue(TakeCash).ToString("F2");

            // Increment the timer by the time elapsed since the last frame
            timeSinceLastIncrement += Time.deltaTime;
        }
        if (startGame && takeBetAmount)
        {
            takeBetAmount = false;
        }
        if (startGame)
        {
            Button_Switch_OFF();
        }
        if (startGame && Multiplier >= 1.01f && !isAutoPlay)
        {
            TakeButtonColor();
        }
    }
    void TakeButtonColor()
    {
        if (startGame)
        {
            takeCashbutton.interactable = true;
            takeCashObj.SetActive(true);
        }
        else
        {
            takeCashbutton.interactable = false;
        }
    }
    void IncrementMultiplier()
    {
        if (isPressed && Multiplier >= 1.01f)
        {
            // Increase the multiplier by the increment rate
            Multiplier *= GetTruncatedValue_float(incrementRate);
            string s = GetTruncatedValue_float(Multiplier).ToString("F2");
            multiplierTxt.text = s;

            // Update the take cash text
            DebugHelper.Log("CheckingTakeCash Value_2 : " + GetTruncatedValue(TakeCash));
            takeCashTxt.text = GetTruncatedValue(TakeCash).ToString("F2");
        }
    }
    public void TakeCashOut() // TakeCash button
    {
        take = true;
        InternetCheck = true;
        TakingCash();
    }
    void API_IntitalizeBetAmount()
    {
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
        }

        APIController.instance.CheckInternetandProcess((success) =>
        {
            DebugHelper.Log("CheckInternetandProcess Betbtn");
            if (success)
            {
                DebugHelper.Log("CheckInternetandProcess Betbtn Success");
                LocalInitializeBet();
            }
            else
            {
                DebugHelper.Log("CheckInternetandProcess Failed");
                BetInputController.Instance.BetAmtInput.interactable = true;
            }
        });
    }
    void LocalInitializeBet()
    {
        DebugHelper.Log($"Entered LocalInitializeBet : {betAmount} , {TotalAmount}");
        if (betAmount > (float)TotalAmount)
        {
            if (TotalAmount >= .1f)
            {
                PassTxt(betAmountTxt, betAmount.ToString("0.00") + " " + currencyType);
            }
            else
            {
                DebugHelper.Log("nmnm");
            }

            DebugHelper.Log("LocalInitializeBet 2 " + betAmount);
            return;
        }
        isCreateMatchSucceess = false;

        #region
        int _index = UnityEngine.Random.Range(100, 999);
        string message = "Bet Initiated";
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = betAmount;
        val.Info = message;
        string m = betAmount.ToString("0.00");
        betAmount = float.Parse(m);
        DebugHelper.Log("LocalInitializeBet Controller.BetButtonclik ");
        DebugHelper.Log("BetAMount ********* " + betAmount + " " + " Balance ******* " + TotalAmount);
        List<string> _list = new List<string>();
        _list.Add(APIController.instance.userDetails.Id);

        GetPredictionProcess(async () =>
        {
            await UniTask.Delay(100);
        });

        if (isPrediction)
        {
            CreateMatchAPICall();
        }
        #endregion
    }
    private void CreateMatchAPICall()    //CREATEMATCHAPICALL CALLING METHOD
    {
        IsCreateMatchCalled = true;
        DebugHelper.Log("CreateMatchAPICalled========>");
        TransactionMetaData TransData = new();
        TransData.Amount = betAmount;
        TransData.Info = "InitBet";
        int _index = UnityEngine.Random.Range(100, 999);
        List<string> _list = new();
        lobbyName = "Room : " + DateTime.UtcNow + UnityEngine.Random.Range(100, 999);
        _list.Add(APIController.instance.userDetails.Id);
        APIController.instance.CreateAndJoinMatch(_index, betAmount, TransData, false, lobbyName, APIController.instance.userDetails.Id,
               false, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.isBlockApiConnection, _list,
               (initialized) =>
               {

               },
     (betIndex, response) =>
     {
         DebugHelper.Log("Is match getting ===>>>> + Success");
         BetIndex = betIndex;
         MatchRes = response;
         betID = response.Message;
         startGame = true;
         PressToBet();
         SetupGameForNewRound();
         HandleGameStart();
         HeatBtn.interactable = false;
         audioController.PlayAudio(AudioEnum.Movement, true);
     },
     (failed) =>
     {
         // Resetting all game Data......
         DebugHelper.Log("Is match getting ===>>>> + Failed");
         startGame = false;
         onClick = false;
         ResetBets();
     });
    }
    void API_Winning()
    {
        string message = "Game Won";
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = TakeCash;
        val.Info = message;
        DebugHelper.Log("isCreateMatchSucceess ====> success " + isCreateMatchSucceess);
        WinningBetAPICall(GetTruncatedValue_WinAmout(TakeCash), GetTruncatedValue_WinAmout(TakeCash));
    }
    private void WinningBetAPICall(double WinAmount, double PotAmount)   //WINNINGBETAPI CALLING METHOD
    {
        TransactionMetaData _metaData = new TransactionMetaData();
        _metaData.Amount = WinAmount;
        _metaData.Info = "Game Won";
        DebugHelper.Log("WinningsAmtBet ... : " + WinAmount);
        APIController.instance.WinningsBetMultiplayerAPI(BetIndex, betID, WinAmount, betAmount, PotAmount, _metaData, (success) =>
        {
            if (success)
            {
                IsCreateMatchCalled = false;
                take = true;
                startGame = false;
                onClick = false;
                // TakeCash
                takeCashbutton.interactable = false;

                if (isAutoPlay && (stopSingleWin > 0) && (TakeCash > stopSingleWin))
                {
                    Stop_AutoPlayBtn();
                }
                isNormal = true;
                audioController.PlayAudio(AudioEnum.winGame);
                winPanel.SetActive(true);
                menuButton.interactable = false;
                Winning_Animations();
                DelayFuction();
            }
            else
            {
                DebugHelper.Log("WinningBetAPIfailed========>");
            }
        }, APIController.instance.userDetails.Id, false, WinAmount == 0 ? false : true, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.commission, MatchRes.MatchToken);
    }
    async void DelayFuction()
    {
        if (isNormal)
        {
            DebugHelper.Log("Check1");
            await UniTask.Delay(3500);
            TimeDelay();
        }
    }
    void TimeDelay() // Clear UI
    {
        DebugHelper.Log("Check2");
        Multiplier = 0f;
        multiplierTxt.text = Multiplier.ToString("0.00");
        multiplierValue_Txt.gameObject.SetActive(false);
        TakeCash = 0f;
        Mstring = " ";
        takeCashTxt.text = GetTruncatedValue(TakeCash).ToString("F2");
        timeSinceLastIncrement = 0f;
        gameLost = false;
        _balanceUpdate = false;
        take = false;
        lost = false;
        isNormal = false;
        background.localPosition = initialBackgroundPosition;
        bgSprite.position = initialBgPos;
        isPrediction = false;
        if (!isAutoPlay)
        {
            Debug.Log("SeetheButtonDisable");
            autoPlayBtn.interactable = true;
            autoPlayicon.color = new Color32(255, 255, 255, 255);
            betUIstarticon_Txt.color = new Color32(255, 255, 255, 255);
            betUIPressToBet_Txt.color = new Color32(255, 255, 255, 255);
        }
        takeCashbutton.gameObject.SetActive(false);
        /////
        HeatBtn.gameObject.SetActive(true);
        btnPressed = false;
        skeletonAnimation.maskInteraction = SpriteMaskInteraction.None;
        skeletonAnimation.AnimationName = "Idle";
        skeletonAnimation.loop = true;
        BetArea_numPad.SetActive(true);
        /////
        if (!autoPlayBtn.interactable)
        {
            autoPlayBtn.interactable = true;
            autoPlayicon.color = new Color32(255, 255, 255, 255);
            betUIstarticon_Txt.color = new Color32(255, 255, 255, 255);
            betUIPressToBet_Txt.color = new Color32(255, 255, 255, 255);
        }
        if (!autoPlayBtnPrss)
        {
            HeatBtn.interactable = true;
            autoCount.SetActive(false);
        }
        Button_Switch_ON();

        // takeCash
        takeCashbutton.interactable = false;
        takeCashObj.SetActive(false);
        isBegin = false;
        winPanel.SetActive(false);
        menuButton.interactable = true;
        takeBetAmount = true;
        ButtonSelect_Anim();

        if (isAutoPlay && (totalCash > 0) && (TotalAmount <= totalCash))
            Stop_AutoPlayBtn();

        isChecked = false;
        NetworkHandler.instance.StartIdleSession();

        if (!isAutoPlay)
        {
            Color color = betAmountTxt.color;
            color.a = 1f;
            betAmountTxt.color = color;
            betFontTxt.color = new Color32(255, 255, 255, 200);

            for (int i = 0; i < btnAmtTxt.Length; i++)
            {
                if (btnAmtTxt[i] != null)
                {
                    Color color1 = btnAmtTxt[i].color;
                    color1.a = 1f;
                    btnAmtTxt[i].color = color1;
                }
            }
        }
        // AutoPlay
        if (isAutoPlay && (rounds >= 0 && rounds <= 100))
        {
            rounds--;
            roundsTxt.text = rounds.ToString();
        }
        if (!stopAutoPlay)
        {
            Stop_AutoPlayBtn();
            Reset_AutoPlayBtn();
        }
        netCheck = false;
    }
    public void HandleInsuffitient()
    {
        // takeCash
        takeCashObj.SetActive(false);
        isBegin = false;
        winPanel.SetActive(false);
        menuButton.interactable = true;
        takeBetAmount = true;
        ButtonSelect_Anim();
    }
    void ResetBets()
    {
        isPressed = false;
        pauseGame = false;
        startGame = false;
        onClick = false;
        winPanel.SetActive(false);
        menuButton.interactable = true;
        multiplierTxt.text = Multiplier.ToString("0.00");
        makeLose = false;
        isPrediction = false;
        IsCreateMatchCalled = false;
        Invoke(nameof(TimeDelay), 1.5f);
    }
    public void ClearAllBetsForFreshStart()
    {
        // Cancel any pending scheduled callbacks from the previous round
        CancelInvoke(nameof(TimeDelay));
        CancelInvoke(nameof(SetAnimDelay));

        // Core game state flags
        startGame = false;
        onClick = false;
        HeatBtnpress = false;
        isPressed = false;
        isFire = false;
        take = false;
        lost = false;
        gameLost = false;
        isNormal = false;
        isBegin = false;
        isPrediction = false;
        IsCreateMatchCalled = false;
        isCreateMatchSucceess = false;
        makeLose = false;
        buttonPress = false;
        btnPressed = false;
        _balanceUpdate = false;
        isChecked = false;
        netCheck = false;
        pauseGame = false;

        // Stop audio & reset spine animation
        audioController.StopAudio(AudioEnum.Movement);
        skeletonAnimation.maskInteraction = SpriteMaskInteraction.None;
        skeletonAnimation.AnimationName = "Idle";
        skeletonAnimation.loop = true;

        // Reset multiplier / cash-out values
        Multiplier = 0f;
        TakeCash = 0f;
        Mstring = "0.00";
        timeSinceLastIncrement = 0f;
        multiplierTxt.text = Multiplier.ToString("0.00");
        takeCashTxt.text = TakeCash.ToString("F2");
        takeCashWintxt.text = TakeCash.ToString("F2");
        ballonCashTxt.text = TakeCash.ToString("F2") + "<size=3.5>" + currencyType + "</size>";
        multiplierValue_Txt.gameObject.SetActive(false);

        // Reset background/parallax positions
        background.localPosition = initialBackgroundPosition;
        bgSprite.position = initialBgPos;

        // Reset core UI panels/buttons
        winPanel.SetActive(false);
        takeCashObj.SetActive(false);
        takeCashbutton.interactable = false;
        takeCashbutton.gameObject.SetActive(false);
        HeatBtn.gameObject.SetActive(true);
        HeatBtn.interactable = true;
        menuButton.interactable = true;
        BetArea_numPad.SetActive(true);
        takeBetAmount = true;

        // Reset AutoPlay fully
        if (isAutoPlay)
        {
            Stop_AutoPlayBtn();
        }
        isAutoPlay = false;
        autoPlayBtnPrss = false;
        stopAutoPlay = false;
        Reset_AutoPlayBtn();

        // Restore bet/plus/minus button interactability + selection visuals
        Button_Switch_ON();
        ButtonSelect_Anim();

        // Restore full opacity on bet text (in case it was dimmed mid-round)
        Color betColor = betAmountTxt.color;
        betColor.a = 1f;
        betAmountTxt.color = betColor;
        betFontTxt.color = new Color32(255, 255, 255, 200);
        for (int i = 0; i < btnAmtTxt.Length; i++)
        {
            if (btnAmtTxt[i] != null)
            {
                Color c = btnAmtTxt[i].color;
                c.a = 1f;
                btnAmtTxt[i].color = c;
            }
        }

        NetworkHandler.instance.StartIdleSession();
    }
    public void BetResetForInsufficient()
    {
        isPressed = false;
        pauseGame = false;
        startGame = false;
        onClick = false;
        buttonPress = false;
        HeatBtnpress = false;

        if (!startGame)
            ButtonSelect_Anim();
    }
    void Winning_Animations()
    {
        winTxt.text = $"{GetTruncatedValue_float(TakeCash):F2} {currencyType}";
        WinTxtObj();
    }
    public void TakingCash()
    {
        DebugHelper.Log($"TakeCashBetAmount 1 : _ {TakeCash}");
        API_Winning();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(2000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        if (currencyType == "USD")
        {
            winTxt.text = $"{GetTruncatedValue_float(TakeCash):F2} {currencyType}";
        }
        else if (currencyType == "EUR")
        {
            winTxt.text = $"{GetTruncatedValue_float(TakeCash):F2} {currencyType}";
        }
        else if (currencyType == "INR")
        {
            winTxt.text = $"{GetTruncatedValue_float(TakeCash):F2} {currencyType}";
        }
    }
    async void balloon_Objs()
    {
        await UniTask.Delay(400);
        if (isPressed)
        {
            if (btnPressed)
            {
                skeletonAnimation.AnimationName = "Fly loop";
                skeletonAnimation.loop = true;
            }
        }
    }
    async void Slider_Objs()
    {
        if (!isAutoPlay)
        {
            await UniTask.Delay(200);
            await UniTask.Delay(500);
        }
    }
    void Button_SwitchingOFF()
    {
        autoPlayBtn.interactable = false;
        minusButton.interactable = false;
        plusButton.interactable = false;

        // Disable all buttons
        button_1.interactable = false; button_2.interactable = false; button_5.interactable = false; button_10.interactable = false;

        for (int i = 0; i < pressed_Buttons.Count; i++)
        {
            pressed_Buttons[i].interactable = false;
            selectedroundBtns[i].interactable = false;
        }
        BetArea_numPad.SetActive(false);
        plusButtomImg.color = new Color32(255, 255, 255, 100);
        minusButtonImg.color = new Color32(255, 255, 255, 100);
    }

    #region ::::::::::::::::::::::::::: Event Trigger Buttons :::::::::::::::::::::::::::
    bool InternetCheck;
    public void OnClickDown()
    {
        netCheck = true;
        NetworkHandler.instance.StartIdleSession(false);

        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (!success)
            {
                InternetCheck = false;
                DebugHelper.Log("CheckInternetandProcess failed: " + success);
                return;
            }
            // Proceed if internet check is successful
            InternetCheck = true;
            onClick = true;

            // Handle button press logic if conditions are met
            if (!startGame && !pauseGame && ((float)TotalAmount < betAmount))
            {
                // Only show insufficient balance if it's not already showing
                if (!insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf)
                {
                    buttonPress = true;
                    CheckInsufficientBalance();
                }
                return;
            }
            if (!numPad && !buttonPress && !lost && !isAutoPlay)
            {
                Button_SwitchingOFF();
                HandleGameStart();
            }
            // Format amounts with two decimal places
            TotalAmount = double.Parse(TotalAmount.ToString("0.00"));
            betAmount = float.Parse(betAmount.ToString("0.00"));

            // Game setup if conditions are met
            if (!startGame && !buttonPress && !gameLost && !isBegin && !NetworkHandler.instance.ConnectionPanel.activeSelf)
            {
                if (!IsCreateMatchCalled)
                {
                    API_IntitalizeBetAmount();
                }
                else
                {
                    return;
                }
            }
            while (!startGame)
            {
                await UniTask.Delay(100);
            }
        });

        // Early return if no internet connection
        if (!InternetCheck)
        {
            return;
        }
    }
    private void HandleGameStart()
    {
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
        }
        // Start game animations and audio
        isPressed = true;

        ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
        ParallaxEffect_();

        if (btnPressed)
        {
            skeletonAnimation.AnimationName = "Fly loop";
            skeletonAnimation.loop = true;
        }
    }
    private void SetupGameForNewRound()
    {
        makeLose = true;
        isBegin = true;
    }
    public void RNG_APICall()   //RNG_APICALL CALLING METHOD
    {
        APIController.instance.GetRNG_API(betAmount, operatorName, APIController.instance.userDetails.gameId, (_IsWin, _MaxWin, _gameCount) =>
        {
            makeLose = _IsWin;
            holdHeight = _MaxWin;
            if (_IsWin)
            {
                DebugHelper.Log($"RNG Calculation:\n==============\n RNG Value Check : randomGamePlay \n MaxHeight : {_MaxWin}\n GameCount : {_gameCount}\n==============\n");
            }
            else
            {
                DebugHelper.Log($"RNG Calculation:\n==============\n RNG Value Check : LoseThisGame \n MaxHeight : {_MaxWin}\n GameCount : {_gameCount}\n==============\n");
            }

        }, gameName, 0);
    }

    #region GET PREDICTION FROM API
    public async void GetPredictionProcess(Action onSuccess)
    {

        if (isPrediction)
            return;

        DebugHelper.Log($"GetPredectedValueForWinREspone_#01 : {isPrediction}");
        isPrediction = true;
        bool predictionValue = false;

        Dictionary<string, string> bodyDict = new Dictionary<string, string>
        {
            //{ "GameName", "Balloon"}
            // Send Data to server
        };
        string payload = JsonConvert.SerializeObject(bodyDict);
        string reqID = "";
        /*reqID = APIController.instance.GetPredictionValue(payload, (init) =>
        {
            DebugHelper.Log("GetPredictionProcess Init Response Prediciton" + init.ToString());
        },
        (success) =>
        {
            predictionValue = true;

            DebugHelper.Log("GetPredictionProcess success Response Prediciton" + success.ToString());
            JObject data = JObject.Parse(success);
            string heightString = data["prediction"].ToString();
            float height = float.Parse(heightString);
            height = (float)Math.Round(height, 2);
            holdHeight = height;

            // For Debugging RNG data
            if (true) // Need to do
            {
                if (!demo)
                    Debug.Log($"RNG Calculation:\n==============\n Selected numbers from server are {height}\n==============\n");
                onSuccess?.Invoke();
            }
        },
        (failure) =>
        {
            DebugHelper.Log("Set_API_Index failure Response Prediciton" + failure.ToString());
        });*/
        float time = Time.time;
        bool waitingForresponseActive = false;
        while (!predictionValue)
        {
            if (!waitingForresponseActive && Time.time - time > APIController.instance.waitingForResponseDelay)
            {
                waitingForresponseActive = true;
                APIController.instance.OnInternetStatusChange?.Invoke(NetworkStatus.WaitingforResponse);
            }
            /*if (Time.time - time > APIController.instance.retryDelay)
            {
                if (BaseSocketController.instance.IsOnline())
                {
                    BaseSocketController.instance.RemoveRequestEvent(reqID);
                    DebugHelper.Log("WinningBetResponce  Retry Called");
                    GetPredictionProcess(onSuccess);
                    return;
                }
            }*/
            await UniTask.Delay(100);
        }
    }
    #endregion
    public void OnClickUp()
    {
        APIController.instance.CheckInternetandProcess((success) =>
        {
            // Check internet connection status
            if (!success)
            {
                InternetCheck = false;
                DebugHelper.Log("CheckInternetandProcess failed: " + success);
                return;
            }
            // Proceed if internet check is successful
            InternetCheck = true;

            // Reset button press state and update animations
            isPressed = false;
        });
        // Early exit if no internet connection
        if (!InternetCheck)
        {
            return;
        }
    }
    public void Button_ONEnter()
    {
        isFire = true;
    }
    public void Button_OFFEnter()
    {
        isFire = false;
    }
    public void Welcom_Button()
    {
        if (!startGame)
            ButtonSelect_Anim();
    }
    private void PressToBet()
    {
        skeletonAnimation.loop = false;
        skeletonAnimation.AnimationName = "Jump";
        Invoke(nameof(SetAnimDelay), 0.5f);
        Slider_Objs();
        SliderDelay();
    }
    void SetAnimDelay()
    {
        btnPressed = true;
        multiplierValue_Txt.gameObject.SetActive(true);
    }
    async void SliderDelay()
    {
        await UniTask.Delay(400);
        isChecked = true;
    }
    #endregion

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    /// <summary>
    public void HeatButton()
    {
        netCheck = true;
        NetworkHandler.instance.StartIdleSession(false);

        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (!success)
            {
                InternetCheck = false;
                DebugHelper.Log("CheckInternetandProcess failed: " + success);
                return;
            }
            // Proceed if internet check is successful
            InternetCheck = true;
            onClick = true;
            HeatBtnpress = true;
            takeCashbutton.gameObject.SetActive(true);
            HeatBtn.gameObject.SetActive(false);

            if (startGame)
            {
                autoPlayicon.color = new Color32(255, 255, 255, 40);
                autoPlayBtn.interactable = false;
            }
            // Handle button press logic if conditions are met
            if (!startGame && !pauseGame && ((float)TotalAmount < betAmount))
            {
                buttonPress = true;
                CheckInsufficientBalance();
                return;
            }
            if (!numPad && !buttonPress && !lost && !isAutoPlay)
            {
                Button_SwitchingOFF();
                HandleGameStart();
            }
            // Format amounts with two decimal places
            TotalAmount = double.Parse(TotalAmount.ToString("0.00"));
            betAmount = float.Parse(betAmount.ToString("0.00"));
            // Game setup if conditions are met
            if (!startGame && !buttonPress && !gameLost && !isBegin && !NetworkHandler.instance.ConnectionPanel.activeSelf)
            {
                if (!IsCreateMatchCalled)
                {
                    API_IntitalizeBetAmount();
                }
                else
                {
                    return;
                }
            }
            while (!startGame)
            {
                await UniTask.Delay(100);
            }
        });
        // Early return if no internet connection
        if (!InternetCheck)
        {
            return;
        }
    }
    public void SettingBetValuesInAPI()
    {
        Debug.Log("CheckingAPI>LOG : " + SessionState.entryAmountDetails.betValues);
    }
    public int currentClickValue;
    public void TakeButtonPress() // Function used in TakeButton Inspector in Editor
    {
        startGame = false;
    }
    public void BetButtonPressed(int betValue, int buttonIndex)
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        menuBtn.SetActive(true); menuCancelBtn.SetActive(false);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            BetInputController.Instance.BetPanel.gameObject.SetActive(false);
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);
            UpdateBetAmountDisplay();
            return;
        }
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !onClick && !isAutoPlay)
        {
            // Set the bet amount
            betAmount = betValue;
            betAmountTxt.text = $"{betAmount:F2} <size=30>{currencyType}</size>";
            BetAmountTxt_Scaling();
            // Set buttons' active state based on bet amount
            SetBetButtonsActiveState(betValue);
            UpdateButtonAnimations(betValue);
            plusButton.interactable = true;
            minusButton.interactable = true;
            Debug.Log("ButtonIntractable True _01" + plusButton);
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            if (maxBet_Reached.activeSelf)
                maxBet_Reached.SetActive(false);
        }
    }
    private void SetBetButtonsActiveState(int activeBet)
    {
        var betValues = SessionState.entryAmountDetails.betValues;
        button_1.gameObject.SetActive(activeBet == betValues[0]);
        button_2.gameObject.SetActive(activeBet == betValues[1]);
        button_5.gameObject.SetActive(activeBet == betValues[2]);
        button_10.gameObject.SetActive(activeBet == betValues[3]);
    }
    private void UpdateButtonAnimations(int betValue)
    {
        // Retrieve the index of the current bet value
        int betIndex = SessionState.entryAmountDetails.betValues.IndexOf(betValue);
    }
    // Select bet buttons
    public void SelectBetButton(int s, int btnIndex)
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        menuBtn.SetActive(true); menuCancelBtn.SetActive(false);
        for (int i = 0; i < setected_Buttons.Count; i++)
        {
            setected_Buttons[i].interactable = false;
            setected_Buttons[i].interactable = true;
        }
        if (!startGame && !take && !onClick && !isAutoPlay)
        {
            if (betAmount < SessionState.entryAmountDetails.maxBetValue)
            {
                betAmount += s;
                UpdateBetAmountDisplay();
                BetAmountTxt_Scaling();
                EnableButtons();
            }
            if (betAmount >= SessionState.entryAmountDetails.maxBetValue)
            {
                betAmount = SessionState.entryAmountDetails.maxBetValue;
                UpdateBetAmountDisplay();
                DisablePlusButton();
                MaxBet_Object();
            }
        }
    }
    private void UpdateBetAmountDisplay()  // SelectBetButton
    {
        betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
        betAmountTxt.gameObject.SetActive(true);
    }
    private void EnableButtons()  // SelectBetButton
    {
        plusButton.interactable = true;
        minusButton.interactable = true;
        Debug.Log("ButtonIntractable True _02" + plusButton);
        plusButtomImg.color = new Color32(255, 255, 255, 255);
        minusButtonImg.color = new Color32(255, 255, 255, 255);
    }
    private void DisablePlusButton()  // SelectBetButton
    {
        plusButton.interactable = false;
        plusButtomImg.color = new Color32(255, 255, 255, 120);
    }
    public async void MaxBet_Object()
    {
        maxBet_Reached.SetActive(true);
        await UniTask.Delay(1700);
        maxBet_Reached.SetActive(false);
    }
    public void PlusButton()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }
        if (keyBoard.gameObject.activeSelf)
            keyBoard.OnCancelInput();
        menuBtn.SetActive(true); menuCancelBtn.SetActive(false);
        if (!startGame && !take && !onClick && !isAutoPlay)
        {
            if (betAmount < SessionState.entryAmountDetails.maxBetValue)
            {
                betAmount += SessionState.entryAmountDetails.incrementValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                minusButton.interactable = true;
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= SessionState.entryAmountDetails.maxBetValue)
            {
                betAmount = SessionState.entryAmountDetails.maxBetValue;
                plusButton.interactable = false;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                plusButtomImg.color = new Color32(255, 255, 255, 100);
                MaxBet_Object();
            }
        }
    }
    public void MinusButton()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }
        if (keyBoard.gameObject.activeSelf)
            keyBoard.OnCancelInput();
        menuBtn.SetActive(true); menuCancelBtn.SetActive(false);
        if (!startGame && !take && !onClick)
        {
            if (betAmount > SessionState.entryAmountDetails.minBetValue)
            {
                betAmount -= SessionState.entryAmountDetails.decrementValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                plusButtomImg.color = new Color32(255, 255, 255, 255);
            }
            if (betAmount <= SessionState.entryAmountDetails.minBetValue)
            {
                betAmount = SessionState.entryAmountDetails.minBetValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                minusButton.interactable = false;
                minusButtonImg.color = new Color32(255, 255, 255, 100);
            }
        }
    }
    public void BetAmountTxt_Scaling()
    {
        // Create and configure a DoTween sequence for scaling animation
        DOTween.Sequence()
            .Append(betAmountTxt.transform.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.2f).SetEase(Ease.InSine))
            .AppendInterval(0.02f)
            .Append(betAmountTxt.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutSine));
    }
    void Button_Switch_ON()
    {
        // Enable all buttons
        button_1.interactable = true; button_2.interactable = true; button_5.interactable = true; button_10.interactable = true;

        for (int i = 0; i < unclicked_Buttons.Count; i++)
        {
            unclicked_Buttons[i].interactable = true;
        }
        // Enable plus and minus buttons with default color
        EnableButton_Switch_ON(plusButton, plusButtomImg);
        EnableButton_Switch_ON(minusButton, minusButtonImg);
    }
    void Button_Switch_OFF()
    {
        // Disable all buttons
        button_1.interactable = false; button_2.interactable = false; button_5.interactable = false; button_10.interactable = false;

        for (int i = 0; i < unclicked_Buttons.Count; i++)
        {
            unclicked_Buttons[i].interactable = false;
        }
        // Disable plus and minus buttons with faded color
        DisableButton_Switch_OFF(plusButton, plusButtomImg);
        DisableButton_Switch_OFF(minusButton, minusButtonImg);
    }
    private void EnableButton_Switch_ON(Button button, Image buttonImage)
    {
        button.interactable = true;
        buttonImage.color = new Color32(255, 255, 255, 255);
    }
    private void DisableButton_Switch_OFF(Button button, Image buttonImage)
    {
        button.interactable = false;
        buttonImage.color = new Color32(255, 255, 255, 120);
    }
    void BetAmountUpdates()
    {
        if (onClick)
        {
            numPadButton.gameObject.SetActive(false);
        }
        else
        {
            numPadButton.gameObject.SetActive(true);
        }

        if ((betAmount <= SessionState.entryAmountDetails.incrementValue) && !isAutoPlay)
        {
            minusButton.interactable = false;
            minusButtonImg.color = new Color32(255, 255, 255, 100);
        }
        else if ((betAmount > SessionState.entryAmountDetails.incrementValue) && !isAutoPlay)
        {
            /*minusButton.interactable = true;
            minusButtonImg.color = new Color32(255, 255, 255, 255);*/
        }

        if ((betAmount < SessionState.entryAmountDetails.maxBetValue) && !isAutoPlay && !take && !lost)
        {
            plusButton.interactable = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
        }
        else if ((betAmount >= SessionState.entryAmountDetails.maxBetValue) && !isAutoPlay)
        {
            plusButton.interactable = false;
            plusButtomImg.color = new Color32(255, 255, 255, 100);
        }
    }
    void ButtonSelect_Anim()
    {
        // Determine which button is active
        GameObject activeButton = null;
        if (button_1.gameObject.activeSelf) activeButton = button_1.gameObject;
        else if (button_2.gameObject.activeSelf) activeButton = button_2.gameObject;
        else if (button_5.gameObject.activeSelf) activeButton = button_5.gameObject;
        else if (button_10.gameObject.activeSelf) activeButton = button_10.gameObject;
    }
    public void Insufficient_OFF()
    {
        buttonPress = false;
        isBegin = false;
        BetResetForInsufficient();
    }
    void InsufficientCancelBtn()
    {
        UI_Controller.instance.PlayButtonSound();
        // Force reset all flags
        buttonPress = false;
        isBegin = false;
        onClick = false;
        HeatBtnpress = false;
        // Hide both popups
        insufficientBalance.SetActive(false);
        insufBal_Rumblebets.SetActive(false);
        // Reset game state
        BetResetForInsufficient();
    }
    private void CheckInsufficientBalance()
    {
        if (buttonPress)
        {
            DebugHelper.Log($"Entered LocalInitializeBet : {betAmount} , {TotalAmount} , {buttonPress}");
            HeatBtn.gameObject.SetActive(true);
            takeCashbutton.gameObject.SetActive(false);

            if (APIController.instance.authentication.operatorname == "demo")
            {
                insufficientBalance.SetActive(true);
                insufBal_Rumblebets.SetActive(false);
            }
            else
            {
                insufficientBalance.SetActive(false);
                insufBal_Rumblebets.SetActive(true);
                DebugHelper.Log("InsufficientPopUpAppers ==>>>_1 : " + buttonPress);
            }
        }
    }
    public void Close_sessionTimeOut() // Show PopUp for Close Button
    {
        currentTime = 180;
        sessionTimeOut.SetActive(false);
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    #region { ::::::::::::::::::::::::: Auto_Play ::::::::::::::::::::::::: }
    void NumCountRounds(int _rounds)
    {
        if (!startGame && !take && !onClick)
        {
            // Set the bet amount
            rounds = _rounds;
            if (rounds <= 99)
            {
                roundsTxt.text = rounds.ToString();
                if (infiniteImg.gameObject.activeSelf)
                    infiniteImg.gameObject.SetActive(false);
            }
            else if (rounds == int.MaxValue)
            {
                infiniteImg.gameObject.SetActive(true);
                roundsTxt.text = " ";
            }
            // Set buttons' active state based on bet amount
            SetRoundBtnActiveState(_rounds);
        }
    }
    private void SetRoundBtnActiveState(int activeBet)
    {
        var _setRounds = setRounds;

        for (int i = 0; i < selectedroundBtns.Count; i++)
        {
            selectedroundBtns[i].gameObject.SetActive(activeBet == _setRounds[i]);
        }
    }
    void AutoPlayBtnPress()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        Reset_AutoPlayBtn();
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }

        if (!startGame)
            autoPlayPanel.SetActive(true);
    }
    void Start_AutoPlayBtn()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        AutoPlayHandler();
        Difference = targetMultiplier;
        if (rounds >= 0)
        {
            isAutoPlay = true;
            autoPlayPanel.SetActive(false);
            autoPlayBtnPrss = true;
            autoPlayicon.gameObject.SetActive(false);
            autoCount.SetActive(true);
            stopAutoPlayBtn.gameObject.SetActive(true);
            if (startGame)
                autoPlayBtn.interactable = false;
            stopAutoPlay = true;

            if (stop_CashDecrease.isOn)
                totalCash = TotalAmount - stopCashDecrease;
        }
    }
    void Stop_AutoPlayBtn()
    {
        if (stopAutoPlayBtn.gameObject.activeSelf)
            audioController.PlayAudio(AudioEnum.buttonClick);
        autoPlayBtnPrss = false;
        stopAutoPlayBtn.gameObject.SetActive(false);
        autoPlayicon.gameObject.SetActive(true);
        autoPlayicon.color = new Color32(255, 255, 255, 40);
        betUIstarticon_Txt.color = new Color32(255, 255, 255, 40);
        betUIPressToBet_Txt.color = new Color32(255, 255, 255, 40);
        stopAutoPlay = false;
        autoCount.SetActive(false);
        totalCash = 0f;
        Invoke(nameof(SetOffAutoCount), 0.2f);
    }
    void SetOffAutoCount()
    {
        if (!isAutoPlay)
        {
            HeatBtn.interactable = true;
            totalCash = 0f;
            Color color = betAmountTxt.color;
            color.a = 1f;
            betAmountTxt.color = color;
            betFontTxt.color = new Color32(255, 255, 255, 200);

            for (int i = 0; i < btnAmtTxt.Length; i++)
            {
                if (btnAmtTxt[i] != null)
                {
                    Color color1 = btnAmtTxt[i].color;
                    color1.a = 1f;
                    btnAmtTxt[i].color = color1;
                }
            }
            Reset_AutoPlayBtn();
        }
    }
    void Reset_AutoPlayBtn()
    {
        if (autoPlayPanel.activeSelf)
            audioController.PlayAudio(AudioEnum.buttonClick);
        if (setRounds.Count >= 0)
            NumCountRounds(setRounds[0]);

        ResetBtnOFF();
        autoplayInputHandler[0].GetAudioBool(true);
        autoplayInputHandler[1].GetAudioBool(true);
        autoplayInputHandler[2].GetAudioBool(true);
        isAutoPlay = false;
        rounds = 9;
        roundsTxt.text = rounds.ToString();
        targetInputField.text = 2f.ToString("0.00");
        targetMultiplier = 2f;
        autoplayInputHandler[0].ResetToggles();
        autoPlayBtn.interactable = true;
        autoPlayicon.color = new Color32(255, 255, 255, 255);
        betUIstarticon_Txt.color = new Color32(255, 255, 255, 255);
        betUIPressToBet_Txt.color = new Color32(255, 255, 255, 255);
        infiniteImg.gameObject.SetActive(false);
        stop_CashDecrease.isOn = false;
        stop_SingleWin.isOn = false;
        autoplayInputHandler[0].GetAudioBool(false);
        autoplayInputHandler[1].GetAudioBool(false);
        autoplayInputHandler[2].GetAudioBool(false);
    }
    void AutoPlayHandler()
    {
        targetMultiplier = autoplayInputHandler[0].GetValue();
        stopCashDecrease = autoplayInputHandler[1].GetValue();
        stopSingleWin = autoplayInputHandler[2].GetValue();
    }
    void Check_AutoplayValue()
    {
        check_stopCashDecrease = autoplayInputHandler[1].GetValue();
        check_stopSingleWin = autoplayInputHandler[2].GetValue();
    }
    void StartBtnHadler()
    {
        if (autoPlayPanel.activeSelf)
        {
            if (stop_CashDecrease.isOn && stop_SingleWin.isOn)
            {
                startBtn.interactable = (check_stopCashDecrease > 0 && check_stopSingleWin > 0);
                GameController.instance.ResetBtnON();
                GameController.instance.resetToggle = true;
            }
            else if (!stop_CashDecrease.isOn && !stop_SingleWin.isOn)
            {
                startBtn.interactable = true;
                GameController.instance.ResetBtnOFF();
                GameController.instance.resetToggle = false;
            }
            else if (stop_CashDecrease.isOn && !stop_SingleWin.isOn)
                startBtn.interactable = (check_stopCashDecrease > 0);
            else
                startBtn.interactable = (check_stopSingleWin > 0);

            Color color = startBtn_Txt.color;
            color.a = startBtn.interactable ? 1f : 0.3f;
            startBtn_Txt.color = color;
            CheckMinCondition();
        }
    }
    public void AutoPlayCloseBtn()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        Reset_AutoPlayBtn();
        autoPlayPanel.SetActive(false);
    }
    void ResetBtnOFF()
    {
        resetBtn.interactable = false;
        Color color = restartBtn_Txt.color;
        color.a = 0.5f;
        restartBtn_Txt.color = color;
    }
    public void ResetBtnON()
    {
        resetBtn.interactable = true;
        Color color = restartBtn_Txt.color;
        color.a = 1f;
        restartBtn_Txt.color = color;
    }
    void CheckMinCondition()
    {
        if (rounds > 9 || stop_CashDecrease.isOn || stop_SingleWin.isOn || autoplayInputHandler[0].GetValue() != 2f)
        {
            ResetBtnON();
            numClick = true;
        }
        else if (rounds <= 9 && (!stop_CashDecrease.isOn && !stop_SingleWin.isOn))
        {
            ResetBtnOFF();
            numClick = false;
        }
    }
    #endregion { ::::::::::::::::::::::::: Auto_Play ::::::::::::::::::::::::: }

    #region { ::::::::::::::::::::::::: ParallaxEffect ::::::::::::::::::::::::: }
    void ApplyParallaxEffect(float holdButtonYPosition)     // Moving BackGround Main Image
    {
        // Calculate the target position for the background
        Vector3 targetBackgroundPosition = initialBackgroundPosition;
        targetBackgroundPosition.y -= holdButtonYPosition + parallaxAmount;
        // Smoothly interpolate towards the target position over time
        background.localPosition = Vector3.Lerp(background.localPosition, targetBackgroundPosition, Time.deltaTime * smoothness);
    }

    public void ParallaxEffect_()
    {
        bgSprite.Translate(Vector2.down * speed * Time.deltaTime);
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    #region { ::::::::::::::::::::::::: Testing_Amount ::::::::::::::::::::::::: }
    [SerializeField] float testAmount = 10f;
    [ContextMenu("ForceUpdateBalance")]
    private void ForceUpdateBalance()
    {
        APIController.instance.UpdateBalanceResponse(testAmount);
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
}

[System.Serializable]
public class BetData
{
    public string DateAndTime;
    public string BetAmount;
    public string WinAmount;
    public string MultiplierValue;
    public string currencyType;
    public bool IsWin;
}