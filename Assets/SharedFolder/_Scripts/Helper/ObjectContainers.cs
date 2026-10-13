using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class BackendAPI
{
    //dev https://b465kezbry2ssew2w6w56x3jhi0ennyi.lambda-url.ap-south-1.on.aws/
    //live https://56ebdif5s4aqltb74xhkvnnrai0sxaey.lambda-url.ap-south-1.on.aws/
    public string LootrixMatchAPI = "https://b465kezbry2ssew2w6w56x3jhi0ennyi.lambda-url.ap-south-1.on.aws/";
    //live https://xpxpmhpjldqvjulexwx34z7jca0kdxzf.lambda-url.ap-south-1.on.aws/
    //dev https://vbklyx2pq3nh6xf3vr55vmrwem0ldawq.lambda-url.ap-south-1.on.aws/
    public string LootrixTransactionAPI = "https://vbklyx2pq3nh6xf3vr55vmrwem0ldawq.lambda-url.ap-south-1.on.aws/";
    //live https://htatuqjumsb7qhv3yvmfx6jlza0pdwyg.lambda-url.ap-south-1.on.aws/
    public string LootrixGetServerAPI = "https://htatuqjumsb7qhv3yvmfx6jlza0pdwyg.lambda-url.ap-south-1.on.aws/";
    // https://6rugffwb323fkm7j7umild4vjm0hfcfm.lambda-url.ap-south-1.on.aws/
    public string LootrixInternetCheckAPI = "https://6rugffwb323fkm7j7umild4vjm0hfcfm.lambda-url.ap-south-1.on.aws/";
    // https://vbklyx2pq3nh6xf3vr55vmrwem0ldawq.lambda-url.ap-south-1.on.aws/
    public string LootrixValidateServerAPI = "https://vbklyx2pq3nh6xf3vr55vmrwem0ldawq.lambda-url.ap-south-1.on.aws/";
    public string LootrixServerInactiveAPI = "https://waekhvdxviqdmzdzo6hisjqsli0bvajw.lambda-url.ap-south-1.on.aws/?requestType=ServerInactive&Id&Message";

    public string LootrixAudioUpdate = "https://lfu76bhfx3dif4sglqij4qu6ou0hghmx.lambda-url.ap-south-1.on.aws/";
    public string LootrixHost = "turbogames.utwebapps.com";
    public bool isGetData = false;
}

[System.Serializable]
public class ApiRequest
{
    /*public NetworkCallType callType;*/
    public string url;
    /*public List<KeyValuePojo> param;*/
    public Action<bool, string, string> action;
}

[Serializable]
public class BetRequest
{
    public string BetId;
    public string PlayerId;
    public string MatchToken;
    public int betId;
}


[Serializable]
public class APIRequestList
{
    public string url;
    /*public ReqCallback callback;*/
}

public class InitBetDetails
{
    public bool status;
    public GameWinningStatus message;
    public int index;
}

[System.Serializable]
public class MinMaxOffest
{
    public float min;
    public float max;
}

[System.Serializable]
public class GameWinningStatus
{
    public string Id;
    public double Amount;
    public MinMaxOffest WinCutOff;
    public float WinProbablity;
    public string Game_Id;
    public string Operator;
    public DateTime create_at;
}

[System.Serializable]
public class UserGameData
{
    public string Id;
    public string name;
    public string token;
    public string session_token;
    public double balance;
    public string currency_type;
    public string game_Id;
    public string gameId;
    public string operatorDomainUrl;
    public string platform;
    public bool isBlockApiConnection;



    public double bootAmount;
    public bool isWin; //lootrix
    public bool hasBot;
    public float commission;
    public float maxWin;
    // public BetAmountDetails betAmountDetails = new();
}

[System.Serializable]
public class TitleData
{
    public string api_url;
    public string wss_url;
    public string server_url;
    public string server_port;
    public string game_name;
    public string game_id;
}


[System.Serializable]
public class AuthenticationData
{
    public string Id;
    public string name;
    public string token;
    public string session_token;
    public double balance;
    public string currency_type;
    public string gamename;
    public string operatorname;
    public string operatorDomainUrl;
    public string platform;
    public string returnurl;
    public string client_url;
    public bool music = true;
    public bool sound = true;

    public string environment = "";

    public string server_type = "colyseus"; //colyseus or nakama

    public string title_data;

    public EntryAmountDetails entryAmountDetails = new EntryAmountDetails();
}


[System.Serializable]
public class EntryAmountDetails
{
    public List<int> betValues = new List<int>();
    public int maxBetValue;
    public int minBetValue;
    public int incrementValue;
    public int decrementValue;
    public List<int> entryAmounts = new List<int>();
    public List<int> smallBlinds = new List<int>();
    public List<int> potLimits = new List<int>();
    public List<int> chaalLimits = new List<int>();
    public int player_count = 2;
    public float comission = 0f;

