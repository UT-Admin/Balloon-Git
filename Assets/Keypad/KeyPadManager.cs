using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class KeyPadManager : MonoBehaviour
{
    public float CurrentBetAmount;
    public TMP_Text CurrentBetInput;
    public TMP_InputField KeyPadInput;
    public float minBetValue;
    public float maxBetValue;
    private float _IncrementValue = 5;
    //public string convertBetString;
    private string pattern = @"^\d+(\.\d{1,2})?\s[A-Z]{3}$";

    private void Awake()
    {
        UpdateCurrentBetAmount(1f);
        KeyPadInput.onSelect.AddListener(delegate { OnEditInput(); });
        KeyPadInput.onValueChanged.AddListener(delegate { OnValueChangeInput(); });
        KeyPadInput.onEndEdit.AddListener(delegate { OnEndEditBetInputFld(); });
    }

    public void UpdateCurrentBetAmount(float betValue)
    {
        CurrentBetAmount = betValue;
        CurrentBetAmount = Mathf.Clamp(CurrentBetAmount, minBetValue, maxBetValue);
        CurrentBetInput.text = CurrentBetAmount.ToString("F2");
        DebugHelper.Log($"Current Bet Amount : {CurrentBetAmount}");
    }

    public void OnClickPlus()
    {
        UpdateCurrentBetAmount((CurrentBetAmount + _IncrementValue));
    }

    public void OnClickMinus()
    {
        UpdateCurrentBetAmount((CurrentBetAmount - _IncrementValue));
    }

    #region KEYPAD HANDLER

    public void OnEditInput()
    {
        //_InputHandler.gameObject.SetActive(true);
        KeyPadInput.text = string.Empty;
        KeyPadInput.interactable = true;
        CurrentBetInput.gameObject.SetActive(false);
        KeyPadInput.textViewport.gameObject.SetActive(true);
       /* _InputHandler.ShowKeyBoard(CurrentBetAmount,
            (inputValue) =>
            {
                CloseKeyPad(inputValue);
            },
            (value) =>
            {
                //CloseKeyPad(value);
            },
            (input) =>
            {
                CloseKeyPad(input);
            });*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || (Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (gameObject.activeSelf)
            {
                //_InputHandler.gameObject.SetActive(false);
                OnEndEditBetInputFld();

                KeyPadInput.textViewport.gameObject.SetActive(false);
                CurrentBetInput.gameObject.SetActive(true);
            }
        }
    }

    //private void CloseKeyPad(float inputValue)
    //{
    //    inputValue = Mathf.Clamp(inputValue, minBetValue, maxBetValue);
    //    _InputHandler.gameObject.SetActive(false);
    //    KeyPadInput.textViewport.gameObject.SetActive(false);
    //    CurrentBetInput.gameObject.SetActive(true);
    //    UpdateCurrentBetAmount(inputValue);
    //    KeyPadInput.interactable = true;
    //}

    public void OnEndEditBetInputFld()
    {
        if (KeyPadInput.interactable)
            KeyPadInput.interactable = false;
        KeyPadInput.interactable = true;

        try
        {
            float value = string.IsNullOrWhiteSpace(KeyPadInput.text) ? 0 : float.Parse(KeyPadInput.text);

            if (value > maxBetValue)
            {
                value = maxBetValue;
                UpdateCurrentBetAmount(value);

            }
            else if (value < minBetValue)
            {
                value = minBetValue;
                UpdateCurrentBetAmount(value);
            }
            else
            {
                UpdateCurrentBetAmount(value);
            }

            KeyPadInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            KeyPadInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            if (KeyPadInput.interactable)
                KeyPadInput.interactable = false;
            KeyPadInput.interactable = true;

        }
        catch (FormatException e)
        {
            DebugHelper.Log(e + " Error in conversion");
        }
    }

    public void OnValueChangeInput()
    {
        if (Regex.IsMatch(KeyPadInput.text, pattern))
        {
            KeyPadInput.text = KeyPadInput.text;
        }
        else
        {
            KeyPadInput.text = KeyPadInput.text.Substring(0, KeyPadInput.text.Length - 1);
        }

       CurrentBetAmount = string.IsNullOrWhiteSpace(KeyPadInput.text) ? 0 : float.Parse(KeyPadInput.text.ToString());
    }
    #endregion
}
