using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NumericKeyboardThemed : MonoBehaviour
{
    [SerializeField] private NumericKeyboardRole role;

    public NumericKeyboardRole Role { get => role; set => role = value; }

    public void Apply(NumericKeyboardPalette palette)
    {
        Color normal = palette.Normal(role);

        Button button = GetComponent<Button>();
        if (button != null)
        {
            // key images stay white and the button tints them
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = normal;
            colors.selectedColor = normal;
            colors.pressedColor = palette.Pressed(role);
            colors.disabledColor = normal;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            return;
        }

        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null) graphic.color = normal;
    }
}
