using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using IQPlay.GameClient;

public class AutoplayInputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public float minVal, maxVal;
    public Button plusButton, minusButton;
    public Toggle toggle;
    float incrementValue = 1f;
    public TMP_Text currencyValue;
    public TMP_Text InputField_txt;
    public bool isToggle;
    [SerializeField] bool isTargetMult;
    [SerializeField] bool isAudioStop;

    private float InputValue { get; set; }

    void OnEnable()
    {
        isAudioStop = true;
        SubscribeToEvents();

        // This line to restrict input to decimal numbers
        if (inputField != null)
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;

        if (!isToggle)
        {
            minVal = 0;
            maxVal = SessionState.entryAmountDetails.maxBetValue;
        }
        //plusButton.interactable = false;
        if (currencyValue != null)
        {
            currencyValue.text = SessionState.entryAmountDetails.currency_type.ToUpper();
        }
        ResetToggles();
        isAudioStop = false;
    }

    public void GetAudioBool(bool isON)
    {
        isAudioStop = isON;
    }

    public float GetValue()
    {
        return InputValue;
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (plusButton != null) plusButton.onClick.AddListener(PlusButtonClick);
        if (minusButton != null) minusButton.onClick.AddListener(MinusButtonClick);
        if (toggle != null) toggle.onValueChanged.AddListener(ToggleValueChanged);
        if (inputField != null) inputField.onValueChanged.AddListener(OnValueChanged);
        if (inputField != null) inputField.onEndEdit.AddListener(OnInputValueEndedit);
    }

    private void UnsubscribeFromEvents()
    {
        if (plusButton != null) plusButton.onClick.RemoveListener(PlusButtonClick);
        if (minusButton != null) minusButton.onClick.RemoveListener(MinusButtonClick);
        if (toggle != null) toggle.onValueChanged.RemoveListener(ToggleValueChanged);
        if (inputField != null) inputField.onValueChanged.RemoveListener(OnValueChanged);
        if (inputField != null) inputField.onEndEdit.RemoveListener(OnInputValueEndedit);
    }

    public void PlusButtonClick()
    {
        //AudioController.instance.PlayButtonSound();
        UI_Controller.instance.PlayButtonSound();
        InputValue += incrementValue;
        DebugHelper.Log(InputValue + " Plus button check1");
        InputValue = Mathf.Clamp(InputValue, minVal, maxVal);
        /*minusButton.interactable = toggle.isOn && InputValue > minVal;
        plusButton.interactable = toggle.isOn && InputValue < maxVal;*/

        GameController.instance.ResetBtnON();
        Plus_Minus_Func();
        UpdateTextField();
    }

    public void MinusButtonClick()
    {
        //AudioController.instance.PlayButtonSound();
        UI_Controller.instance.PlayButtonSound();
        InputValue -= incrementValue;
        DebugHelper.Log(InputValue + " Minus button check1");
        InputValue = Mathf.Clamp(InputValue, minVal, maxVal);
        /*minusButton.interactable = toggle.isOn && InputValue > minVal;
        plusButton.interactable = toggle.isOn && InputValue < maxVal;*/

        GameController.instance.ResetBtnON();
        Plus_Minus_Func();
        UpdateTextField();
    }

    public void ToggleValueChanged(bool value)
    {
        if (SettingsPanelHandler.instance.SoundToggle.isOn && !isAudioStop)
            MasterAudioController.instance.PlayAudio(AudioEnum.toggle);

        //AudioController.instance.PlayToggleSound();
        if (inputField != null) inputField.interactable = value;
        if (plusButton != null) plusButton.interactable = value;
        if (minusButton != null) minusButton.interactable = value && InputValue > minVal;
        plusButton.interactable = toggle.isOn && InputValue < maxVal;

        Color color = currencyValue.color;
        color.a = value ? 1f : 0.5f;
        currencyValue.color = color;

        Color color1 = InputField_txt.color;
        color1.a = value ? 1f : 0.5f;
        InputField_txt.color = color1;

        if (!value && InputValue > minVal)
        {
            ResetToggles();
        }
    }

    public void OnValueChanged(string value)
    {
        // if (string.IsNullOrEmpty(value))
        // {
        //     InputValue = 0;
        // }
        // else if (float.TryParse(value, out float result))
        // {
        //     InputValue = result;
        // }
        // UpdateTextField();
    }

    /*public void OnInputValueEndedit(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            InputValue = minVal;
        }
        else
        {
            InputValue = float.Parse(value);
        }
        DebugHelper.Log(InputValue + " OnInputValueEndedit");

        InputValue = Mathf.Clamp(InputValue, minVal, maxVal);
        DebugHelper.Log(InputValue + minVal + " OnInputValueEndedit1");

        inputField.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        inputField.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        Plus_Minus_Func();
        UpdateTextField();
    }*/
    public void OnInputValueEndedit(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            InputValue = minVal;
        }
        else
        {
            // Remove any non-numeric characters except decimal point and minus sign
            string cleanedValue = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

            if (string.IsNullOrEmpty(cleanedValue))
            {
                InputValue = minVal;
            }
            else if (float.TryParse(cleanedValue, System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                InputValue = result;
            }
            else
            {
                InputValue = minVal;
            }
        }

        DebugHelper.Log(InputValue + " OnInputValueEndedit");

        InputValue = Mathf.Clamp(InputValue, minVal, maxVal);
        DebugHelper.Log(InputValue + minVal + " OnInputValueEndedit1");

        inputField.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        inputField.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        Plus_Minus_Func();
        UpdateTextField();
    }

    void UpdateTextField()
    {
        if (isTargetMult)
        {
            if (inputField != null)
            {
                DebugHelper.Log(TruncateToTwoDecimalPlaces(InputValue) + " %$#@OnInputValueEndedit");
                inputField.text = " <size=30>" + "x" + "</size>" + TruncateToTwoDecimalPlaces(InputValue).ToString("F2");
            }
        }
        else
        {
            if (inputField != null)
            {
                DebugHelper.Log(TruncateToTwoDecimalPlaces(InputValue) + " %$#@OnInputValueEndedit");
                inputField.text = " <size=30>" + "</size>" + TruncateToTwoDecimalPlaces(InputValue).ToString("F2");
                //inputField.text = InputValue.ToString("F2");
            }
        }
    }

    public void ResetToggles()
    {
        if (!isToggle)
        {
            InputValue = minVal;
            ToggleValueChanged(false);
        }
        else
        {
            minVal = 1.01f;

            InputValue = 2f;
        }
        UpdateTextField();

        if (toggle != null && !isToggle) toggle.isOn = false;

        Plus_Minus_Func();
    }


    private float TruncateToTwoDecimalPlaces(float value)
    {

        return (float)Math.Floor((decimal)(value * 100)) / 100;
    }

    void Plus_Minus_Func()
    {
        if (!isToggle)
        {
            minusButton.interactable = toggle.isOn && InputValue > minVal;
            plusButton.interactable = toggle.isOn && InputValue < maxVal;
        }
        else
        {
            minusButton.interactable = toggle.isOn && InputValue >= 1.02f;
            plusButton.interactable = toggle.isOn && InputValue < maxVal;
        }
    }
}
