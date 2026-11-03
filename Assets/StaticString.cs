using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
public static class StaticString
{
    public const string defaultGameName = "balloon";
    public const string Rpc_GetTime = "rpc_GetTime";
    public const string Rpc_FindMatchTemplet = "Rpc_JoinFunGame";
    public const string Rpc_SetLoginData = "rpc_SetLoginData";
    public const string JoinOrCreateMatchRpc = "JoinOrCreateMatchRpc";
    public const string CURRENCYNOTATION = "USD";

    public static string SetCurrencyNotation(this float val, string CurrencyNotation = "USD", float currencyTextSize = 0)
    {
        if (currencyTextSize > 0)
        {
            return val % 1 == 0 ? $"{val.FloorValue()}<size={currencyTextSize}> {CurrencyNotation}" : $"{val.FloorValue()}<size={currencyTextSize}> {CurrencyNotation}";
        }

        return val % 1 == 0 ? $"{val.FloorValue()} {CurrencyNotation}" : $"{val.FloorValue()} {CurrencyNotation}";
    }

    public static string SetCurrencyNotation<T>(this float val, string CurrencyNotation)
    {
        return val % 1 == 0 ? $"{val.FloorValue()} {CurrencyNotation}" : $"{val.FloorValue()} {CurrencyNotation}";
    }

    public static string SetCurrencyNotation(this double val, string CurrencyNotation = "USD", float currencyTextSize = 0)
    {
        string formattedValue = FloorValue(val);

        // Add currency notation with optional text size
        if (currencyTextSize > 0)
        {
            return $"{formattedValue}<size={currencyTextSize}> {CurrencyNotation}</size>";
        }

        return $"{formattedValue} {CurrencyNotation}";
    }

    public static string SetCurrencyNotation<T>(this double val, string CurrencyNotation)
    {
        return val % 1 == 0 ? $"{FloorValue(val)} {CurrencyNotation}" : $"{FloorValue(val)} {CurrencyNotation}";
    }

    // public static string FloorValue(this double val)
    // {
    //     return ((Decimal.Floor((decimal)val * 100) / 100)).ToString("N2");
    // }

    public static string FloorValue(this float val)
    {
        return ((Decimal.Floor((decimal)val * 100) / 100)).ToString("N2");
    }

    public static double FloorDecimalValue(this double val)
    {
        return (double)(Decimal.Floor((decimal)val * 100) / 100);
    }
    public static float FloorDecimalValue(this float val)
    {
        return (float)(Decimal.Floor((decimal)val * 100) / 100);
    }

    public static string FloorValue(double val, CurrencySymbol currency = CurrencySymbol.None)
    {
        decimal floored = Decimal.Floor((decimal)val * 100) / 100;

        NumberFormatInfo format = APIController.instance.authentication.currency_type.ToLower() == "inr" ? IndianFormat : InternationalFormat;

        string symbol = currency switch
        {
            CurrencySymbol.USD => "$",
            CurrencySymbol.INR => "₹",
            _ => ""
        };

        string number = Math.Abs(floored).ToString("N2", format);
        string sign = floored < 0 ? "-" : "";

        return sign + symbol + number;
    }

    public static readonly NumberFormatInfo InternationalFormat = new NumberFormatInfo
    {
        NumberDecimalSeparator = ".",
        NumberGroupSeparator = ",",
        NumberGroupSizes = new[] { 3 }
    };

    public static readonly NumberFormatInfo IndianFormat = new NumberFormatInfo
    {
        NumberDecimalSeparator = ".",
        NumberGroupSeparator = ",",
        NumberGroupSizes = new[] { 3, 2 }
    };

    public enum NumberStyle
    {
        International,
        Indian
    }

    public enum CurrencySymbol
    {
        None,
        USD,
        INR
    }
}
