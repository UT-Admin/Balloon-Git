using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using IQPlay.GameClient;
public class ToggleHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image sourceImg;
    public Sprite[] images; // 0 = default, 1 = hover
    public bool isDangerBtn;
    public Button parentButton;
    private bool isHovered;
    private Coroutine hoverCheckCoroutine;
    private void Awake()
    {
        sourceImg = GetComponent<Image>();
    }
    private void OnEnable()
    {
        //   DebugHelper.Log("ToggleHighlighter OnEnable");
        sourceImg.sprite = images[0];
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SessionState.platform == "mobile" || SessionState.platform == "tablet")
            return;

        DebugHelper.Log("ToggleCheckDesktop_=>>>>_01 : ");
        isHovered = true;
        if (isDangerBtn)
        {
            if (parentButton.interactable)
            {
                sourceImg.sprite = images[1];
                StartCoroutine(WatchForDisable());
                DebugHelper.Log("ToggleCheckDesktop_=>>>>_02 : ");
            }
        }
        else
        {
            sourceImg.sprite = images[1];
            DebugHelper.Log("ToggleCheckDesktop_=>>>>_03 : " + images[1].name);
        }
        //if (hoverCheckCoroutine == null)
        //    hoverCheckCoroutine = StartCoroutine(CheckHoverExit());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (SessionState.platform == "mobile" || SessionState.platform == "tablet")
            return;
        isHovered = false;
        ResetSprite();
        if (hoverCheckCoroutine != null)
        {
            StopCoroutine(hoverCheckCoroutine);
            hoverCheckCoroutine = null;
        }
    }
    private IEnumerator WatchForDisable()
    {
        yield return new WaitUntil(() => !parentButton.interactable);
        if (isHovered)
        {
            sourceImg.sprite = images[0];
            isHovered = false;
            DebugHelper.Log("ToggleCheckDesktop_=>>>>_04 : " + images[0].name);
        }
    }
    private IEnumerator CheckHoverExit()
    {
        while (isHovered)
        {
            DebugHelper.Log($"Check hover exit result is => {RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition, null)}");
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                transform as RectTransform,
                Input.mousePosition,
                null))
            {
                OnPointerExit(null);
                yield break;
            }
            yield return null;
        }
    }
    private void ResetSprite()
    {
        if (isDangerBtn)
        {
            if (parentButton.interactable)
                sourceImg.sprite = images[0];
        }
        else
        {
            sourceImg.sprite = images[0];
        }
    }
}