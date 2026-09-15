using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using IQPlay.GameClient;

public class APIController : MonoBehaviour
{

    public static APIController instance;

    [Header("Response from webGL json")]
    public string DummyData;
    [Header("Need Dummy Data to test live games in editor")]

    private int defaultDelay = 2;

    [Header("==============================================")]

    public Action OnAuthDataUpdate;
    public Action OnUserDetailsUpdate;
    public Action OnUserBalanceUpdate;
    public Action OnUserDeposit;

    public TitleData titleData;

    public Action<NetworkStatus> OnInternetStatusChange;

    public Action<bool> OnSwitchingTab;
    public bool isWin = false;
    public bool IsBotInGame = true;
    public GameWinningStatus winningStatus;
    public UserGameData userDetails;
    public List<BetRequest> betRequest = new List<BetRequest>();
    public bool isPlayByDummyData;
    public double maxWinAmount;
    public bool isClickDeopsit = false;

    public int defaultBootAmount = 25;

    public float waitingForResponseDelay = 4, retryDelay = 7;

    public List<APIRequestList> apiRequestList;
    public bool isInFocus = true;
    public bool isOnline = true;
    public bool MobileShow;
    public BackendAPI BackendAPIURL = new BackendAPI();

    public List<Betlist> betlistArray = new List<Betlist>();

#if UNITY_WEBGL
    #region WebGl Events
    [DllImport("__Internal")]
    public static extern void GetLoginData();
    [DllImport("__Internal")]
    public static extern void DisconnectGame(string message);
    [DllImport("__Internal")]
    public static extern void SetAudio(int sound, int music); // 1 or 0
    [DllImport("__Internal")]
    public static extern void FullScreen();
    [DllImport("__Internal")]
    private static extern void ShowDeposit();
    [DllImport("__Internal")]
    private static extern void UpdateBalance();

    [DllImport("__Internal")]
    public static extern void CloseWindow();

    private Action<BotDetails> GetABotAction;

    #endregion

    private void Awake()
    {
        AudioListener.volume = 0;
        instance = this;
    }

    void Start()
    {
        GetLambdaURL(true);
#if UNITY_WEBGL && !UNITY_EDITOR
        GetLoginData();
#elif UNITY_EDITOR
        // SetUserData("");
        StartAuthentication(DummyData);
#endif
    }
    #region WebGl Response


    public void UpdateBalanceTrigger()
    {
        DebugHelper.Log("UpdateBalanceTrigger ==>  ");
        OnUserDeposit?.Invoke();
        isClickDeopsit = false;
        if (!userDetails.isBlockApiConnection)
            GetUpdatedBalance();
    }

    public void UpdateBalanceResponse(double data)
    {
        DebugHelper.Log("Balance Updated response  :::::::----::: " + data);
        userDetails.balance = (double)data;
        authentication.balance = (double)data;
        OnUserBalanceUpdate?.Invoke();
#if !UNITY_EDITOR && UNITY_WEBGL
        UpdateBalance();
#endif
        if (isClickDeopsit)
        {
            OnUserDeposit?.Invoke();
        }
    }
    public void ApiCallBackDebugger(string data)
    {
        byte[] bytesToEncode = Convert.FromBase64String(data);
        string base64EncodedString = Encoding.UTF8.GetString(bytesToEncode);
        DebugHelper.Log(base64EncodedString + "inpuT");
        JObject OBJ = JObject.Parse(base64EncodedString);
        string url = OBJ["url"].ToString();
        int code = int.Parse(OBJ["status"].ToString());
        string body = OBJ["body"].ToString();
        string error = OBJ["error"].ToString();
        APICallBack(url, code, body, error);
    }
    public async void CheckAPICallBack(string url, int timeout)
    {
        DebugHelper.Log($"API_ Response :-  URL{url} -- timeout check");
        await UniTask.Delay(timeout * 1000);
        DebugHelper.Log($"API_ Response :-  URL{url} -- timeout..");

        foreach (var item in apiRequestList)
        {
            if (item.url == url)
            {
                item.callback(false, "timeout", "timeout");
            }
        }

        apiRequestList.RemoveAll(x => x.url == url);
    }

    public void OnSwitchingTabs(string data)
    {
        Time.timeScale = 1;
        isInFocus = data == "true" ? true : false;
        DebugHelper.Log($"Calleeedddd switching tab {data}   -   {isOnline}   -   {isInFocus}");
        OnSwitchingTab?.Invoke(data.ToLower() == "true");
    }
    #endregion

    public void OnClickDepositBtn()
    {
        isClickDeopsit = true;
        ShowDeposit();
    }
#endif
    /*public async void PlaceAIBet()
    {
        if (SelectNumberData.instance.Num == 1)
        {
            SelectNumberData.instance.PlacedAmountIs = APIController.instance.authentication.entryAmountDetails.minBetValue;
        }

        InGameHandler.Instance.SetSelectedBetIndex(0);
        InGameHandler.Instance.LetstartGame();
    }*/

