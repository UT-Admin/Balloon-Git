using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Text.RegularExpressions;
using IQPlay.GameClient;

public class KeyBoardHandler : MonoBehaviour
{
    public Button[] NumKeys;
    public Button decimalButton, submitButton, backSpaceButton, cancelButton;
    public TMP_InputField displayText;
    public string currentInput = "";
    Action<float> OnSubmitAction, OnValurChangedAction;
    Action<float> OnCancelAction;
    public CanvasGroup canvasGroup;
    public static KeyBoardHandler instance;
    public RectTransform BetInputTextRect;
    [SerializeField] GameController controller;
    private void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Start()
    {
        for (int i = 0; i < NumKeys.Length; i++)
        {
            int j = i;
            NumKeys[i].onClick.AddListener(() => OnNumberPressed(j));
        }
        submitButton.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); OnSubmitInput(); });
        backSpaceButton.onClick.AddListener(() => OnBackSpacePressed());
        decimalButton.onClick.AddListener(() => OnDecimalValuePressed());
        cancelButton.onClick.AddListener(() => OnCancelInput());
    }
    public void OnNumberPressed(int number)
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
        BetInputController.Instance.BetAmtInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        BetInputController.Instance.BetAmtInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        int decimalIndex = currentInput.IndexOf('.');
        if (decimalIndex != -1)
        {
            string decimalPart = currentInput.Substring(decimalIndex + 1);
            if (decimalPart.Length == 2)
                return;
        }
        BetInputController.Instance.BetAmtInput.text = string.Empty;
        currentInput += number;
        UpdateDisplay(false);
    }
    void OnDecimalValuePressed()
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
        currentInput += ".";
        UpdateDisplay(false);
    }
    void UpdateDisplay(bool clampValue = true)
    {
        if (float.TryParse(currentInput, out float result))
        {
            if (clampValue)
            {
                result = Mathf.Clamp(result, SessionState.entryAmountDetails.minBetValue, SessionState.entryAmountDetails.maxBetValue);
                displayText.text = result.ToString();
            }
            else
            {
                displayText.text = currentInput;
                controller.betAmount = float.Parse(currentInput);
            }
        }
        else
        {
            displayText.text = currentInput;
            MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
        }
        currentInput = displayText.text;
        decimalButton.interactable = !currentInput.Contains(".");
        controller.betAmountTxt.gameObject.SetActive(false);
        if (!GameController.instance.numBool)
        {
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(true);
        }
        if (currentInput.Length > 10)
        {
            BetInputTextRect.localPosition = new Vector2(-((BetInputTextRect.sizeDelta.x / 2) + 20), 0);
        }
    }
    void OnBackSpacePressed()
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
        if (currentInput.Length > 0)
        {
            currentInput = displayText.text.Substring(0, displayText.text.Length - 1);
            UpdateDisplay(false);
        }
        if (!currentInput.Contains("."))
        {
            decimalButton.interactable = true;
        }
    }
    public void OnClearButtonPressed()
    {
        currentInput = "";
        displayText.text = "0";
    }
    public void OnSubmitInput()
    {
        DebugHelper.Log("OnSubmitInput");
        currentInput = displayText.text;
        if (string.IsNullOrWhiteSpace(currentInput))
        {
            DebugHelper.Log("KeyPad Input WIthout Value");
            controller.betAmount = SessionState.entryAmountDetails.minBetValue;
            OnSubmitAction?.Invoke(float.Parse(controller.betAmount.ToString("0.00")));
        }
        else
        {
            if (displayText.text == ".")
            {
                currentInput = SessionState.entryAmountDetails.minBetValue.ToString();
                controller.betAmount = SessionState.entryAmountDetails.minBetValue;
            }
            else
            {
                currentInput = displayText.text;
            }
            float f;
            try
            {
                f = Mathf.Clamp(float.Parse(currentInput), SessionState.entryAmountDetails.minBetValue, SessionState.entryAmountDetails.maxBetValue);
                OnSubmitAction?.Invoke(f);

                if (f >= SessionState.entryAmountDetails.maxBetValue)
                    GameController.instance.MaxBet_Object();
            }
            catch
            {
                DebugHelper.Log("Cant Convert Too lengthy");
                OnSubmitAction?.Invoke(SessionState.entryAmountDetails.maxBetValue);
                controller.betAmount = SessionState.entryAmountDetails.maxBetValue;
            }
            DebugHelper.Log("KeyPad Input WIth Some Values");
        }
        DebugHelper.Log("OnSubmitInput Called");
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
    }
    public void OnCancelInput()
    {
        DebugHelper.Log("OnCancelInput");
        currentInput = displayText.text;
        if (string.IsNullOrWhiteSpace(currentInput))
        {
            DebugHelper.Log("IsEmptyInput " + BetInputController.Instance.IsEmptyInput);
            BetInputController.Instance.IsEmptyInput = true;

            DebugHelper.Log("KeyPad Input WIthout Value");
            controller.betAmount = SessionState.entryAmountDetails.minBetValue;

            OnCancelAction?.Invoke(float.Parse(controller.betAmount.ToString("0.00")));
        }
        else
        {
            if (displayText.text == ".")
            {
                currentInput = SessionState.entryAmountDetails.minBetValue.ToString();
                controller.betAmount = SessionState.entryAmountDetails.minBetValue;
            }
            else
            {
                currentInput = displayText.text;
            }
            float f;
            try
            {
                f = Mathf.Clamp(float.Parse(currentInput), SessionState.entryAmountDetails.minBetValue, SessionState.entryAmountDetails.maxBetValue);
                OnSubmitAction?.Invoke(f);
            }
            catch
            {
                DebugHelper.Log("Cant Convert Too lengthy");
                OnSubmitAction?.Invoke(SessionState.entryAmountDetails.maxBetValue);
                controller.betAmount = SessionState.entryAmountDetails.maxBetValue;
            }

            DebugHelper.Log("KeyPad Input WIth Some Values");
        }
        DebugHelper.Log("OnCancelInput Called");
        BetInputController.Instance.CloseKeyPadPanel();
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
    }
    public void ShowKeyBoard(double currentValue, Action<float> _onSubmitAction, Action<float> _onValueChanged, Action<float> _onCancelAction)
    {
        BetInputController.Instance.BetAmtInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        BetInputController.Instance.BetAmtInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        currentInput = string.Empty;
        UpdateDisplay();
        OnSubmitAction = _onSubmitAction;
        OnCancelAction = _onCancelAction;
        OnValurChangedAction = _onValueChanged;
    }
}