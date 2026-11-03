using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NumericKeyboard : MonoBehaviour
{
    public enum KeyboardType { Number, Decimal }
    public enum PlatformChoice { Auto, Android, IOS }
    public enum ThemeChoice { Auto, Dark, Light }

    [Serializable] public class StringEvent : UnityEvent<string> { }

    [Header("References")]
    [SerializeField] private NumericKeyboardPanel androidPanel;
    [SerializeField] private NumericKeyboardPanel iosPanel;
    [SerializeField] private GameObject outsideClickOverlay;
    [SerializeField] private NumericKeyboardLayoutAnimator layoutAnimator;

    [Header("Platform")]
    [Tooltip("Only take over input on phones and tablets. Desktop browsers keep their normal input fields.")]
    [SerializeField] private bool mobileOnly = true;
    [SerializeField] private PlatformChoice platformChoice = PlatformChoice.Auto;
    [SerializeField] private ThemeChoice themeChoice = ThemeChoice.Auto;
    [Tooltip("Browser back button closes the keyboard. Adds a history entry, which some host pages answer with a reload.")]
    [SerializeField] private bool closeOnBrowserBack = true;

    [Header("Rules")]
    [SerializeField] private double minValue = 0.1;
    [SerializeField] private double maxValue = 100;
    [SerializeField, Range(0, 8)] private int decimalPlaces = 2;
    [SerializeField, Range(1, 12)] private int maxWholeDigits = 9;
    [Tooltip("Off: Done hands over whatever was typed and your game deals with empty or too-big values.")]
    [SerializeField] private bool validateRange = false;

    [Header("Behaviour")]
    [SerializeField] private bool replaceValueOnFirstKey = true;
    [Tooltip("Fire the field's onEndEdit with the original text when the edit is cancelled.")]
    [SerializeField] private bool notifyEndEditOnCancel = true;
    [SerializeField] private bool applyValidValueOnDismiss = true;

    [Header("Events")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;
    public UnityEvent onClickedOutside;
    public StringEvent onValueChanged;
    public StringEvent onConfirmed;

    public static NumericKeyboard Instance { get; private set; }

    // close from anywhere, e.g. when a popup appears; silently skips the field's onEndEdit
    public static void CloseIfOpen(bool silently = false)
    {
        if (Instance == null || !Instance.isOpen) return;
        if (silently) Instance.CloseSilently();
        else Instance.CloseKeyboard();
    }

    public void CloseSilently()
    {
        bool saved = notifyEndEditOnCancel;
        notifyEndEditOnCancel = false;
        CloseKeyboard();
        notifyEndEditOnCancel = saved;
    }

    public bool IsOpen => isOpen;
    public bool IsAvailable => keyboardEnabled;
    public event Action<bool> AvailabilityChanged;
    public NumericKeyboardPlatform ActivePlatform =>
        activePanel != null ? activePanel.Platform : NumericKeyboardPlatform.Android;
    public bool IsDark => ResolveDark();

    private TMP_InputField targetInput;
    private KeyboardType activeType;
    private double activeMin, activeMax;
    private int activePrecision;

    private string typedText = "";
    private string textBeforeEditing = "";
    private bool replaceOnNextKey;
    private bool isOpen;
    private bool endEditFired;

    private NumericKeyboardPanel activePanel;
    private bool systemDark = true;
    private bool keyboardEnabled = true;

    private static readonly Regex NumberPattern = new Regex(@"\d+(?:\.\d*)?|\.\d+");

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ArmKeyboardBack(string objectName);
    [DllImport("__Internal")] private static extern void DisarmKeyboardBack();
    [DllImport("__Internal")] private static extern int NumericKeyboardDetectPlatform();
    [DllImport("__Internal")] private static extern int NumericKeyboardIsDarkMode();
    [DllImport("__Internal")] private static extern void NumericKeyboardWatchTheme(string objectName);
    [DllImport("__Internal")] private static extern void NumericKeyboardUnwatchTheme();
    [DllImport("__Internal")] private static extern void Updatekeyboardstatus(bool status);
#endif

    private void Awake()
    {
        Instance = this;
        keyboardEnabled = !mobileOnly || DetectMobile();
        ApplyInputMode();

        if (androidPanel != null) androidPanel.gameObject.SetActive(false);
        if (iosPanel != null) iosPanel.gameObject.SetActive(false);

        activePanel = ChoosePanel();
        if (activePanel == null)
            Debug.LogError("NumericKeyboard: no keyboard panel assigned. Run Tools > Numeric Keyboard > Build Keyboards.", this);
        else if (layoutAnimator != null)
            layoutAnimator.SetKeyboardPanel(activePanel.Rect);

        if (outsideClickOverlay != null)
        {
            Button overlayButton = outsideClickOverlay.GetComponent<Button>();
            if (overlayButton != null)
            {
                overlayButton.onClick.RemoveListener(OnOutsideClick);
                overlayButton.onClick.AddListener(OnOutsideClick);
            }
            else
            {
                Debug.LogWarning("NumericKeyboard: the outside-click overlay needs a Button component.", this);
            }
            outsideClickOverlay.SetActive(false);
        }

        QuerySystemTheme();
        ApplyAppearance();
        ShowError("");
    }

    private void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        NumericKeyboardWatchTheme(gameObject.name);
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnDisable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        NumericKeyboardUnwatchTheme();
#endif
        if (!isOpen) return;
        isOpen = false;
        RestoreTargetText();
#if UNITY_WEBGL && !UNITY_EDITOR
        if (closeOnBrowserBack) DisarmKeyboardBack();
#endif
    }

    // call with your own mobile/PC result; it wins over the automatic check
    public void SetKeyboardEnabled(bool enable)
    {
        if (keyboardEnabled == enable) return;
        if (!enable && isOpen) CloseKeyboard();
        keyboardEnabled = enable;
        ApplyInputMode();
        AvailabilityChanged?.Invoke(enable);
    }

    private void ApplyInputMode()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.mobileKeyboardSupport = !keyboardEnabled;
#endif
    }

    private static bool DetectMobile()
    {
#if UNITY_EDITOR
        return true;
#elif UNITY_WEBGL
        return NumericKeyboardDetectPlatform() != 0;
#else
        return Application.isMobilePlatform;
#endif
    }

    private NumericKeyboardPanel ChoosePanel()
    {
        NumericKeyboardPlatform wanted = ResolvePlatform();
        NumericKeyboardPanel panel = wanted == NumericKeyboardPlatform.IOS ? iosPanel : androidPanel;
        if (panel != null) return panel;
        return androidPanel != null ? androidPanel : iosPanel;
    }

    private NumericKeyboardPlatform ResolvePlatform()
    {
        if (platformChoice == PlatformChoice.Android) return NumericKeyboardPlatform.Android;
        if (platformChoice == PlatformChoice.IOS) return NumericKeyboardPlatform.IOS;
#if UNITY_WEBGL && !UNITY_EDITOR
        int detected = NumericKeyboardDetectPlatform();
        if (detected == 2) return NumericKeyboardPlatform.IOS;
        return NumericKeyboardPlatform.Android;
#else
        return Application.platform == RuntimePlatform.IPhonePlayer
            ? NumericKeyboardPlatform.IOS
            : NumericKeyboardPlatform.Android;
#endif
    }

    private bool ResolveDark()
    {
        if (themeChoice == ThemeChoice.Dark) return true;
        if (themeChoice == ThemeChoice.Light) return false;
        return systemDark;
    }

    private void QuerySystemTheme()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        systemDark = NumericKeyboardIsDarkMode() != 0;
