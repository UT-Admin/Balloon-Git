using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static System.Net.Mime.MediaTypeNames;
//using DebugHelper = UnityEngine.DebugHelper;
using Input = UnityEngine.Input;
using System;
using IQPlay.GameClient;

public class BetInputController : MonoBehaviour
{
    public KeyBoardHandler _KeyBoardHandler;
    public TMP_InputField BetAmtInput;

    public Button Done;
    int clickCount = 0;
    public RectTransform BetPanel;

    public static BetInputController Instance;
    public bool IsEmptyInput = false;

    [SerializeField] GameController controller;
    private void Awake()
    {
        Instance = this;
        BetAmtInput.onValueChanged.AddListener(delegate { OnBetAmountEdit(); });
        BetAmtInput.onEndEdit.AddListener(delegate { OnEndEditBetAmount(BetAmtInput.text.Length < 15 ? SessionState.entryAmountDetails.minBetValue : (float.Parse(BetAmtInput.text))); });
    }
    void Start()
    {
        Done.onClick.AddListener(delegate { CloseKeyPadPanel(); });
        BetAmtInput.text = "1.00";

    }
    private void Update()
    {
        //if (BetPanel.gameObject.activeSelf)
        //{
        if ((Input.GetKeyDown(KeyCode.Return)) || (Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (_KeyBoardHandler.gameObject.activeSelf)
            {
                DebugHelper.Log("Enter key was pressed!");
                CloseKeyPadPanel();
                if (!GameController.instance.numBool)
                {
                    BetAmtInput.textViewport.gameObject.SetActive(false);
                }
            }

        }
        //}
    }
    public void OpenKeyPadPanel()
    {
        controller.numPad = true;
        DebugHelper.Log(" TOwer UserDevice ::" + SessionState.platform);
        if (GameController.instance.numBool)
        {
            return;
        }
        //BetPanel.gameObject.SetActive(true);
        if (KeyBoardHandler.instance != null)
        {
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(true);
            DisableBetInput();
        }
    }
    public void OnBetAmountEdit()
    {
        string pattern = @"^\d*(\.\d{0,2})?$";
        if (Regex.IsMatch(BetAmtInput.text, pattern))
        {
            BetAmtInput.text = BetAmtInput.text;
        }
        else
        {

            BetAmtInput.text = BetAmtInput.text.Substring(0, BetAmtInput.text.Length - 1);
        }
    }
    public void OnEndEditBetAmount(float amount)
    {
        DebugHelper.Log("OnEndEditBetAmount");
        _KeyBoardHandler.OnSubmitInput();

        /*if (GameController.instance.numBool)
        {
            if (!string.IsNullOrWhiteSpace(BetAmtInput.text))
            {
                if (BetAmtInput.text == ".")
                {
                    amount = APIController.instance.authentication.entryAmountDetails.minBetValue;
                    DebugHelper.Log($" CheckingTheNumPadAmt _1 : {amount}");

                }
                try
                {
                    amount = Mathf.Clamp((float)amount, APIController.instance.authentication.entryAmountDetails.minBetValue, APIController.instance.authentication.entryAmountDetails.maxBetValue);
                    DebugHelper.Log($" CheckingTheNumPadAmt _2 : {amount}");
                }
                catch
                {
                    DebugHelper.Log("Cant convert Too Lengthy OnEndEditBetAmount");
                }
            }
            else
            {
                if (GameController.instance.numBool)
                {
                    amount = APIController.instance.authentication.entryAmountDetails.minBetValue;
                    DebugHelper.Log($" CheckingTheNumPadAmt _3 : {amount}");
                }
                DebugHelper.Log("eMPTY iNPUT eNDeDIT");
                IsEmptyInput = true;

            }
        }
        if (GameController.instance.numBool)
        {
            controller.betAmount = amount;
            string _s = controller.betAmount.ToString("0.00");
            controller.betAmount = float.Parse(_s);
            DebugHelper.Log($" CheckingTheNumPadAmt _4 : {amount}");
            EnableBetInput();
            DebugHelper.Log($" CheckingTheNumPadAmt _5 : {amount}");
            BetAmtInput.interactable = false;
            BetAmtInput.interactable = true;
            BetAmtInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            BetAmtInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

            if (GameController.instance.numBool)
            {
                controller.betAmountTxt.gameObject.SetActive(true);

            }
        }*/

        StartCoroutine(ResetInputInteractableNextFrame());
    }
    private IEnumerator ResetInputInteractableNextFrame()
    {
        yield return null; // Wait one frame
        BetAmtInput.interactable = false;
        BetAmtInput.interactable = true;
    }

    public void OnEditInput()
    {
        if (!controller.onClick)
        {
            if (GameController.instance.numBool)
            {
                string val = controller.betAmount.ToString("F2");
                BetAmtInput.text = val;
                DisableBetInput();
            }
            else
            {
                DebugHelper.Log("OnEditInput ELse");

                controller.numPad = true;
                OpenKeyPadPanel();
                _KeyBoardHandler.gameObject.SetActive(true);
                _KeyBoardHandler.ShowKeyBoard(GetTruncatedValue2(controller.betAmount),
                 (float inputValue) =>
                 {
                     DebugHelper.Log("EDIT OVER : " + inputValue + "    " + controller.betAmount);
                     controller.betAmount = inputValue;
                     DebugHelper.Log("EDIT OVER 333 : " + inputValue + "    " + controller.betAmount);
                     controller.betAmountTxt.text = inputValue.ToString("F2") + " <size=30>" + SessionState.entryAmountDetails.currency_type + "</size>";

                     CloseKeyPadPanel();
                     controller.betAmountTxt.gameObject.SetActive(true);
                     DebugHelper.Log("Done " + controller.betAmount);
                     DebugHelper.Log($" CheckingTheNumPadAmt _5 : CloseKeyPadPanel");
                 },
                 (value) =>
                 {
                     controller.betAmountTxt.gameObject.SetActive(false);
                 },
                 (input) =>
                 {
                     DebugHelper.Log("Cancelled ");
                     DebugHelper.Log("EDIT OVER@#$=>> 2" + input + controller.betAmount);
                     //controller.betAmount = input;
                     //controller.betAmountTxt.text = GetTruncatedValue2(controller.betAmount).ToString("F2") + " <size=30>" + APIController.instance.userDetails.currency_type + "</size>";
                     controller.betAmount = input;
                     controller.betAmountTxt.text = input.ToString("F2") + " <size=30>" + SessionState.entryAmountDetails.currency_type + "</size>";
                     CloseKeyPadPanel();
                     controller.betAmountTxt.gameObject.SetActive(true);
                     DebugHelper.Log("Done " + controller.betAmount);
                     DebugHelper.Log($" CheckingTheNumPadAmt _6 : CloseKeyPadPanel");
                 });
                DebugHelper.Log(" Done  ");
            }
        }
    }

    private double GetTruncatedValue(float value)
    {
        return (double)(Math.Floor((decimal)(value) * 100) / 100);
    }

    private float GetTruncatedValue2(double value)
    {
        return (float)(Math.Floor((decimal)(value) * 100) / 100);
    }

    public void RestrictInput()
    {
        if (string.IsNullOrWhiteSpace(BetAmtInput.text))
        {
            DebugHelper.Log("Closing +");

            IsEmptyInput = true;
            controller.betAmount = SessionState.entryAmountDetails.minBetValue;
        }
        else
        {
            float amount = GetTruncatedValue2(controller.betAmount);
            if (BetAmtInput.text == ".")
            {
                amount = SessionState.entryAmountDetails.minBetValue;
            }
            else
            {
                if (BetAmtInput.text.Length > 15)
                {
                    amount = SessionState.entryAmountDetails.maxBetValue;
                }
                else
                {
                    amount = float.Parse(BetAmtInput.text);
                }
            }
            amount = Mathf.Clamp(amount, SessionState.entryAmountDetails.minBetValue, SessionState.entryAmountDetails.maxBetValue);
            IsEmptyInput = false;

            controller.betAmount = amount;
            /*            string _s = controller.betAmount.ToString("0.00");
                        controller.betAmount = float.Parse(_s);*/
        }
    }

    public void CloseKeyPadPanel()
    {
        DebugHelper.Log("EDIT OVER 444 : " + controller.betAmount);
        controller.numPad = false;
        DebugHelper.Log("CloseKeyPadPanel ");
        BetPanel.gameObject.SetActive(false);
        RestrictInput();
        DebugHelper.Log("EDIT OVER 555 : " + controller.betAmount);
        if (KeyBoardHandler.instance != null)
        {
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);

        }
        _KeyBoardHandler.gameObject.SetActive(false);
        EnableBetInput();
        DebugHelper.Log("CloseKeyPadPanel Done ");
        /*controller.HandGesture();*/

    }

    public void EnableBetInput()
    {
        DebugHelper.Log("EnableBetInput");

        BetAmtInput.interactable = true;
        if (!GameController.instance.numBool)
        {
            if (!GameController.instance.numBool)
            {
                controller.betAmountTxt.gameObject.SetActive(true);
            }
            else
            {
                controller.betAmountTxt.gameObject.SetActive(false);
            }
        }

        //string _s = controller.betAmount.ToString("0.00");
        //controller.betAmount = double.Parse(_s);
        controller.betAmountTxt.text = (controller.betAmount).ToString("F2") + " <size=30>" + SessionState.entryAmountDetails.currency_type + "</size>";

        DebugHelper.Log("EnableBetInput Called");

    }

    public void DisableBetInput()
    {
        DebugHelper.Log("DisableBetInput");
        controller.betAmountTxt.gameObject.SetActive(false);
        BetAmtInput.text = controller.betAmount.ToString("0.00");
        //string _s = controller.betAmount.ToString("0.00");
        //controller.betAmount = double.Parse(_s);

        BetAmtInput.textViewport.gameObject.SetActive(true);
        DebugHelper.Log("DisableBetInput");

    }
}
