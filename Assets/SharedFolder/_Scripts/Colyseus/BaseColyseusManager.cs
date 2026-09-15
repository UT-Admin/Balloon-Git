using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Colyseus;
using Colyseus.Schema;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IQPlay.GameClient
{
    // Non-generic surface for UI widgets (PlayerCountDisplay, BetHistoryHandler)
    // that need to reference "whichever concrete manager this game has" without
    // depending on its TRoomState type — assign the manager's GameObject to a
    // [SerializeField] MonoBehaviour field and cast to this at runtime.
    public interface IRoomManager
    {
        event Action OnStateChanged;
        event Action<List<Betlist>> OnBetHistoryReceived;
        ushort PlayerCount { get; }
        void RequestBetHistory();
    }

    public abstract class BaseColyseusManager<TRoomState> : MonoBehaviour, IRoomManager where TRoomState : Schema
    {
        protected Client _client;
        protected Room<TRoomState> _room;

        public TRoomState RoomState => _room?.State;
        public bool IsConnected => _room != null;
        public bool IsSessionFailed { get; protected set; }
        public float Balance { get; protected set; }
        public string Currency { get; protected set; } = "USD";
        public ushort PlayerCount => GetPlayerCount();

        // Override to read the concrete TRoomState's own playerCount field —
        // the base class can't reach it directly since Colyseus schema fields
        // are plain public fields per game (e.g. AviatorRoomState.playerCount),
        // not something a shared interface constraint on TRoomState can require.
        protected virtual ushort GetPlayerCount() => 0;

        public event Action<PlayerStateData> OnPlayerStateReceived;
        public event Action<ErrorData> OnErrorReceived;
        public event Action OnConnected;
        public event Action<string> OnConnectionFailed;
        public event Action OnReconnecting;
        public event Action<List<Betlist>> OnBetHistoryReceived;
        public event Action OnStateChanged;

        protected abstract string GameSlug { get; }

        private string PrefRoomId => $"{GameSlug}_colyseus_room_id";
        private string PrefToken => $"{GameSlug}_colyseus_token";
        private string PrefSeed => $"{GameSlug}_client_seed";

        protected string _clientSeed;
        private string _sessionEndReason;
        protected bool _isConnecting;
        private string _playerJwt;
        private bool _isSoftReconnect;
        private bool _pendingReconnectCheck;
        private List<Betlist> _pendingBetHistory;

#if UNITY_WEBGL && !UNITY_EDITOR
        private static string GetPref(string key)             => WebGLSessionBridge.GetSessionItem(key);
        private static void   SetPref(string key, string val) => WebGLSessionBridge.SetSessionItem(key, val);
        private static void   DeletePref(string key)          => WebGLSessionBridge.RemoveSessionItem(key);
#else
        private static string GetPref(string key) => PlayerPrefs.GetString(key, "");
        private static void SetPref(string key, string val) { PlayerPrefs.SetString(key, val); PlayerPrefs.Save(); }
        private static void DeletePref(string key) { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
#endif

        // Subclasses own the typed singleton (static members don't resolve
        // polymorphically), so each subclass's own Awake() should check/set
        // its own Instance, then call base.Awake() for the shared init below.
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _clientSeed = GetPref(PrefSeed);
            if (string.IsNullOrEmpty(_clientSeed))
            {
                _clientSeed = Guid.NewGuid().ToString("N");
                SetPref(PrefSeed, _clientSeed);
            }

            BalloonGameConfig.Init(GameSlug);
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLSessionBridge.NotifyGameVersion(Application.version);
#endif
            _client = new Client(BalloonGameConfig.ServerUrl);
        }

        protected virtual void Start() => ConnectAsync().Forget();

        protected virtual void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            var roomToLeave = _room;
            _room = null;
            _ = roomToLeave?.Leave();
            ConnectAsync().Forget();
        }

        protected async UniTask<string> ExchangeSessionCodeAsync(string code)
        {
            var url = $"{BalloonGameConfig.ApiUrl}/game/session";
            var json = $"{{\"code\":\"{code}\"}}";
            var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            await req.SendWebRequest().ToUniTask();

            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception($"Session exchange failed: {req.error} — {req.downloadHandler.text}");

            var response = JsonUtility.FromJson<SessionResponse>(req.downloadHandler.text);
            if (string.IsNullOrEmpty(response?.token))
                throw new Exception("Session exchange returned empty token");

            return response.token;
        }

        protected async UniTaskVoid ConnectAsync()
        {
            if (_isConnecting) return;
            _isConnecting = true;

            var savedRoomId = GetPref(PrefRoomId);
            var savedToken = GetPref(PrefToken);

            string jwtToken = null;
            if (!BalloonGameConfig.IsB2BMode)
            {
                DeletePref(PrefRoomId);
                DeletePref(PrefToken);
                savedRoomId = null;
                savedToken = null;
            }

            if (BalloonGameConfig.IsB2BMode)
            {
                if (!string.IsNullOrEmpty(_playerJwt))
                {
                    jwtToken = _playerJwt;
                }
                else
                {
                    try
                    {
                        jwtToken = await ExchangeSessionCodeAsync(BalloonGameConfig.SessionCode);
                        _playerJwt = jwtToken;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[{GameSlug}] session exchange failed — {e.Message}");
                        _isConnecting = false;
                        _isSoftReconnect = false;
                        IsSessionFailed = true;
#if UNITY_WEBGL && !UNITY_EDITOR
                        WebGLSessionBridge.NotifySessionExpired();
#endif
                        OnConnectionFailed?.Invoke("SESSION_EXPIRED");
                        return;
                    }
                }
            }

            var joinOptions = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(jwtToken))
                joinOptions["token"] = jwtToken;
            joinOptions["operatorId"] = BalloonGameConfig.IsB2BMode && !string.IsNullOrEmpty(BalloonGameConfig.Operator)
                ? BalloonGameConfig.Operator
                : "demo";

            try
            {
                if (!BalloonGameConfig.IsB2BMode && !string.IsNullOrEmpty(savedRoomId) && !string.IsNullOrEmpty(savedToken))
                {
                    try
                    {
                        var rt = new ReconnectionToken { RoomId = savedRoomId, Token = savedToken };
                        _room = await _client.Reconnect<TRoomState>(rt);
                    }
                    catch
                    {
                        DeletePref(PrefRoomId);
                        DeletePref(PrefToken);
                        _room = await _client.JoinOrCreate<TRoomState>(GameSlug, joinOptions);
                    }
                }
                else
                {
                    _room = await _client.JoinOrCreate<TRoomState>(GameSlug, joinOptions);
                }

                SetPref(PrefRoomId, _room.ReconnectionToken.RoomId);
                SetPref(PrefToken, _room.ReconnectionToken.Token);

                RegisterCommonHandlers();
                RegisterGameHandlers();

                _isSoftReconnect = false;
                _isConnecting = false;
                _pendingReconnectCheck = false;
                SessionState.isOnline = true;
                SafeInvokeInternetStatus(NetworkStatus.Active);
                OnConnected?.Invoke();
            }
            catch (Exception e)
            {
                _isConnecting = false;
                Debug.LogError($"[{GameSlug}] connection failed — {e.Message}");
                if (_isSoftReconnect)
                {
                    _isSoftReconnect = false;
                    _playerJwt = null;
                    IsSessionFailed = true;
                    SafeInvokeConnectionFailed("SESSION_EXPIRED");
                    return;
                }
                if (e.Message.Contains("GAME_MISMATCH"))
                {
                    SafeInvokeConnectionFailed("GAME_MISMATCH");
                    return;
                }
                SafeInvokeConnectionFailed(e.Message);
                await UniTask.Delay(3000);
                ConnectAsync().Forget();
            }
        }

        protected void SafeInvokeConnectionFailed(string message)
        {
            try { OnConnectionFailed?.Invoke(message); }
            catch (Exception ex) { Debug.LogWarning($"[{GameSlug}] OnConnectionFailed subscriber threw, continuing anyway: {ex.Message}"); }
        }

        protected void SafeInvokeError(ErrorData error)
        {
            try { OnErrorReceived?.Invoke(error); }
            catch (Exception ex) { Debug.LogWarning($"[{GameSlug}] OnErrorReceived subscriber threw, continuing anyway: {ex.Message}"); }
        }

        protected void SafeInvokeInternetStatus(NetworkStatus status)
        {
            try { SessionEvents.OnInternetStatusChange?.Invoke(status); }
            catch (Exception ex) { Debug.LogWarning($"[{GameSlug}] OnInternetStatusChange subscriber threw, continuing anyway: {ex.Message}"); }
        }

        private void RegisterCommonHandlers()
        {
            _room.OnStateChange += (state, _isFirst) => OnStateChanged?.Invoke();

            _room.OnMessage<ErrorWrapper>("error", data =>
            {
                if (data?.error == null) return;
                if (data.error.code == "SESSION_EXPIRED" || data.error.code == "SESSION_TIMEOUT")
                    _sessionEndReason = data.error.code;
                SafeInvokeError(data.error);
            });

            _room.OnMessage<BetHistoryData>("bet_history", data =>
            {
                if (data?.bets == null || data.bets.Count == 0) return;
                var list = new List<Betlist>();
                foreach (var b in data.bets)
                    list.Add(new Betlist { id = b.id, bet_amount = b.bet_amount, win_amount = b.win_amount, matchID = b.matchID, dateTime = b.dateTime });
                _pendingBetHistory = list;
                OnBetHistoryReceived?.Invoke(list);
            });

            _room.OnLeave += code =>
            {
                SessionState.isOnline = false;
                if (!string.IsNullOrEmpty(_sessionEndReason))
                {
                    IsSessionFailed = true;
                    SafeInvokeConnectionFailed(_sessionEndReason);
                    return;
                }
                SafeInvokeInternetStatus(NetworkStatus.NetworkIssue);
                if (!_isConnecting)
                    ConnectAsync().Forget();
            };
        }

        // Override to register game-specific message handlers (e.g. "spin_result",
        // "tick", "cashout_confirmed") against the now-connected `_room`. Called
        // once per successful connect, right after RegisterCommonHandlers.
        protected virtual void RegisterGameHandlers() { }

        protected void HandlePlayerStateCommon(float balance, string currency, string username)
        {
            Balance = balance;
            Currency = string.IsNullOrEmpty(currency) ? Currency : currency;
            BalloonGameConfig.SetCurrency(Currency);
            SessionState.entryAmountDetails.currency_type = Currency;
            if (!string.IsNullOrEmpty(username))
            {
                SessionState.name = username;
                SessionState.Id = username;
            }
            SessionEvents.OnUserDetailsUpdate?.Invoke();
        }

        protected void RaisePlayerStateReceived(PlayerStateData data) => OnPlayerStateReceived?.Invoke(data);

        public List<Betlist> ConsumePendingBetHistory()
        {
            var data = _pendingBetHistory;
            _pendingBetHistory = null;
            return data;
        }

        public void SoftReconnect()
        {
            IsSessionFailed = false;
            _sessionEndReason = "";
            _isSoftReconnect = true;
            OnReconnecting?.Invoke();
            ConnectAsync().Forget();
        }

        public void RequestBalanceRefresh(string unused = null) =>
            _ = _room?.Send("refresh_balance", new { });

        public void RequestBetHistory() =>
            _ = _room?.Send("request_bet_history", new { });

        public void AddBalance(string amountStr)
        {
            if (float.TryParse(amountStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float amount))
                _ = _room?.Send("add_balance", new { amount });
        }

        public void NotifyFocusChange(string visibleStr)
        {
            bool visible = visibleStr == "1";
            SessionState.isInFocus = visible;
            SessionEvents.OnSwitchingTab?.Invoke(visible);
        }

        public void NotifyBrowserOffline(string unused = null)
        {
            _pendingReconnectCheck = true;
            OnBrowserOffline();
            SafeInvokeInternetStatus(NetworkStatus.NetworkIssue);
            if (!_isConnecting)
                ConnectAsync().Forget();
        }

        public void NotifyBrowserOnline(string unused = null)
        {
            SafeInvokeInternetStatus(NetworkStatus.Active);
            if (_pendingReconnectCheck)
            {
                _pendingReconnectCheck = false;
                if (!_isConnecting)
                    ConnectAsync().Forget();
            }
        }

        // Override to react to a browser-offline notification before the shared
        // reconnect-trigger runs — e.g. Roulette cancels an in-flight spin here.
        protected virtual void OnBrowserOffline() { }
    }

    // [DllImport] can't live inside a generic type (CS7042) — BaseColyseusManager<T>
    // is generic, so these externs are pulled out into their own plain static class.