#elif UNITY_EDITOR
        systemDark = UnityEditor.EditorGUIUtility.isProSkin;
#endif
    }

    private void ApplyAppearance()
    {
        if (activePanel != null) activePanel.ApplyTheme(ResolveDark());
    }

    public void SetTheme(ThemeChoice choice)
    {
        themeChoice = choice;
        ApplyAppearance();
    }

    public void OnSystemThemeChanged(string value)
    {
        systemDark = value != "light";
        if (themeChoice == ThemeChoice.Auto) ApplyAppearance();
    }

    // TMP focuses the field on tap, then fires onEndEdit the moment a keyboard key steals the selection.
    // So we drop focus ourselves a frame later, with onEndEdit muted.
    private IEnumerator ReleaseFieldFocus(TMP_InputField input)
    {
        yield return null;
        if (input == null) yield break;

        TMP_InputField.SubmitEvent realEndEdit = input.onEndEdit;
        input.onEndEdit = new TMP_InputField.SubmitEvent();
        input.DeactivateInputField();
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == input.gameObject)
            EventSystem.current.SetSelectedGameObject(null);
        input.onEndEdit = realEndEdit;
    }

    public void OpenForInput(TMP_InputField input, KeyboardType type,
        double minimum = double.NaN, double maximum = double.NaN, int precision = -1)
    {
        if (!keyboardEnabled || input == null || activePanel == null) return;
        StartCoroutine(ReleaseFieldFocus(input));
        if (isOpen && targetInput == input) return;
        #if UNITY_WEBGL && !UNITY_EDITOR
        Updatekeyboardstatus(true);
#endif
       
        bool wasOpen = isOpen;
        TMP_InputField previousField = null;
        if (wasOpen)
        {
            if (notifyEndEditOnCancel && !endEditFired) previousField = targetInput;
            RestoreTargetText();
        }
        endEditFired = false;

        targetInput = input;
        activeType = type;
        activeMin = double.IsNaN(minimum) ? minValue : minimum;
        activeMax = double.IsNaN(maximum) ? maxValue : maximum;
        if (activeMin > activeMax)
        {
            Debug.LogWarning("NumericKeyboard: min is bigger than max, swapping them.", this);
            double swap = activeMin; activeMin = activeMax; activeMax = swap;
        }

        activePrecision = type == KeyboardType.Number
            ? 0
            : (precision < 0 ? decimalPlaces : Mathf.Clamp(precision, 0, 8));

        textBeforeEditing = input.text ?? "";
        typedText = TrimToRules(ExtractFirstNumber(textBeforeEditing));
        replaceOnNextKey = replaceValueOnFirstKey && typedText.Length > 0;

        input.readOnly = true;
        input.DeactivateInputField();
        input.SetTextWithoutNotify(typedText);

        activePanel.SetDecimalAvailable(activeType == KeyboardType.Decimal && activePrecision > 0);
        activePanel.SetHint(validateRange ? $"Allowed: {Pretty(activeMin)} to {Pretty(activeMax)}" : "");
        activePanel.SetValue(typedText);
        ShowError("");
        if (outsideClickOverlay != null) outsideClickOverlay.SetActive(true);
        isOpen = true;

        if (layoutAnimator != null) layoutAnimator.OpenForInput(input.transform as RectTransform);
        else activePanel.gameObject.SetActive(true);

        if (!wasOpen)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (closeOnBrowserBack) ArmKeyboardBack(gameObject.name);
#endif
            onOpened?.Invoke();
        }
        onValueChanged?.Invoke(typedText);

        if (previousField != null && previousField != input) previousField.onEndEdit?.Invoke(previousField.text);
    }

    public void PressDigit(string digit)
    {
        if (!isOpen || string.IsNullOrEmpty(digit) || digit.Length != 1 || digit[0] < '0' || digit[0] > '9') return;

        string start = replaceOnNextKey ? "" : typedText;
        if (start == "0") start = "";   // 5 after a lone 0 gives "5", not "05"

        string candidate = start + digit;
        if (FitsRules(candidate)) AcceptTyped(candidate);
    }

    public void PressDecimal()
    {
        if (!isOpen || activeType != KeyboardType.Decimal || activePrecision == 0) return;

        string start = replaceOnNextKey ? "" : typedText;
        if (start.Contains(".")) return;

        string candidate = start.Length == 0 ? "0." : start + ".";
        if (FitsRules(candidate)) AcceptTyped(candidate);
    }

    public void Backspace()
    {
        if (!isOpen) return;
        replaceOnNextKey = false;
        if (typedText.Length == 0) return;
        AcceptTyped(typedText.Substring(0, typedText.Length - 1));
    }

    public void Clear()
    {
        if (isOpen) AcceptTyped("");
    }

    public void Done()
    {
        if (!isOpen) return;
        if (TryCommit(true)) CloseKeyboard();
    }

    public async void CloseKeyboard()
    {
        if (!isOpen) return;
        isOpen = false;

        TMP_InputField cancelledField = (notifyEndEditOnCancel && !endEditFired) ? targetInput : null;
        RestoreTargetText();
        if (outsideClickOverlay != null) outsideClickOverlay.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
        if (closeOnBrowserBack) DisarmKeyboardBack();
#endif
        if (layoutAnimator != null) layoutAnimator.CloseKeyboard();
        else if (activePanel != null) activePanel.gameObject.SetActive(false);

        onClosed?.Invoke();

        // fired last so a handler that reopens the keyboard doesn't get closed again
        if (cancelledField != null) cancelledField.onEndEdit?.Invoke(cancelledField.text);
        await UniTask.WaitForEndOfFrame();
#if UNITY_WEBGL && !UNITY_EDITOR
      //  Updatekeyboardstatus(false);
#endif
    }

    public void OnOutsideClick()
    {
        if (!isOpen) return;
        onClickedOutside?.Invoke();
        if (applyValidValueOnDismiss) TryCommit(false);
        CloseKeyboard();
    }

    public void OnBrowserBackRequested()
    {
        if (!isOpen) return;
        if (applyValidValueOnDismiss) TryCommit(false);
        CloseKeyboard();
    }

    private bool TryCommit(bool showErrors)
    {
        if (targetInput == null) return false;

        string number;
        if (!validateRange)
        {
            if (typedText.Length == 0)
            {
                number = "";
            }
            else
            {
                bool ok = double.TryParse(typedText, NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out double typed);
                number = ok ? Format(typed) : typedText;
            }
        }
        else
        {
            bool parsed = double.TryParse(typedText, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out double value);
            if (!parsed || double.IsNaN(value) || double.IsInfinity(value))
            {
                if (showErrors) ShowError("Enter a valid number.");
                return false;
            }

            value = Math.Round(value, activePrecision, MidpointRounding.AwayFromZero);

            if (value < activeMin || value > activeMax)
            {
                if (showErrors) ShowError($"Enter a value from {Pretty(activeMin)} to {Pretty(activeMax)}.");
                return false;
            }
            number = Format(value);
        }

        string finalText = ReplaceFirstVisibleNumber(textBeforeEditing, number);
        textBeforeEditing = finalText;

        targetInput.SetTextWithoutNotify(finalText);
        targetInput.onValueChanged?.Invoke(finalText);
        endEditFired = true;
        targetInput.onEndEdit?.Invoke(finalText);
        onConfirmed?.Invoke(number);

        // the game's onEndEdit may have changed the text (clamped it, say), so keep that
        if (targetInput != null) textBeforeEditing = targetInput.text ?? "";
        return true;
    }

    private string Format(double value)
    {
        value = Math.Round(value, activePrecision, MidpointRounding.AwayFromZero);
        return activePrecision == 0
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("F" + activePrecision, CultureInfo.InvariantCulture);
    }

    private void AcceptTyped(string newText)
    {
        replaceOnNextKey = false;
        typedText = newText;
        ShowError("");
        if (targetInput != null) targetInput.SetTextWithoutNotify(typedText);
        if (activePanel != null) activePanel.SetValue(typedText);
        onValueChanged?.Invoke(typedText);
    }

    private void RestoreTargetText()
    {
        if (targetInput != null) targetInput.SetTextWithoutNotify(textBeforeEditing);
    }

    private bool FitsRules(string candidate)
    {
        int dot = candidate.IndexOf('.');
        string whole = dot >= 0 ? candidate.Substring(0, dot) : candidate;
        string fraction = dot >= 0 ? candidate.Substring(dot + 1) : "";

        if (whole.Length > maxWholeDigits) return false;
        if (fraction.Length > activePrecision) return false;
        if (targetInput != null && targetInput.characterLimit > 0 && candidate.Length > targetInput.characterLimit) return false;
        return true;
    }

    private string TrimToRules(string value)
    {
        int dot = value.IndexOf('.');
        if (dot < 0) return value;
        if (activePrecision == 0) return value.Substring(0, dot);
        string fraction = value.Substring(dot + 1);
        if (fraction.Length > activePrecision) fraction = fraction.Substring(0, activePrecision);
        return value.Substring(0, dot) + "." + fraction;
    }

    private void ShowError(string message)
    {
        if (activePanel != null) activePanel.SetError(message);
    }

    private static string Pretty(double number) => number.ToString("0.########", CultureInfo.InvariantCulture);

    private static string ExtractFirstNumber(string richText)
    {
        string visible = BuildVisibleText(richText ?? "", out _);
        Match match = NumberPattern.Match(visible);
        if (!match.Success) return "";
        return match.Value.StartsWith(".") ? "0" + match.Value : match.Value;
    }

    private static string ReplaceFirstVisibleNumber(string richText, string replacement)
    {
        if (string.IsNullOrEmpty(richText)) return replacement;

        string visible = BuildVisibleText(richText, out List<int> rawIndex);
        Match match = NumberPattern.Match(visible);
        if (!match.Success) return richText + replacement;

        int start = rawIndex[match.Index];
        int end = rawIndex[match.Index + match.Length - 1] + 1;
        return richText.Substring(0, start) + replacement + richText.Substring(end);
    }

    // strips <tags> and remembers where each visible character sat in the original
    private static string BuildVisibleText(string richText, out List<int> rawIndex)
    {
        var visible = new StringBuilder(richText.Length);
        rawIndex = new List<int>(richText.Length);
        bool insideTag = false;

        for (int i = 0; i < richText.Length; i++)
        {
            char c = richText[i];
            if (!insideTag && c == '<' && richText.IndexOf('>', i) > i) insideTag = true;

            if (!insideTag)
            {
                visible.Append(c);
                rawIndex.Add(i);
            }
            else if (c == '>')
            {
                insideTag = false;
            }
        }
        return visible.ToString();
    }
}
