using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using IQPlay.GameClient;

public class BetHistoryHandler : MonoBehaviour
{
    public static BetHistoryHandler Instance { get; private set; }
    [SerializeField] private List<Betlist> betLists = new List<Betlist>();
    [SerializeField] private GameObject betItemPrefab;
    [SerializeField] private Transform betContainer;
    [SerializeField] private GameObject betHistoryGameobject;
    [SerializeField] public GameObject loadMoreGameobject;
    [SerializeField] private Button loadMoreBtn;
    [SerializeField] private Button closeButton;
    private Dictionary<string, GameObject> betUIItems = new Dictionary<string, GameObject>();
    public List<Betlist> Betlists { get { return betLists; } private set { betLists = value; } }
    private Queue<GameObject> betItemPool = new Queue<GameObject>();
    private const int PoolSize = 5;
    private int _remainingBetCount;

    void Awake()
    {
        Instance = this;
        loadMoreBtn.onClick.AddListener(OnLoadMoreBetsClicked);
        closeButton.onClick.AddListener(HideBetHistory);
        InitializePool();
    }
    void Start()
    {
        /*APIController.instance.OnUserDetailsUpdate += OnUserDetailsUpdate;*/
        SessionEvents.OnUserDetailsUpdate += OnUserDetailsUpdate;
        BalloonColyseusManager.Instance.OnBetHistoryReceived += OnBetHistoryReceived;
    }
    private void OnBetHistoryReceived(List<Betlist> bets)
    {
        AddMyBetHistory(bets, 0);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowBetHistory();
        }
    }
    private void OnUserDetailsUpdate()
    {
        GetLastBet("-1");
    }
    void OnLoadMoreBetsClicked()
    {
        Debug.Log("Loading more bets...");
        if (_remainingBetCount > 0)
        {
            GetLastBet(betLists[^1].matchID);
        }
    }
    private void InitializePool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject obj = Instantiate(betItemPrefab, betContainer);
            obj.SetActive(false);
            betItemPool.Enqueue(obj);
        }
    }
    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        betItemPool.Enqueue(obj);
    }
    private GameObject GetPooledObject()
    {
        if (betItemPool.Count > 0)
        {
            GameObject obj = betItemPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(betItemPrefab, betContainer);
            return obj;
        }
    }
    public void GetLastBet(string matchID)
    {
        if (!BalloonGameConfig.IsB2BMode) return;
        BalloonColyseusManager.Instance.RequestBetHistory();

        /*var authentication = APIController.instance.authentication;
        var userDetails = APIController.instance.userDetails;

        if (authentication.operatorname == "demo")
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(userDetails.gameId))
        {
            return;
        }

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
                if ((int)jsonObject["code"] == 200)
                {
                    JObject betlistString = JObject.Parse(jsonObject["data"].ToString());
                    List<Betlist> betlistArray = JsonConvert.DeserializeObject<List<Betlist>>(betlistString["betlist"]?.ToString());
                    int balance = int.Parse(betlistString["balance"].ToString());
                    AddMyBetHistory(betlistArray, balance);
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
        });*/
    }
    public async void AddMyBetHistory(List<Betlist> bets, int remainingBetCount)
    {
        loadMoreGameobject.SetActive(false);
        DebugHelper.Log($"Adding {bets.Count} bets. Remaining on server: {remainingBetCount}");
        if (bets == null || bets.Count == 0)
        {
            Debug.LogWarning("No bets to add.");
            return;
        }
        foreach (var item in bets)
        {
            UpdateBet(item);
        }
        _remainingBetCount = remainingBetCount;
        SortBetsByDate();
        Debug.Log($"Processed {bets.Count} bets. Remaining on server: {_remainingBetCount}");
        await UniTask.Delay(200);
        loadMoreGameobject.SetActive(_remainingBetCount > 0);
    }
    public void RemoveBet(string betId)
    {
        betLists.RemoveAll(b => b.id == betId);
        if (betUIItems.TryGetValue(betId, out GameObject item))
        {
            ReturnToPool(item);
            betUIItems.Remove(betId);
        }
    }
    public void AddBet(Betlist bet)
    {
        UpdateBet(bet);
    }
    public void UpdateBet(Betlist bet)
    {
        if (bet == null)
        {
            Debug.LogWarning("Bet is null. Cannot add.");
            return;
        }

        if (!betLists.Exists(x => x.id == bet.id))
        {
            betLists.Add(bet);
            GameObject newItem = GetPooledObject();
            MyBetDetailsContailer betUI = newItem.GetComponent<MyBetDetailsContailer>();
            if (betUI != null)
            {
                int index = betLists.Count - 1;
                betUI.SetData(bet, index);
            }
            betUIItems[bet.id] = newItem;
            Debug.Log($"Added single bet: {bet.id}");
        }
        else
        {
            int index = betLists.FindIndex(b => b.id == bet.id);
            betLists[index] = bet;
            if (betUIItems.TryGetValue(bet.id, out GameObject existingItem))
            {
                MyBetDetailsContailer betUI = existingItem.GetComponent<MyBetDetailsContailer>();
                if (betUI != null)
                {
                    betUI.SetData(bet, index);
                }
            }
            Debug.Log($"Updated existing bet: {bet.id}");
        }
        SortBetsByDate();
    }
    private void SortBetsByDate()
    {
        betLists = betLists.OrderByDescending(bet => bet.GetDateAndTime()).ToList();

        ReorderUIItems();
    }
    private async void ReorderUIItems()
    {
        for (int i = 0; i < betLists.Count; i++)
        {
            string betId = betLists[i].id;
            if (betUIItems.TryGetValue(betId, out GameObject item))
            {
                item.transform.SetSiblingIndex(i);
                // Update the visual appearance based on new index
                MyBetDetailsContailer betUI = item.GetComponent<MyBetDetailsContailer>();
                if (betUI != null)
                {
                    betUI.SetData(betLists[i], i);
                }
                await UniTask.Yield();
            }
        }
    }
    public void ShowBetHistory()
    {
        betHistoryGameobject.SetActive(true);
    }
    public void HideBetHistory()
    {
        betHistoryGameobject.SetActive(false);
    }
}

// Betlist now comes from IQPlay.GameClient (see BaseColyseusManager.cs) — the
// local copy that used to live here was a same-name-different-type shadowing
// it, which is why the delegate assignment above didn't match.