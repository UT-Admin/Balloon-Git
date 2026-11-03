using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NumericKeyboardPanel : MonoBehaviour
{
    [SerializeField] private NumericKeyboardPlatform platform;

    [Header("Display")]
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_Text errorText;
    [Tooltip("Hidden while an error is showing, so the error can take its place.")]
    [SerializeField] private TMP_Text errorReplaces;
    [SerializeField] private Button decimalButton;
    [SerializeField] private string emptyPlaceholder = "0";

    private bool valueIsEmpty = true;

    public RectTransform Rect => (RectTransform)transform;
    public NumericKeyboardPlatform Platform { get => platform; set => platform = value; }

    public void ApplyTheme(bool dark)
    {
        NumericKeyboardPalette palette = NumericKeyboardPalette.Get(platform, dark);
        foreach (NumericKeyboardThemed themed in GetComponentsInChildren<NumericKeyboardThemed>(true))
            themed.Apply(palette);
        RefreshValueStyle();
    }

    public void SetValue(string value)
    {
        valueIsEmpty = string.IsNullOrEmpty(value);
        if (valueText != null) valueText.text = valueIsEmpty ? emptyPlaceholder : value;
        RefreshValueStyle();
    }

    public void SetHint(string hint)
    {
        if (hintText != null) hintText.text = hint ?? "";
    }

    public void SetError(string message)
    {
        bool has = !string.IsNullOrEmpty(message);
        if (errorText != null)
        {
            errorText.text = has ? message : "";
            errorText.gameObject.SetActive(has);
        }
        if (errorReplaces != null) errorReplaces.gameObject.SetActive(!has);
    }

    public void SetDecimalAvailable(bool available)
    {
        if (decimalButton == null) return;
        decimalButton.interactable = available;
        CanvasGroup group = decimalButton.GetComponent<CanvasGroup>();
        if (group != null) group.alpha = available ? 1f : 0.38f;
    }

    private void RefreshValueStyle()
    {
        if (valueText != null) valueText.alpha = valueIsEmpty ? 0.35f : 1f;
    }
}