#if UNITY_WEBGL && !UNITY_EDITOR
    internal static class WebGLSessionBridge
    {
        [DllImport("__Internal")] internal static extern string GetSessionItem(string key);
        [DllImport("__Internal")] internal static extern void   SetSessionItem(string key, string value);
        [DllImport("__Internal")] internal static extern void   RemoveSessionItem(string key);
        [DllImport("__Internal")] internal static extern void   NotifySessionExpired();
        [DllImport("__Internal")] internal static extern void   NotifyGameVersion(string version);
    }
#endif

    [Serializable] public class SessionResponse { public string token; }
    [Serializable] public class ErrorData { public string code; public string message; public bool retryable; }
    [Serializable] public class ErrorWrapper { public ErrorData error; }

    [Serializable]
    public class PlayerStateData
    {
        public float balance;
        public string currency;
        public string username;
    }

    [Serializable] public class BetHistoryEntry { public string id; public double bet_amount; public double win_amount; public string matchID; public string dateTime; }
    [Serializable] public class BetHistoryData { public List<BetHistoryEntry> bets; }

    [Serializable]
    public class Betlist
    {
        public string id;
        public string username;
        public double bet_amount;
        public double win_amount;
        public bool isCurrentGame = false;
        public string dateTime;
        public string matchID;

        public string GetDateAndTimeString() => GetDateAndTime().ToString("yyyy-MM-dd HH:mm:ss");

        public DateTime GetDateAndTime() => ConvertToLocalTime(dateTime);

        public DateTime ConvertToLocalTime(string dateTimeString) =>
            DateTime.TryParse(dateTimeString, out var parsed) ? parsed.ToLocalTime() : default;
    }
}
