using UnityEngine;

[DisallowMultipleComponent]
public class NumericInputSettings : MonoBehaviour
{
    [Header("Keyboard type")]
    public bool overrideKeyboardType;
    public NumericKeyboard.KeyboardType keyboardType = NumericKeyboard.KeyboardType.Decimal;

    [Header("Allowed range")]
    public bool overrideLimits;
    public double minValue = 0.1;
    public double maxValue = 100;

    [Header("Decimal places")]
    public bool overridePrecision;
    [Range(0, 8)] public int decimalPlaces = 2;

    private void OnValidate()
    {
        if (maxValue < minValue) maxValue = minValue;
    }
}
