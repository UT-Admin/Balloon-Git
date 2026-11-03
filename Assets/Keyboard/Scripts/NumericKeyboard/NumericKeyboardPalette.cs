using UnityEngine;

public enum NumericKeyboardPlatform { Android, IOS }

public enum NumericKeyboardRole
{
    PanelBackground,
    Card, CardText, CardHint,
    Error,
    DigitKey, FunctionKey, ActionKey,
    KeyText, ActionKeyText,
    ToolbarBackground, ToolbarText,
    ToolbarButton, ToolbarDivider
}

public struct NumericKeyboardPalette
{
    public Color panel, card, cardHint, toolbar, divider;
    public Color key, keyPressed;
    public Color functionKey, functionKeyPressed;
    public Color action, actionPressed;
    public Color text, actionText, error;

    public Color Normal(NumericKeyboardRole role)
    {
        switch (role)
        {
            case NumericKeyboardRole.PanelBackground: return panel;
            case NumericKeyboardRole.Card: return card;
            case NumericKeyboardRole.CardHint: return cardHint;
            case NumericKeyboardRole.Error: return error;
            case NumericKeyboardRole.DigitKey: return key;
            case NumericKeyboardRole.FunctionKey: return functionKey;
            case NumericKeyboardRole.ActionKey:
            case NumericKeyboardRole.ToolbarButton: return action;
            case NumericKeyboardRole.ActionKeyText: return actionText;
            case NumericKeyboardRole.ToolbarBackground: return toolbar;
            case NumericKeyboardRole.ToolbarDivider: return divider;
            default: return text;
        }
    }

    public Color Pressed(NumericKeyboardRole role)
    {
        switch (role)
        {
            case NumericKeyboardRole.DigitKey: return keyPressed;
            case NumericKeyboardRole.FunctionKey: return functionKeyPressed;
            case NumericKeyboardRole.ActionKey:
            case NumericKeyboardRole.ToolbarButton: return actionPressed;
            default: return Normal(role);
        }
    }

    public static NumericKeyboardPalette Get(NumericKeyboardPlatform platform, bool dark)
    {
        return platform == NumericKeyboardPlatform.IOS ? (dark ? IosDark() : IosLight())
                                                       : (dark ? AndroidDark() : AndroidLight());
    }

    private static NumericKeyboardPalette AndroidDark() => new NumericKeyboardPalette
    {
        panel = Hex("#202124"), card = Hex("#303134"), cardHint = Hex("#9AA0A6"),
        toolbar = Hex("#202124"), divider = Hex("#202124"),
        key = Hex("#3C4043"), keyPressed = Hex("#5F6368"),
        functionKey = Hex("#2D2F31"), functionKeyPressed = Hex("#4A4D51"),
        action = Hex("#8AB4F8"), actionPressed = Hex("#AECBFA"),
        text = Hex("#E8EAED"), actionText = Hex("#202124"), error = Hex("#F28B82")
    };

    private static NumericKeyboardPalette AndroidLight() => new NumericKeyboardPalette
    {
        panel = Hex("#E8EAED"), card = Hex("#FFFFFF"), cardHint = Hex("#5F6368"),
        toolbar = Hex("#E8EAED"), divider = Hex("#E8EAED"),
        key = Hex("#FFFFFF"), keyPressed = Hex("#DADCE0"),
        functionKey = Hex("#DADCE0"), functionKeyPressed = Hex("#BDC1C6"),
        action = Hex("#1A73E8"), actionPressed = Hex("#185ABC"),
        text = Hex("#202124"), actionText = Hex("#FFFFFF"), error = Hex("#D93025")
    };

    // on iOS a pressed digit key turns the function-key colour and the other way round
    private static NumericKeyboardPalette IosDark() => new NumericKeyboardPalette
    {
        panel = Hex("#1C1C1E"), card = Hex("#2C2C2E"), cardHint = Hex("#8E8E93"),
        toolbar = Hex("#2C2C2E"), divider = Hex("#38383A"),
        key = Hex("#636366"), keyPressed = Hex("#48484A"),
        functionKey = Hex("#48484A"), functionKeyPressed = Hex("#636366"),
        action = Hex("#0A84FF"), actionPressed = Hex("#409CFF"),
        text = Hex("#FFFFFF"), actionText = Hex("#FFFFFF"), error = Hex("#FF453A")
    };

    private static NumericKeyboardPalette IosLight() => new NumericKeyboardPalette
    {
        panel = Hex("#D1D3D9"), card = Hex("#FFFFFF"), cardHint = Hex("#8E8E93"),
        toolbar = Hex("#F2F2F7"), divider = Hex("#C6C6C8"),
        key = Hex("#FFFFFF"), keyPressed = Hex("#ABB0BA"),
        functionKey = Hex("#ABB0BA"), functionKeyPressed = Hex("#FFFFFF"),
        action = Hex("#007AFF"), actionPressed = Hex("#0A5FD0"),
        text = Hex("#000000"), actionText = Hex("#FFFFFF"), error = Hex("#FF3B30")
    };

    private static Color Hex(string html)
    {
        ColorUtility.TryParseHtmlString(html, out Color color);
        return color;
    }
}
