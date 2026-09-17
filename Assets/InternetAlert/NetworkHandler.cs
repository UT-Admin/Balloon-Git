using System;
using System.Collections;
using UnityEngine;
using IQPlay.GameClient;

public class NetworkHandler : MonoBehaviour
{
    public GameObject ConnectionPanel;
    public GameObject ServerPopPanel;
    public static NetworkHandler instance;
    public GameObject waitingForResponse;
    public GameObject SessionPopup;
    public GameObject ServerKick;
    private DateTime _LastActiveTime;
    [SerializeField] private double _SessionDelay;

    private void Start()
    {
        instance = this;
        /*APIController.instance.OnInternetStatusChange += GetNetworkStatus;*/
        SessionEvents.OnInternetStatusChange += GetNetworkStatus;
        SetDelay(180);
        StartIdleSession();
    }
    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Mouse0) && !GameController.instance.netCheck) || (SettingsPanelHandler.instance.HTP.gameObject.activeInHierarchy && Input.mouseScrollDelta.y != 0) ||
            (!GameController.instance.autoPlayPanel.activeSelf && Input.mouseScrollDelta.y != 0))
        {
            StartIdleSession();
        }
    }

    DateTime lastActive = new DateTime();
    public void GetNetworkStatus(NetworkStatus data)
    {
        DebugHelper.Log($"NetworkStatus ==> {data.ToString()}");
        if (data != NetworkStatus.Active)
        {
            if (data == NetworkStatus.NetworkIssue)
            {
                ConnectionPanel.SetActive(true);
                waitingForResponse.gameObject.SetActive(false);
            }
            else if (!ConnectionPanel.activeSelf && data == NetworkStatus.ServerIssue)
            {
                CancelInvoke(nameof(CheckToEnable));
                Invoke(nameof(CheckToEnable), 3);
                waitingForResponse.gameObject.SetActive(false);
            }
            else if (data == NetworkStatus.WaitingforResponse)
            {
                DebugHelper.Log("THeGameIsin_WaitingForResponse");
                if (!waitingForResponse.activeSelf)
                    ShowWaitingForResponse();
            }
            StartIdleSession(false);
        }
        else
        {
            HideWaitingForResponse();
            DebugHelper.Log($"NetworkStatus ==> {data.ToString()}");
            CancelInvoke(nameof(CheckToEnable));
            ConnectionPanel.SetActive(false);
            ServerPopPanel.SetActive(false);
        }

        AudioListener.volume = (data == NetworkStatus.Active && !ConnectionPanel.gameObject.activeSelf && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;

        if (BalloonGameConfig.IsB2BMode)
        {
            BalloonColyseusManager.Instance.RequestBalanceRefresh();
            DebugHelper.Log($"Updated Blaance ---> 0 ");
        }
        else
        {
            GameController.instance.InitAmountDetails();
        }
    }
    private void CheckToEnable()
    {
        if (!ConnectionPanel.activeSelf)
        {
            ServerPopPanel.SetActive(true);
        }
    }
    public void ShowWaitingForResponse()
    {
        waitingForResponse.gameObject.SetActive(true);
        ConnectionPanel.SetActive(false);
    }
    public void HideWaitingForResponse()
    {
        waitingForResponse.gameObject.SetActive(false);
        ConnectionPanel.SetActive(false);
    }
    private IEnumerator ValidateIdle()
    {
        while ((DateTime.Now - _LastActiveTime).TotalSeconds <= _SessionDelay)
        {
            yield return new WaitForSeconds(1f);
        }
        if (!GameController.instance.startGame && !ConnectionPanel.activeSelf && !ServerPopPanel.activeInHierarchy)
            SessionPopup.SetActive(true);
        SetDelay();
    }

    public void OnReconnectButtonClicked()
    {
        SessionPopup.SetActive(false);
        StartIdleSession();

        if (GameController.instance != null)
            GameController.instance.ClearAllBetsForFreshStart();

        BalloonColyseusManager.Instance.SoftReconnect();
    }


    public void StartIdleSession(bool _check = true)
    {
        if (SessionPopup.activeSelf) { return; }
        SessionPopup.SetActive(false);
        StopCoroutine(nameof(ValidateIdle));
        if (_check)
        {
            _LastActiveTime = DateTime.Now;
            StartCoroutine(nameof(ValidateIdle));
        }
    }
    public DateTime GetLastActiveTime()
    {
        return _LastActiveTime;
    }
    public void SetDelay(double delay = 180)
    {
        _SessionDelay = delay;
    }
}
