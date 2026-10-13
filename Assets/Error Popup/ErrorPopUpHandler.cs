using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IQPlay.GameClient;

public class ErrorPopUpHandler : MonoBehaviour
{
    public static ErrorPopUpHandler instance;
    public GameObject ErrorPanel;
    public TMP_Text errorText;
    CanvasGroup canvasGroup;

    public int _code;

    void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowError(int code, string error)
    {
        if (code == 504)
        {
            NetworkHandler.instance.ServerPopPanel.SetActive(true);
            return;
        }
        ErrorPanel.gameObject.SetActive(true);
        canvasGroup.DOFade(1, 0.2f).From(0);
        CancelInvoke(nameof(HideError));
        Invoke(nameof(HideError), 1.2f);
        errorText.text = "<sprite=0>" + error;
        _code = code;
    }

    void HideError()
    {
        canvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            ErrorPanel.gameObject.SetActive(false);
            switch (_code)
            {
                case 401:
                case 403:
                case 405:
                case 412:
                case 413:
                case 501:
                     UI_Controller.instance.ExitWebGL();
                    //SettingsPanelHandler.instance.RedirectingPanel.SetActive(true);

                    break;
                case 503:
                    // SettingsPanelHandler.Instance.ShowServerMaintanence();
                    NetworkHandler.instance.ServerPopPanel.SetActive(true);

                    break;
                default:
                    if (GameConfig.IsB2BMode)
                        BalloonColyseusManager.Instance.RequestBalanceRefresh();
                    // You can add a default action here if needed
                    break;
            }
        });
    }

}
