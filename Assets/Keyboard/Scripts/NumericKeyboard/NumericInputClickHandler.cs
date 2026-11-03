using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NumericInputClickHandler : MonoBehaviour, IPointerClickHandler
{
    private NumericInputAutoBinder owner;
    private TMP_InputField input;

    public void Initialize(NumericInputAutoBinder binder, TMP_InputField field)
    {
        owner = binder;
        input = field;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (owner != null && input != null) owner.OpenInput(input);
    }
}