    public void SetEntryAmount(string entryData, string currency_type)
    {
        // DebugHelper.Log(entryData);
        if (string.IsNullOrEmpty(entryData))
        {
            SetDefaultAmount(currency_type);
            return;
        }
        JObject entrydata = JObject.Parse(entryData);
        betValues = JsonConvert.DeserializeObject<List<int>>(entrydata["betValues"].ToString());
        maxBetValue = int.Parse(entrydata["maxBetValue"].ToString());
        minBetValue = int.Parse(entrydata["minBetValue"].ToString());
        incrementValue = int.Parse(entrydata["incrementValue"].ToString());
        decrementValue = int.Parse(entrydata["decrementValue"].ToString());
        entryAmounts = JsonConvert.DeserializeObject<List<int>>(entrydata["entryAmounts"].ToString());
        smallBlinds = JsonConvert.DeserializeObject<List<int>>(entrydata["smallBlinds"].ToString());
        potLimits = JsonConvert.DeserializeObject<List<int>>(entrydata["potLimits"].ToString());
        chaalLimits = JsonConvert.DeserializeObject<List<int>>(entrydata["chaalLimits"].ToString());
        //player_count = int.Parse(entrydata["player_count"].ToString());
        //comission = int.Parse(entrydata["comission"].ToString());
    }
    public void SetDefaultAmount(string currency_type)
    {
        if (currency_type == "USD" || currency_type == "EUR")
        {
            betValues = new List<int> { 1, 2, 5, 10 };
            minBetValue = 1;
            maxBetValue = 100;
            incrementValue = 1;
            decrementValue = 1;
            entryAmounts = new List<int> { };
            smallBlinds = new List<int> { };
            potLimits = new List<int> { };
            chaalLimits = new List<int> { };
        }
        else
        {
            betValues = new List<int> { 10, 100, 150, 250 };
            minBetValue = 10;
            maxBetValue = 100;
            incrementValue = 10;
            decrementValue = 10;
            entryAmounts = new List<int> { };
            smallBlinds = new List<int> { };
            potLimits = new List<int> { };
            chaalLimits = new List<int> { };
        }
    }

}
[Serializable]
public class BetAmountDetails
{
    public float[] BetValues = new float[] { };
    public float MaxBetValue;
    public float MinBetValue;
    public float IncrementValue;
    public float DecrementValue;
}
[System.Serializable]
public class TransactionMetaData
{
    public double Amount;
    public string Info;
}
public class CreateAndJoinGameReq
{
    public string CreateBy;
    public string GameName;
    public string LobbyName;
    public string Operator;
    public string GameId;
    public string currency;
    public string provider;
    public string session_token;
    public string platform;
    public int Index;
    public double Amount;
    public TransactionMetaData Metadata;
    public bool IsAbleToCancel;
    public bool IsBot;
    public List<string> Players;
}
public class InitBetReq
{
    public int Index;
    public double Amount;
    public TransactionMetaData Metadata;
    public bool IsAbleToCancel;
    public bool IsBot;
    public string GameName;
    public string OperatorName;
    public string GameID;
    public string MatchToken;
    public string PlayerId;
    public string PlayerName;
}

public class GetPredictionReq
{
    public string RowCount;
    public string ColumnCount;
    public string PredictionCount;
    public string GameName;

}
public class GetRandomCardReq
{
    public string RiskCount;
    public string CardData;
    public string Currentval;
    public string TotalMultiplier;
    public string Status;
    public string Amount;
    public string Operator;
}
public class CancelBetReq
{
    public int Index;
    public string Betid;
    public double Amount;
    public TransactionMetaData Metadata;
    public bool IsBot;
    public string GameName;
    public string OperatorName;
    public string GameID;
    public string PlayerId;
    public string MatchToken;
}

[System.Serializable]
public class BetDetails
{
    public string betID;
    public int index;
    public BetProcess Status;
    public string IsAbleToCancel;
    public Action<bool> action;
    public Action<string> betIdAction;
}

public enum BetProcess
{
    Processing = 0,
    Success = 1,
    Failed = 2,
    None = 3
}

[System.Serializable]
public class ApiResponse
{
    public int code;
    public string message;
    public string data;
    public string output;
}

public class ExternalAPIRequest
{
    public string data;
}

public class NakamaApiResponse
{
    public int Code;
    public string Message;
}

public class BetResponse
{
    public bool status;
    public string message;
    public int index;
}

[System.Serializable]
public class BotDetails
{
    public string userId;
    public string name;
    public double balance;
}

public static class MatchExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new Guid(hashBytes);
    }
}
[System.Serializable]
public class CreateMatchResponse
{
    public bool status;
    public string MatchToken;
    public int MatchCount;
    public double WinChance;
    public string Message;
    public int IsRandom;
    public double balance;
}



public class WinLoseRNG
{
    public double amount;
    public string operatorName;
    public string gameID;
    public string gameName;
    public double playerSetMultiplier;

}


// public static class ShowLog()
// {
//    // public Color color = Color.Black;
// }


// class colorLog(message, color)
// {



//     switch (color)
//     {
//         case "success":
//             color = "Green";
//             break;
//         case "info":
//             color = "DodgerBlue";
//             break;
//         case "error":
//             color = "Red";
//             break;
//         case "warning":
//             color = "Orange";
//             break;
//         default:
//             color = color;
//     }

// console.log("%c" + message, "color:" + color);
// }