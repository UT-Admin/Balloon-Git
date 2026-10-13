using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IQPlay.GameClient;

public class ServerKickMessagePopUp : MonoBehaviour
{
    public RectTransform PopupBG;
    public TMP_Text MsgTxt;
    public int Code;
    public string Message = "";


    public void ShowPopup(string message, int code)
    {

        Message = message;
        MsgTxt.text = "<sprite=0>" + message;
        Code = code;
        gameObject.SetActive(true);
        GetComponent<Image>().DOFade(1, 0.5f).From(0);
        PopupBG.sizeDelta = new(140, PopupBG.sizeDelta.y);
        RectMask2D mask = PopupBG.GetComponentInChildren<RectMask2D>();
        float value = 70;
        DOTween.Sequence().Append(PopupBG.DOAnchorPosY(-300, 0.5f).From(new Vector2(0, 300)))
            .Append(PopupBG.DOSizeDelta(new Vector2(760, PopupBG.sizeDelta.y), 0.5f))
            .Join(DOTween.To(() => value, x => value = x, 0, 0.5f).OnUpdate(() => mask.padding = new(value, 0, value, 0)));
        CancelInvoke(nameof(HidePopup));


        Invoke(nameof(HidePopup), 3f);
        if (Code == 402)
        {
            MsgTxt.text = Message;
            BalloonColyseusManager.Instance.RequestBalanceRefresh();
        }
    }

    private void HidePopup()
    {
        RectMask2D mask = PopupBG.GetComponentInChildren<RectMask2D>();
        float value = 0;
        DOTween.Sequence().Append(PopupBG.DOSizeDelta(new Vector2(140, PopupBG.sizeDelta.y), 0.5f))
        .Join(DOTween.To(() => value, x => value = x, 70, 0.5f).OnUpdate(() => mask.padding = new(value, 0, value, 0)))
            .Append(PopupBG.DOAnchorPosY(300, 0.5f))
            .Join(GetComponent<Image>().DOFade(0, 0.5f)).OnComplete(() =>
            {

                if (Code == 401)
                {
                    MsgTxt.text = Message;
                    // GameController.Instance.ExitWebGL();
                }
                else
                {
                    DebugHelper.Log("TEST================>");
                    MsgTxt.text = Message;
                    //UIController.instance.ShowLoadingScreen();
                    // Invoke(nameof(RedirectToGame), 3);
                }
                gameObject.SetActive(false);
            });
    }
}
