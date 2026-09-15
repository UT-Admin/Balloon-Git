using System;
using System.Collections.Generic;
using IQPlay.GameClient;
using UnityEngine;

// Mirrors RouletteColyseusManager.cs: all connection/reconnect/JWT-exchange/
// SessionState plumbing lives in BaseColyseusManager<TRoomState>. Only the
// message set differs (start_round / set_holding / cash_out / tick /
// round_result instead of spin / spin_result), because Balloon's round is a
// live-ticking per-player cash-out, not a one-shot spin.
public class BalloonColyseusManager : BaseColyseusManager<BalloonRoomState>
{
    public static BalloonColyseusManager Instance { get; private set; }

    public event Action<TickData> OnTick;
    public event Action<RoundResultData> OnRoundResult;

    private bool _roundInFlight;

    protected override string GameSlug => "balloon";

    protected override void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        base.Awake();

        // Reset _roundInFlight on any error the base class surfaces, same as
        // the old inline "error" handler used to (SESSION_EXPIRED/TIMEOUT are
        // already handled by the base — this just clears our local flag).
        OnErrorReceived += _ => _roundInFlight = false;
    }

    protected override ushort GetPlayerCount() => RoomState?.playerCount ?? 0;

    protected override void RegisterGameHandlers()
    {
        _room.OnMessage<BalloonPlayerStateData>("player_state", data =>
        {
            HandlePlayerStateCommon(data.balance, data.currency, data.username);
            RaisePlayerStateReceived(data);
        });

        _room.OnMessage<TickData>("tick", data => OnTick?.Invoke(data));

        _room.OnMessage<RoundResultData>("round_result", data =>
        {
            Balance = data.balance;
            _roundInFlight = false;
            OnRoundResult?.Invoke(data);
        });
    }

    protected override void OnBrowserOffline()
    {
        if (!_roundInFlight) return;
        _roundInFlight = false;
        SafeInvokeError(new ErrorData { code = "CONNECTION_LOST", message = "Connection lost during round.", retryable = true });
    }

    // clientRoundId should be a fresh Guid per round (GameController generates
    // one the moment the pump button is pressed) — it's the idempotency key
    // the server uses to dedupe a retried start_round.
    public void SendStartRound(string clientRoundId, float betAmount, float autoTarget = 0f)
    {
        _roundInFlight = true;
        _ = _room?.Send("start_round", new { clientRoundId, betAmount, clientSeed = _clientSeed, autoTarget });
    }

    // Send on both press (holding=true) and release (holding=false) of the
    // pump button — replaces the local isPressed flag driving IncrementMultiplier().
    public void SendSetHolding(bool holding) =>
        _ = _room?.Send("set_holding", new { holding });

    public void SendCashOut() =>
        _ = _room?.Send("cash_out", new { });

    // Optimistic local balance display update (iqplay_balance_update bridge message)
    // — same pattern as ColyseusManager.DirectBalanceUpdate. Followed by a real
    // refresh_balance round trip so the server-held balance stays authoritative.
    public void DirectBalanceUpdate(string amountStr)
    {
        if (!float.TryParse(amountStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float newBalance)) return;

        Balance = newBalance;
        RaisePlayerStateReceived(new PlayerStateData { balance = newBalance, currency = Currency, username = SessionState.name });
        RequestBalanceRefresh();
    }
}

// Wire shape from the "player_state" message — extends the base's
// PlayerStateData (balance/currency/username) with the extra fields Balloon's
// live round state needs. Kept as its own type instead of redefining
// PlayerStateData, since that name is already taken in IQPlay.GameClient.
[Serializable]
public class BalloonPlayerStateData : PlayerStateData
{
    public string phase;        // idle | inflating | resolved
    public float betAmount;
    public float multiplier;
    public bool holding;
    public float autoTarget;
}

[Serializable]
public class TickData
{
    public float multiplier;
}

[Serializable]
public class RoundResultData
{
    public string clientRoundId;
    public string outcome;        // "cashed_out" | "burst"
    public float multiplier;
    public float payout;
    public string revealedSeed;   // only present on "burst"
    public string roundHash;      // only present on "burst"
    public float balance;
}
