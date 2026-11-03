using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumericInputAutoBinder : MonoBehaviour
{
    [SerializeField] private RectTransform gameUI;
    [SerializeField] private NumericKeyboard numericKeyboard;

    private readonly Dictionary<TMP_InputField, bool> registered = new Dictionary<TMP_InputField, bool>();

    private void Start()
    {
        if (gameUI == null || numericKeyboard == null)
        {
            Debug.LogError("NumericInputAutoBinder: assign GameUI and NumericKeyboard.", this);
            return;
        }

        numericKeyboard.AvailabilityChanged += OnAvailabilityChanged;
        if (numericKeyboard.IsAvailable)
        {
            int count = RegisterAllUnder(gameUI);
            Debug.Log($"NumericInputAutoBinder: hooked up {count} numeric input field(s).");
        }
    }

    private void OnDestroy()
    {
        if (numericKeyboard != null) numericKeyboard.AvailabilityChanged -= OnAvailabilityChanged;
    }

    private void OnAvailabilityChanged(bool available)
    {
        if (available) { if (gameUI != null) RegisterAllUnder(gameUI); }
        else UnregisterAll();
    }

    public int RegisterAllUnder(Transform root)
    {
        int count = 0;
        foreach (TMP_InputField field in root.GetComponentsInChildren<TMP_InputField>(true))
            if (Register(field)) count++;
        return count;
    }

    public bool Register(TMP_InputField field)
    {
        if (numericKeyboard == null || !numericKeyboard.IsAvailable) return false;
        if (field == null || !IsNumeric(field)) return false;

        if (!registered.ContainsKey(field)) registered[field] = field.readOnly;
        field.readOnly = true;   // typing goes through our keyboard, not the browser's

        NumericInputClickHandler handler = field.GetComponent<NumericInputClickHandler>();
        if (handler == null) handler = field.gameObject.AddComponent<NumericInputClickHandler>();
        handler.Initialize(this, field);
        return true;
    }

    public void UnregisterAll()
    {
        foreach (KeyValuePair<TMP_InputField, bool> entry in registered)
        {
            TMP_InputField field = entry.Key;
            if (field == null) continue;
            NumericInputClickHandler handler = field.GetComponent<NumericInputClickHandler>();
            if (handler != null) Destroy(handler);
            field.readOnly = entry.Value;
        }
        registered.Clear();
    }

    public void OpenInput(TMP_InputField input)
    {
        if (input == null || !input.interactable) return;

        NumericKeyboard.KeyboardType type =
            input.contentType == TMP_InputField.ContentType.IntegerNumber
                ? NumericKeyboard.KeyboardType.Number
                : NumericKeyboard.KeyboardType.Decimal;
        double min = double.NaN, max = double.NaN;
        int precision = -1;

        NumericInputSettings settings = input.GetComponent<NumericInputSettings>();
        if (settings != null)
        {
            if (settings.overrideKeyboardType) type = settings.keyboardType;
            if (settings.overrideLimits) { min = settings.minValue; max = settings.maxValue; }
            if (settings.overridePrecision) precision = settings.decimalPlaces;
        }

        numericKeyboard.OpenForInput(input, type, min, max, precision);
    }

    // NumericInputSettings counts as an opt-in even on a text field
    private static bool IsNumeric(TMP_InputField field)
    {
        return field.contentType == TMP_InputField.ContentType.IntegerNumber
            || field.contentType == TMP_InputField.ContentType.DecimalNumber
            || field.GetComponent<NumericInputSettings>() != null;
    }
}