    public async void GetUpdatedBalance()
    {
        bool playerinfoReceived = false;
        /*BaseSocketController.instance.WSS_PlayerInfo(
        (initateRes) =>
        {

        },
        (successAction) =>
        {
            OnInternetStatusChange?.Invoke(NetworkStatus.Active);
            playerinfoReceived = true;
            DebugHelper.Log("authentication response is : " + successAction);
            JObject apiResponse = JObject.Parse(successAction);
            DebugHelper.Log("authentication response is : " + (int)apiResponse["code"]);
            DebugHelper.Log(apiResponse.ToString());
            if ((int)apiResponse["code"] == 200)
            {
                JObject json = JObject.Parse(apiResponse["data"].ToString());
                DebugHelper.Log($"Player Info Json Response 1 => {json.ToString()}");
                authentication.balance = (float)json["balance"];
                userDetails.balance = authentication.balance;
                DebugHelper.Log("/8::----::: " + authentication.balance);
                OnUserBalanceUpdate?.Invoke();
#if !UNITY_EDITOR && UNITY_WEBGL
                UpdateBalance();
#endif
            }
            else
            {
                DisconnectGame((string)apiResponse["message"]);
            }
        }, (falseAction) =>
        {
            DebugHelper.Log("Player Info Failed " + falseAction);
        });*/

        float time = Time.time;
        bool waitingForresponseActive = false;
        while (!playerinfoReceived)
        {
            if (!waitingForresponseActive && Time.time - time > waitingForResponseDelay)
            {
                waitingForresponseActive = true;
                OnInternetStatusChange?.Invoke(NetworkStatus.WaitingforResponse);
            }
            if (Time.time - time > retryDelay)
            {
                /*if (BaseSocketController.instance.IsOnline())
                {
                    GetUpdatedBalance();
                    return;
                }*/
            }
            await UniTask.Delay(100);
        }
    }


    public AuthenticationData authentication = new AuthenticationData();


    public async void StartAuthentication(string data)
    {
        DebugHelper.Log("Response from wegbl for authentication : " + data);
        authentication = JsonUtility.FromJson<AuthenticationData>(data);
        //return;
        /*string orginalData = Cryptography.DecryptStr(authentication.title_data);
        Debug.Log("GetTitleData Response is orginal ::: " + orginalData);
        var finalresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(orginalData);
        if (finalresponse.ContainsKey("code") && finalresponse["code"].ToString() == "200" && finalresponse.ContainsKey("data"))
        {
            titleData = JsonConvert.DeserializeObject<TitleData>(finalresponse["data"]);

        }
        else
        {
            ErrorPopUpHandler.instance.ShowError(int.Parse(finalresponse["code"]), finalresponse["message"].ToString());
        }*/

        await UniTask.Delay(10);
        OnUserBalanceUpdate?.Invoke();

        {

            bool ignoreAuthdata = true;
            string _url = authentication.server_type.ToLower() == "aws" ? titleData.wss_url : titleData.server_url;
            /*BaseSocketController.instance.SetServerType(authentication.server_type.ToLower() == "aws" ? WebSocketProvider.AWS : WebSocketProvider.Colyseus);
            BaseSocketController.instance.ConnectWebSocket(_url, APIController.instance.authentication.gamename);*/


            try
            {
                JObject apiResponse = JObject.Parse(data);
                string json = apiResponse["game_data"].ToString();
                DebugHelper.Log(json);
                if (string.IsNullOrEmpty(json))
                {
                    ignoreAuthdata = false;
                }
                else
                {
                    authentication.entryAmountDetails.SetEntryAmount(json, authentication.currency_type);
                }
            }
            catch (Exception ex)
            {
                ignoreAuthdata = false;
            }

            OnAuthDataUpdate?.Invoke();



            /*BaseSocketController.instance.WSS_Authentication(

               (initaitedres) =>
               {
                   DebugHelper.Log("authentication response is : " + initaitedres + "ignore" + ignoreAuthdata);
               },

               (successRes) =>
               {
                   JObject apiResponse = JObject.Parse(successRes);
                   
                   DebugHelper.Log("Auth response  => " + apiResponse.ToString());
                   if ((int)apiResponse["code"] == 200)
                   {
                       JObject data = JObject.Parse(apiResponse["data"].ToString());
                       JObject output = JObject.Parse(apiResponse["output"].ToString());
                       authentication.session_token = (string)output["session_token"];
                       authentication.name = (string)data["username"];
                       authentication.balance = (double)data["balance"];
                       DebugHelper.Log("Auth response 1 => " + authentication.balance);
                       if (!ignoreAuthdata)
                       {
                           authentication.entryAmountDetails = new();
                           authentication.entryAmountDetails.SetEntryAmount(output["game_data"].ToString(), authentication.currency_type);
                       }
                       //authentication.music = (string)json1["music"].ToString();

                       DebugHelper.Log("authentication  is : " + JsonUtility.ToJson(authentication));
                       userDetails.Id = authentication.Id;
                       userDetails.game_Id = authentication.operatorname + "_" + authentication.gamename;
                       //userDetails.isBlockApiConnection = authentication.operatorname == "demo";
                       userDetails.name = authentication.name;
                       userDetails.session_token = authentication.session_token;
                       userDetails.token = authentication.token;
                       userDetails.platform = authentication.platform;
                       userDetails.operatorDomainUrl = authentication.operatorDomainUrl;
                       userDetails.currency_type = authentication.currency_type;
                       userDetails.gameId = (string)output["gameid"];
                       userDetails.hasBot = true;
                       userDetails.balance = authentication.balance;
                       userDetails.maxWin = 0;
                       userDetails.isWin = true;
                       userDetails.commission = 0;
                       userDetails.bootAmount = 25;
                       IsBotInGame = userDetails.hasBot;
                       userDetails.bootAmount = defaultBootAmount;
                       string musics = LocalStorage.Load("Lootrix_music");
                       string sounds = LocalStorage.Load("Lootrix_sound");
                       bool music = (string.IsNullOrEmpty(musics) || musics == "true") ? true : false;
                       bool sound = (string.IsNullOrEmpty(sounds) || sounds == "true") ? true : false;
                       authentication.sound = sound;
                       authentication.music = music;
                       if (string.IsNullOrWhiteSpace(userDetails.gameId))
                           userDetails.gameId = "ecd5c5ce-e0a1-4732-82a0-099ec7d180be";
                       DebugHelper.Log("Check this once !!!!!!!!!!!!!" + JsonUtility.ToJson(userDetails));
                       // MiniRouletteUIController.instance.SettingsPanel.UpdateToggle(authentication.sound, authentication.music);
                       if (authentication.operatorname == "demo")
                           BetHistoryHandler.Instance.loadMoreGameobject.SetActive(false);
                       AudioListener.volume = 1;

#if UNITY_EDITOR
                       if (MobileShow)
                       {
                           userDetails.platform = "mobile";
                       }
                       else
                       {
                           userDetails.platform = "desktop";
                       }
#endif
                   }
                   else
                   {
#if !UNITY_EDITOR
                       DisconnectGame("Illigal Access");
#else
                       DebugHelper.Log("Illigal Access");
#endif
                   }
                   DebugHelper.Log("On User Balance Updated");

                   OnUserBalanceUpdate?.Invoke();
                   OnUserDetailsUpdate?.Invoke();
               },


               (errorRes) =>
               {
                   DebugHelper.Log("Authentication Failed " + errorRes);
                   JObject apiResponse = JObject.Parse(errorRes);
                   ErrorPopUpHandler.instance.ShowError((int)apiResponse["code"], (string)apiResponse["message"]);
               }
           );*/
            // }
            // else
            // {
            //    // ErrorPopUpHandler.instance.ShowError

        }



        // if (data.Length < 30 || (authentication != null && authentication.operatorname == "demo"))
        // {
        //     SetUserData(data);
        //     return;
        // }

    }

