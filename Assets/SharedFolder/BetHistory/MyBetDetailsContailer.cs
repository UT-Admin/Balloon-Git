using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IQPlay.GameClient;

public class MyBetDetailsContailer : MonoBehaviour
{
    [SerializeField] private Image backgroundImage; // Reference to the Image component
    [SerializeField] private Sprite oddSprite;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text betAmount;
    [SerializeField] private TMP_Text cashoutAmount;
    [SerializeField] private TMP_Text multiplierText;

    [SerializeField] private GameObject multiplierContainer;
    public void SetData(Betlist item, int index)
    {
        dateText.text = item.GetDateAndTimeString();
        betAmount.text = FloorValue(item.bet_amount);
        cashoutAmount.text = item.win_amount > 0
            ? FloorValue(item.win_amount)
            : "--";
        double mul = item.win_amount / item.bet_amount;
        multiplierContainer.SetActive(mul > 0);
        multiplierText.text = "x" + mul.ToString("F2");

        // Apply image based on odd/even index
        if (backgroundImage != null)
        {
            bool isOdd = index % 2 != 0;
            if (isOdd) // Odd index (1, 3, 5, ...)
            {
                backgroundImage.enabled = false;
                Debug.Log($"Bet {item.id} at index {index}: EVEN - Image DISABLED");
            }
            else // Even index (0, 2, 4, ...)
            {
                backgroundImage.sprite = oddSprite;
                backgroundImage.enabled = true;
                Debug.Log($"Bet {item.id} at index {index}: ODD - Image ENABLED");
            }
        }
        else
        {
            Debug.LogWarning("backgroundImage is null!");
        }

        gameObject.SetActive(true);
    }

    string FloorValue(double val)
    {
        return (Decimal.Floor((decimal)val * 100) / 100).ToString("N2");
    }
}