    // public void GetRandomCard()
    // {
    //     Dictionary<string, string> bodyDict = new Dictionary<string, string>
    //     {
    //         { "env", "dev" },
    //         { "betAmount", UnityEngine.Random.Range(10,50).ToString() },
    //         { "CardData",JsonConvert.SerializeObject(GameController.Instance.CardListToServer) }
    //     };
    //     string payload = JsonConvert.SerializeObject(bodyDict);
    //     _wsClient.FetchGameResponse(gameID, "getballposition", payload);
    // }

    public async void CheckInternetForButtonClick(Action<bool> action)
    {
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_FORMDATA
                 ,
                 "https://6rugffwb323fkm7j7umild4vjm0hfcfm.lambda-url.ap-south-1.on.aws/",
                 new List<KeyValuePojo>(),
                 (bool isSuccess, string error, string body) =>
                 {
                     action.Invoke(isSuccess);
                 }, 2);
    }


    // public  async void GetTitleData(Action<bool> action)
    // {

    //     // List<KeyValuePojo> param = new List<KeyValuePojo>();
    //     // param.Add(new KeyValuePojo(){keyId = "request_type",value = "GetTitleData"});
    //     // param.Add(new KeyValuePojo() {keyId = "game_name",value = authentication.gamename});
    //      Dictionary<string, string> payload = new Dictionary<string, string>
    //     {
    //         { "request_type", "GetTitleData" },
    //         { "game_name", authentication.gamename }
    //     };

    //     string EncryptedPayload = Cryptography.EncryptStr(JsonConvert.SerializeObject(payload));

    //     WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_JSONDATA,
    //         $"https://fwiknm2h5fpjwc32oguaevkggi0tibgf.lambda-url.ap-south-1.on.aws/{authentication.environment}",
    //         new List<KeyValuePojo>() {new KeyValuePojo{keyId = "data",value = EncryptedPayload}},
    //         async (bool isSuccess, string error, string body) =>
    //         {  
    //             JObject obj = JObject.Parse(body);
    //             Debug.Log("TitleData Receivec" + obj["data"]);
    //             Debug.Log("TitleData Receivec" + Cryptography.DecryptStr(obj["data"].ToString()));
    //         //  action.Invoke(isSuccess);
    //         }, 2);
    // }

    public string CleanUrl(string url)
    {
        // Remove the protocol part (http:// or https://) and the trailing slash
        return url.Replace("https://", "").Replace("http://", "").TrimEnd('/');
    }
    public async void GetTitleData(Action<TitleData> action)
    {

        int count = 0;
        while (count < 3)
        {
            bool responseReceived = false;
            bool successResponse = false;
            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload["request_type"] = "GetTitleData";
            payload["game_name"] = authentication.gamename;
            string payloadJson = JsonConvert.SerializeObject(payload);
            /*string encryptPayload = Cryptography.EncryptStr(payloadJson);*/

            /*WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_JSONDATA, authentication.client_url, new List<KeyValuePojo>() { new KeyValuePojo { value = encryptPayload, keyId = "data" } }, async (success, error, body) =>
            {
                responseReceived = true;
                Debug.Log("GetTitleData Response is ::: " + body);
                if (success)
                {
                    var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                    string orginalData = Cryptography.DecryptStr(response["data"]);
                    Debug.Log("GetTitleData Response is orginal ::: " + orginalData);
                    var finalresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(orginalData);

                    if (finalresponse.ContainsKey("code") && finalresponse["code"].ToString() == "200" && finalresponse.ContainsKey("data"))
                    {
                        successResponse = true;
                        action.Invoke(JsonConvert.DeserializeObject<TitleData>(finalresponse["data"]));
                    }
                    else
                    {
                        ErrorPopUpHandler.instance.ShowError(int.Parse(finalresponse["code"]), finalresponse["message"].ToString());
                    }
                }

            }, 3);*/

            while (responseReceived == false)
            {
                await UniTask.Delay(50);
            }
            if (successResponse)
            {
                break;
            }
            else
            {
                count++;
            }
        }

        if (count >= 3)
        {
            ErrorPopUpHandler.instance.ShowError(420, "Something went wrong!");
        }

    }

    public async void GetServerData(Action<TitleData> action)
    {

        int retrycount = 0;
        string domainUrl = "";
        while (domainUrl == "" && retrycount < 6)
        {
            retrycount++;
            bool isRun = true;
            Dictionary<string, string> payload = new Dictionary<string, string>();
            payload["request_type"] = "GetServer";
            payload["game_name"] = authentication.gamename;
            string payloadJson = JsonConvert.SerializeObject(payload);
            /*string encryptPayload = Cryptography.EncryptStr(payloadJson);

            WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_JSONDATA, "https://fwiknm2h5fpjwc32oguaevkggi0tibgf.lambda-url.ap-south-1.on.aws/" + authentication.environment, new List<KeyValuePojo>() { new KeyValuePojo { value = encryptPayload, keyId = "data" } }, async (success, error, body) =>
            {
                Debug.Log("GetServerDetails Response is ::: " + body);
                if (success)
                {
                    var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                    string orginalData = Cryptography.EncryptStr(response["data"]);
                    Debug.Log("GetServerDetails Response is orginal ::: " + orginalData);
                    var finalresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(orginalData);

                    if (finalresponse.ContainsKey("code") && finalresponse["code"].ToString() == "200" && finalresponse.ContainsKey("data"))
                    {
                        action.Invoke(JsonConvert.DeserializeObject<TitleData>(finalresponse["data"]));
                    }
                    else
                    {
                        if (retrycount > 5 && finalresponse.ContainsKey("message"))
                        {
#if UNITY_WEBGL && !UNITY_EDITOR
                                DisconnectGame(finalresponse["message"]);
#endif
                        }
                        else
                        {
#if UNITY_WEBGL && !UNITY_EDITOR
                                DisconnectGame("Internal Server Error");
#endif
                        }
                        await UniTask.Delay(1000);
                    }
                }
                else
                {
                    if (retrycount > 5)
                    {
#if UNITY_WEBGL && !UNITY_EDITOR
                                DisconnectGame("Internal Server Error");
#endif
                        return;
                    }
                    await UniTask.Delay(1000);
                }
                isRun = false;

            }, 3);*/
            while (isRun)
            {
                await UniTask.Delay(100);
            }
        }
    }

    public async void CheckInternetandProcess(Action<bool> action, bool isLooping = false)
    {
        action.Invoke(isOnline);
        return;
    }

    #region API
    public int InitlizeBet(float amount, TransactionMetaData metadata, bool isAbleToCancel = false, Action<bool> action = null, string playerId = "", bool isBot = false, Action<string> betIdAction = null)
    {
        DebugHelper.Log("" + amount);
        if (string.IsNullOrWhiteSpace(playerId) || playerId == userDetails.Id)
        {
            DebugHelper.Log("Dummy Data" + amount);
            userDetails.balance -= amount;
            OnUserBalanceUpdate.Invoke();
        }
        else
        {
            DebugHelper.Log(playerId + " __ " + userDetails.Id + "__ Dummy Data ---1" + amount);
        }
        action?.Invoke(true);
        return 0;

    }

    public void AddBet(int index, string BetId, TransactionMetaData metadata, float amount, Action<bool> action = null, string playerId = "", bool isBot = false)
    {
        if (playerId == "" || playerId == userDetails.Id)
        {
            userDetails.balance -= amount;
            OnUserBalanceUpdate.Invoke();
        }
        action?.Invoke(true);
        return;
    }

    public void CancelBet(int index, string metadata, float amount, Action<bool> action = null, string playerId = "", bool isBot = false)

    {
        if (playerId == "" || playerId == userDetails.Id)
        {
            userDetails.balance += amount;
            OnUserBalanceUpdate.Invoke();
        }
        action?.Invoke(true);
        return;
    }

    public void WinningsBet(int index, double amount, double spend_amount, TransactionMetaData metadata, Action<bool> action = null, string playerId = "", bool isBot = false)
    {


        if (isPlayByDummyData)
        {
            if (playerId == "" || playerId == userDetails.Id)
            {
                DebugHelper.Log("Winning Bet Data **********");
                userDetails.balance += amount;
                OnUserBalanceUpdate.Invoke();
            }
            action?.Invoke(true);
            return;
        }
    }

    public void APICallBack(string url, int code, string body, string error)
    {
        DebugHelper.Log($"API_ Response :-  URL{url} -- Code {code} -- Body {body} -- Error {error}");

        foreach (var item in apiRequestList)
        {
            if (item.url == url)
            {
                if (code == 200)
                {
                    item.callback(true, error, body);
                }
                else
                {
                    item.callback(false, error, body);
                }
            }
        }
        apiRequestList.RemoveAll(x => x.url == url);
    }

    public async void ExecuteAPI(ApiRequest api, int timeout = 0)
    {
        WebApiManager.Instance.GetNetWorkCall(api.callType, api.url, api.param, (success, error, body) =>
        {
            DebugHelper.Log($"<color=orange>Success is set to {success}, error is set to {error} and body is set to {body}\nURL is : {api.url}</color>");
            if (success)
            {
                DebugHelper.Log($"<color=orange>API sent to success</color>");
                api.action?.Invoke(success, error, body);
            }
            else
            {
                if (timeout >= 3)
                {
                    api.action?.Invoke(success, error, body);
                    DebugHelper.Log($"<color=orange>API run failed with timeout {timeout}</color>");
                }
                else
                {
                    DebugHelper.Log($"<color=orange>API recalled with timeout set to {timeout}</color>");
                    ExecuteAPI(api, timeout++);
                }
            }
        }, defaultDelay);
    }
    public bool winningBetCalled;

    public async void WinningsBetMultiplayerAPI(int betIndex, string betId, double win_amount_with_comission, double spend_amount, double pot_amount, TransactionMetaData metadata, Action<bool> action, string playerId, bool isBot, bool isWinner, string gameName, string operatorName, string gameId, float commission, string matchToken)
    {
        winningBetCalled = true;
        string reqID = "";
        DebugHelper.Log($"BetIndex: {betIndex}, playerId: {playerId}, matchToken: {matchToken} , BetId : {betId},betRequest Count + {betRequest.Count}");
        BetRequest request = betRequest.Find(x => x.betId == betIndex && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != betId)
        {

            DebugHelper.Log("Request Bet" + request.BetId + "betID" + betId);

            await UniTask.Delay(200);
        }

        double currentBalance = userDetails.balance + win_amount_with_comission;
        DebugHelper.Log("Winning Bet amount" + win_amount_with_comission + "==" + currentBalance + "==" + APIController.instance.userDetails.balance + "==" + spend_amount);
#if CasinoGames
        bool winningBetResponce = false;
        /*reqID = BaseSocketController.instance.WSS_WinningBet(betId, isWin ? 1 : 0, win_amount_with_comission, spend_amount,
        (initatedres) =>
        {

            //winningBetResponce = true;

            // DebugHelper.Log(initatedres);
            // double userbalance = currentBalance;
            // UpdateBalanceResponse(userbalance);
            // action?.Invoke(true);
            DebugHelper.Log("WinningsBetMultiplayerAPI Initialized" + initatedres);
        },
        (successRes) =>
        {
            OnInternetStatusChange?.Invoke(NetworkStatus.Active);
            winningBetResponce = true;
            DebugHelper.Log(successRes);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(successRes);
            action?.Invoke(response != null && (response.code == 200 || response.code == 224));
            if (response.code == 224)
            {
                GetUpdatedBalance();
                return;
            }
            BetHistoryHandler.Instance.UpdateBet(new Betlist
            {
                dateTime = DateTime.Now.ToString(),
                id = betId,
                bet_amount = spend_amount,
                win_amount = win_amount_with_comission,
                matchID = matchToken
            });
            JObject json = JObject.Parse(response.message);
            double userbalance = (double)json["balance"];
            UpdateBalanceResponse(userbalance);
        },
        (failRes) =>
        {
            DebugHelper.Log("WinningsBetMultiplayerAPI Failed" + failRes);
            JObject jobject = JObject.Parse(failRes);
            matchResponse.status = false;
            matchResponse.Message = jobject["message"]?.ToString();
            ErrorPopUpHandler.instance.ShowError((int)jobject["code"], jobject["message"].ToString());
        });*/


        float time = Time.time;
        bool waitingForresponseActive = false;
        /*while (!winningBetResponce)
        {
            if (!waitingForresponseActive && Time.time - time > waitingForResponseDelay)
            {
                waitingForresponseActive = true;
                OnInternetStatusChange?.Invoke(NetworkStatus.WaitingforResponse);
            }
            if (Time.time - time > retryDelay)
            {
                if (BaseSocketController.instance.IsOnline())
                {
                    BaseSocketController.instance.RemoveRequestEvent(reqID);
                    WinningsBetMultiplayerAPI(betIndex, betId, win_amount_with_comission, spend_amount, pot_amount, metadata, action, playerId, isBot, isWinner, gameName, operatorName, gameId, commission, matchToken);
                    return;
                }
            }
            await UniTask.Delay(100);
        }*/
#endif
    }

    public async void GetRandomPredictionIndexApi(int rowCount, int columnCount, int predectedCount, Action<string, bool> OnScucces = null, string gamename = "")
    {
#if CasinoGames

        bool randomPredictionResponce = false;
        string reqID = "";
       /* reqID = BaseSocketController.instance.WSS_GetRandomPredictions(rowCount, columnCount, predectedCount, gamename,
         (init) =>
         {
             randomPredictionResponce = true;

         },
         (success) =>
         {

             ApiResponse response = JsonUtility.FromJson<ApiResponse>(success);
             if (response.code == 200)
             {
                 OnInternetStatusChange?.Invoke(NetworkStatus.Active);
                 OnScucces?.Invoke(response.message, true);
             }
             else
             {
                 OnScucces?.Invoke(success, false);
             }
         },
         (error) =>
         {

         }
         );*/
        float time = Time.time;
        bool waitingForresponseActive = false;
        while (!randomPredictionResponce)
        {

            if (!waitingForresponseActive && Time.time - time > waitingForResponseDelay)
            {
                waitingForresponseActive = true;
                OnInternetStatusChange?.Invoke(NetworkStatus.WaitingforResponse);
            }
            /*if (Time.time - time > retryDelay)
            {
                if (BaseSocketController.instance.IsOnline())
                {
                    BaseSocketController.instance.RemoveRequestEvent(reqID);
                    DebugHelper.Log("WinningBetResponce  Retry Called");
                    GetRandomPredictionIndexApi(rowCount, columnCount, predectedCount, OnScucces, gamename);

                    return;
                }
            }*/
            await UniTask.Delay(100);
        }
#endif
    }

    public async void AddBetMultiplayerAPI(int index, string BetId, TransactionMetaData metadata, double amount, Action<bool> action, string playerId, bool isBot, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == index && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != BetId)
        {
            await UniTask.Delay(200);
        }

        /*BaseSocketController.instance.WSS_AddBet(BetId, amount, matchToken,
        (initatedres) =>
        {
            // DebugHelper.Log("Add Bet Init Res :: " + initatedres);
            // action?.Invoke(true);
            // UpdateBalanceResponse(authentication.balance);
            // DebugHelper.Log("Add Bet Init Res :: " + initatedres);
        },
        (successRes) =>
        {
            OnInternetStatusChange?.Invoke(NetworkStatus.Active);
            DebugHelper.Log("Add Bet Success Res :: " + successRes);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(successRes);
            action?.Invoke(response != null && (response.code == 200 || response.code == 224));
            if (response.code == 224)
            {
                GetUpdatedBalance();
                return;
            }
            JObject json = JObject.Parse(response.message);
            double userbalance = (double)json["balance"];
            UpdateBalanceResponse(userbalance);
        },
        (failRes) =>
        {
            DebugHelper.Log("Add Bet Failed" + failRes);
            JObject jobject = JObject.Parse(failRes);
            matchResponse.status = false;
            matchResponse.Message = jobject["message"]?.ToString();
            ErrorPopUpHandler.instance.ShowError((int)jobject["code"], jobject["message"].ToString());
        });*/
        return;
    }

    /*
     Dictionary<string, string> bodyDict = new Dictionary<string, string>
            {
                { "env", environment},
                { "betAmount", betAmount },
                { "CardData",JsonConvert.SerializeObject(GameController.Instance.CardListToServer) }
            };
            AWS_SocketController.instance.GetRandomCard();
    */
   /* public string GetPredictionValue(string payload, Action<string> initAction, Action<string> successAction, Action<string> errorAction)
    {
        return BaseSocketController.instance.FetchGamePrediction("Prediction", authentication.gamename, payload, initAction, successAction, errorAction);
    }*/

    public CreateMatchResponse matchResponse;
    public async void CreateAndJoinMatch(int index, double amount, TransactionMetaData metadata, bool isAbleToCancel, string lobbyName, string playerId, bool isBot, string gameName, string operatorName, string game_ID, bool isBlockAPI, List<string> players, Action<CreateMatchResponse> initalizedAction, Action<int, CreateMatchResponse> successAction, Action<CreateMatchResponse> errorAction)
    {
        DebugHelper.Log("1CreateAndJoinMatch 1 init => " + index);


        matchResponse = new CreateMatchResponse();
        if (isBlockAPI)
        {
            matchResponse.status = true;
            matchResponse.MatchToken = DateTime.UtcNow.ToString().ToGuid().ToString();
            successAction.Invoke(index, matchResponse);
            return;
        }

        if (betRequest.Exists(x => x.betId == index))
        {
            DebugHelper.Log("Checking BetRequest already Exists ===> " + index);
            betRequest.RemoveAll(x => x.betId == index);
        }

        BetRequest bet = new BetRequest();
        bet.PlayerId = playerId;
        bet.betId = index;
        betRequest.Add(bet);
        DebugHelper.Log($"BetRequest JSON Temp before Response.....BetIndex_{index}");
        bool createandjoingameResponseReceived = false;
        string reqID = "";
        /*reqID = BaseSocketController.instance.WSS_CreateAndJoin(lobbyName, index, amount, isAbleToCancel, JsonConvert.SerializeObject(metadata),

         (initiatedres) =>
         {
             //createandjoingameResponseReceived = true;
             DebugHelper.Log(" 2CreateAndJoinMatch 1 init => " + initiatedres);
             JObject obj = JObject.Parse(initiatedres);
             string message = obj["message"]?.ToString();
             DebugHelper.Log(message);
             if (message.Contains("timeout"))
             {
                 // ErrorPopUpHandler.instance.ShowError("Request TimeOut");
                 matchResponse.status = false;
                 matchResponse.Message = initiatedres;
                 errorAction?.Invoke(matchResponse);
                 DebugHelper.Log("CreateAndJoinMatch Failed " + initiatedres);
             }
             else
             {
                 DebugHelper.Log("CreateAndJoinMatch 1 => " + initiatedres);
             }
         },
         (successres) =>
         {

             OnInternetStatusChange?.Invoke(NetworkStatus.Active);
             createandjoingameResponseReceived = true;
             DebugHelper.Log("3CreateAndJoinMatch 1 init =>" + successres);
             JObject jsonObject = JObject.Parse(successres);
             if ((int)jsonObject["code"] == 200)
             {
                 winningBetCalled = false;
                 JObject jsonObject1 = JObject.Parse(jsonObject["data"].ToString());
                 DebugHelper.Log("jsonObject1  => " + double.Parse(jsonObject1["balance"].ToString()));
                 matchResponse.status = true;
                 matchResponse.MatchToken = jsonObject1["MatchToken"].ToString();
                 matchResponse.MatchCount = int.Parse(jsonObject1["MatchCount"].ToString());
                 matchResponse.WinChance = double.Parse(jsonObject1["WinChance"].ToString());
                 matchResponse.balance = double.Parse(jsonObject1["balance"].ToString());
                 matchResponse.Message = jsonObject1["Message"].ToString();
                 matchResponse.IsRandom = int.Parse(jsonObject1["IsRandom"].ToString());
                 bet.BetId = jsonObject1["Message"].ToString();
                 bet.MatchToken = jsonObject1["MatchToken"].ToString();
                 UpdateBalanceResponse(matchResponse.balance);
                 successAction?.Invoke(index, matchResponse);
                 DebugHelper.Log(JsonConvert.SerializeObject(matchResponse) + "CreateAndJoinMatch 2 => " + bet.BetId + " ... " + bet.MatchToken + "....Balance" + matchResponse.balance);
                 BetHistoryHandler.Instance.AddBet(new Betlist
                 {
                     dateTime = DateTime.Now.ToString(),
                     id = bet.BetId,
                     bet_amount = amount,
                     win_amount = 0,
                     matchID = bet.MatchToken
                 });
                 successAction = null;
             }
             else if ((int)jsonObject["code"] == 224)
             {

             }
             else
             {
                 *//*
                              401 - User token is invalid
                              402 - Insufficient fund
                              403 - User token is expired
                              405 - Internal error with no retry
                              409 - Duplicate transaction
                              413 - Invalid Client-Signature
                              500 - Internal error
                              502 - Error in lootrix side
                              408 - timeout
                              *//*
                 // switch((int)jsonObject["code"])
                 // {
                 //     case 402:
                 // }
                 matchResponse.Message = "";

                 DebugHelper.Log(jsonObject.ToString());

                 ErrorPopUpHandler.instance.ShowError((int)jsonObject["code"], jsonObject["message"]?.ToString());
                 matchResponse.status = false;
                 errorAction?.Invoke(matchResponse);

             }
         },
         (errorres) =>
         {

             JObject jobject = JObject.Parse(errorres);

             try
             {
                 int code = (int)jobject["code"];
                 createandjoingameResponseReceived = true;
             }
             catch (Exception ex)
             {

             }


             DebugHelper.Log("CreateAndJoinMatch Failed " + errorres);

             matchResponse.status = false;
             matchResponse.Message = jobject["message"]?.ToString();
             errorAction?.Invoke(matchResponse);
             ErrorPopUpHandler.instance.ShowError((int)jobject["code"], jobject["message"].ToString());

         });*/

        // Wait for 5 seconds for response
        float time = Time.time;
        bool waitingForresponseActive = false;
        /* while (!createandjoingameResponseReceived)
         {

             if (!waitingForresponseActive && Time.time - time > waitingForResponseDelay)
             {
                 waitingForresponseActive = true;
                 OnInternetStatusChange?.Invoke(NetworkStatus.WaitingforResponse);
             }
             if (Time.time - time > retryDelay)
             {
                 if (BaseSocketController.instance.IsOnline())
                 {
                     BaseSocketController.instance.RemoveRequestEvent(reqID);
                     DebugHelper.Log("NewCreateAndJoinMatch_1  Retry Called");
                     CreateAndJoinMatch(index, amount, metadata, isAbleToCancel, lobbyName, playerId, isBot, gameName, operatorName, game_ID, isBlockAPI, players, initalizedAction, successAction, errorAction);
                     return;
                 }
             }
             await UniTask.Delay(100);
         }*/
        while (!createandjoingameResponseReceived)
        {

            if (!waitingForresponseActive && Time.time - time > waitingForResponseDelay)
            {
                waitingForresponseActive = true;
                OnInternetStatusChange?.Invoke(NetworkStatus.WaitingforResponse);
            }
            if (Time.time - time > retryDelay)
            {
                OnInternetStatusChange?.Invoke(NetworkStatus.NetworkIssue);
                /*BaseSocketController.instance.RemoveRequestEvent(reqID);
                DebugHelper.Log("CreateAndJoinMatch 3 => " + createandjoingameResponseReceived);
                bool pingSuccess = await BaseSocketController.instance.TryPing();*/
                /*if (pingSuccess)
                {
                    //OnInternetStatusChange?.Invoke(NetworkStatus.Active);
                    DebugHelper.Log("NewCreateAndJoinMatch_1  Retry Called");
                    CreateAndJoinMatch(index, amount, metadata, isAbleToCancel, lobbyName, playerId, isBot, gameName, operatorName, game_ID, isBlockAPI, players, initalizedAction, successAction, errorAction);
                    return;
                }*/
            }
            await UniTask.Delay(500);
        }
        // return index;
    }

    #endregion
    public void UpdateAudioSettings()
    {

        LocalStorage.Save("Lootrix_sound", APIController.instance.authentication.sound ? "true" : "false");
        LocalStorage.Save("Lootrix_music", APIController.instance.authentication.music ? "true" : "false");
        return;

#if !UNITY_EDITOR
        SetAudio(APIController.instance.authentication.sound ? 1 : 0, APIController.instance.authentication.music ? 1 : 0);
#endif
        return;
        if (APIController.instance.userDetails.isBlockApiConnection)
            return;
        var param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "requestType", value = "audio" });
        param.Add(new KeyValuePojo { keyId = "session_token", value = APIController.instance.authentication.session_token });
        param.Add(new KeyValuePojo { keyId = "user_id", value = APIController.instance.authentication.Id });
        param.Add(new KeyValuePojo { keyId = "sound", value = (APIController.instance.authentication.sound ? 1 : 0).ToString() });
        param.Add(new KeyValuePojo { keyId = "music", value = (APIController.instance.authentication.music ? 1 : 0).ToString() });
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, BackendAPIURL.LootrixAudioUpdate, param, (success, error, body) =>
        {
            if (success)
            {
                DebugHelper.Log("Audio Settings Has been Updated");
            }
        });
    }
    public void ServerInactiveAPI()
    {
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, BackendAPIURL.LootrixServerInactiveAPI, new List<KeyValuePojo>()
      {
          new KeyValuePojo { keyId = "requestType", value = "ServerInactive" }, new KeyValuePojo { keyId = "Id", value = userDetails.gameId }, new KeyValuePojo { keyId = "Host", value = "Nakama.Helpers.NakamaManager.Instance.connectedHost" }
      }, (bool isSuccess, string error, string body) => { }, 2);
    }

    #region  TODO
    public async void GetLambdaURL(bool isLive)
    {
        return;
        bool success = false;
        while (!success)
        {
            ApiRequest apiRequest = new ApiRequest();///?requestType=GetGameServer&game_name=carrom
            apiRequest.url = "https://qllb52jc5pxturffykekbtewn40osanl.lambda-url.ap-south-1.on.aws/";
            List<KeyValuePojo> param = new List<KeyValuePojo>();
            param.Add(new KeyValuePojo { keyId = "LoginType", value = isLive ? "1" : "0" });
            param.Add(new KeyValuePojo { keyId = "GameName", value = "" });
            apiRequest.param = param;
            apiRequest.callType = NetworkCallType.GET_METHOD;
            apiRequest.action = (success1, error, body) =>
            {
                success = success1;

                if (success1)
                {
                    ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                    if (response.code == 200)
                    {
                        BackendAPIURL = JsonUtility.FromJson<BackendAPI>(response.message);
                        BackendAPIURL.isGetData = true;
                        // NakamaManager.Instance.connectedHost = BackendAPIURL.LootrixHost;
                        // DebugHelper.Log(NakamaManager.Instance.connectedHost + "DSFSDFSDF");
                    }
                }
            };
            ExecuteAPI(apiRequest, 3);
            await UniTask.Delay(3000);
        }
    }
    public async void CancelBetMultiplayerAPI(int betIndex, string betId, double amount, TransactionMetaData metadata, Action<bool> action, string playerId, bool isBot, bool isWinner, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == betIndex && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != betId)
        {
            await UniTask.Delay(200);
        }
        CancelBetReq cancelBetreq = new CancelBetReq();
        cancelBetreq.Amount = amount;
        cancelBetreq.GameID = gameId == "" ? userDetails.gameId : gameId;
        cancelBetreq.GameName = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName;
        cancelBetreq.Index = betIndex;
        cancelBetreq.IsBot = isBot;
        cancelBetreq.Metadata = metadata;
        cancelBetreq.Betid = betId;
        cancelBetreq.MatchToken = matchToken;
        cancelBetreq.OperatorName = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName;
        cancelBetreq.PlayerId = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId;
        // Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_CancelBet", cancelBetreq.ToJson(), (res) =>
        // {
        //     DebugHelper.Log(res);
        //     ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
        //     action?.Invoke(response != null && response.code == 200);
        //     JObject json = JObject.Parse(response.message);
        //     double userbalance = (double)json["balance"];
        //     UpdateBalanceResponse(userbalance);

        // });
        return;
    }
    public void GetRNG_API(float amount, string operatorname, string gameid, Action<bool, float, int> canWin, string gamename, float playersetmultiplier)
    {
        WinLoseRNG winlogic = new()
        {
            amount = amount,
            operatorName = operatorname,
            gameID = gameid,
            gameName = gamename,
            playerSetMultiplier = playersetmultiplier
        };

        // Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_GetIsWinOrLose", winlogic.ToJson(), (res) =>
        // {
        //     DebugHelper.Log("Rng Calculation inside GetRNG 2");
        //     JObject jsonObject = JObject.Parse(res);
        //     userDetails.isWin = ((int.Parse(jsonObject["iswin"].ToString()) > 0));
        //     userDetails.maxWin = float.Parse(jsonObject["Multiplier"].ToString());
        //     int gameCount = int.Parse(jsonObject["GameCount"].ToString());
        //     canWin.Invoke(userDetails.isWin, userDetails.maxWin, gameCount);
        // });
    }


    public void GetLastBet(string matchID)
    {
        if (authentication.operatorname == "demo")
        {
            return;
        }
        return;
        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "user_id", value = authentication.Id });
        param.Add(new KeyValuePojo { keyId = "operator", value = authentication.operatorname });
        param.Add(new KeyValuePojo { keyId = "game_id", value = userDetails.gameId });
        param.Add(new KeyValuePojo { keyId = "request_type", value = "getleastbet" });
        param.Add(new KeyValuePojo { keyId = "limit", value = "20" });
        param.Add(new KeyValuePojo { keyId = "match_id", value = matchID });
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_JSONDATA, authentication.client_url, param, (success, error, body) =>
        {
            if (success)
            {
                JObject jsonObject = JObject.Parse(body);
                if ((int)(jsonObject["code"]) == 200)
                {
                    ///betlist
                    JObject betlistString = JObject.Parse(jsonObject["data"].ToString());

                    List<Betlist> betlistArray = JsonConvert.DeserializeObject<List<Betlist>>(betlistString["betlist"]?.ToString());
                    int balance = int.Parse(betlistString["balance"].ToString());

                }
                else
                {
                    DebugHelper.Log("GetLastBet response is : " + (string)jsonObject["message"]);
                }
            }
            else
            {
                DebugHelper.Log("GetLastBet response is : " + error);
            }
        });
    }
    #endregion
}