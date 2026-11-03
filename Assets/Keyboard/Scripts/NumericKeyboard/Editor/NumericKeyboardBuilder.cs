using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NumericKeyboardBuilder : EditorWindow
{
    private enum PreviewTheme { Dark, Light }

    private PreviewTheme previewTheme = PreviewTheme.Dark;
    private float keyHeight = 150f;
    private float cardHeight = 190f;
    private float iosKeyHeight = 128f;
    private float iosToolbarHeight = 100f;
    private float keyGap = 10f;

    private const string GeneratedFolder = "Assets/NumericKeyboard/Generated";
    private const string AndroidPanelName = "KeyboardUI_Android";
    private const string IosPanelName = "KeyboardUI_iOS";
    private const string LegacyPanelName = "KeyboardUI";

    private const float SidePadding = 12f;
    private const float TopPadding = 12f;
    private const float CardGap = 10f;
    private const float BottomPadding = 20f;

    private const float IosSidePadding = 8f;
    private const float IosTopPadding = 10f;
    private const float IosBottomPadding = 40f;
    private const float IosCornerScale = 2.4f;   // higher = tighter key corners

    [MenuItem("Tools/Numeric Keyboard/Build Keyboards (Android + iOS)")]
    private static void Open() => GetWindow<NumericKeyboardBuilder>("Numeric Keyboard");

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Builds the Android and iOS keyboards in the open scene. Sizes are in Canvas units.",
            MessageType.Info);

        previewTheme = (PreviewTheme)EditorGUILayout.EnumPopup("Colours shown in the Editor", previewTheme);
        keyGap = EditorGUILayout.Slider("Gap between keys", keyGap, 4f, 20f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
        keyHeight = EditorGUILayout.Slider("Key height", keyHeight, 80f, 200f);
        cardHeight = EditorGUILayout.Slider("Visualizer card height", cardHeight, 120f, 280f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("iOS", EditorStyles.boldLabel);
        iosKeyHeight = EditorGUILayout.Slider("Key height", iosKeyHeight, 80f, 200f);
        iosToolbarHeight = EditorGUILayout.Slider("Toolbar height", iosToolbarHeight, 70f, 140f);

        EditorGUILayout.Space();
        if (GUILayout.Button("Build keyboards in open scene", GUILayout.Height(34))) Build();
    }

    private void Build()
    {
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog("TextMeshPro needs setting up",
                "Import the essentials first:\nWindow > TextMeshPro > Import TMP Essential Resources\n\nThen run this again.", "OK");
            return;
        }

        Canvas canvas = FindOrCreateCanvas();
        EnsureEventSystem();

        string[] oldNames = { LegacyPanelName, AndroidPanelName, IosPanelName, "OutsideClickOverlay" };
        bool anyOld = false;
        foreach (string n in oldNames) if (canvas.transform.Find(n) != null) anyOld = true;
        if (anyOld)
        {
            if (!EditorUtility.DisplayDialog("Replace existing keyboard?",
                    "A keyboard already exists in this Canvas. Rebuild it?", "Replace", "Cancel")) return;
            foreach (string n in oldNames)
            {
                Transform old = canvas.transform.Find(n);
                if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            }
        }

        Sprite keySprite = GetOrCreateKeySprite();
        Sprite backspaceIcon = GetOrCreateIcon("IconBackspace.png", new[]
        {
            new Vector4(0.08f, 0.50f, 0.30f, 0.78f), new Vector4(0.30f, 0.78f, 0.92f, 0.78f),
            new Vector4(0.92f, 0.78f, 0.92f, 0.22f), new Vector4(0.92f, 0.22f, 0.30f, 0.22f),
            new Vector4(0.30f, 0.22f, 0.08f, 0.50f),
            new Vector4(0.50f, 0.38f, 0.74f, 0.62f), new Vector4(0.50f, 0.62f, 0.74f, 0.38f)
        }, 9f);
        Sprite checkIcon = GetOrCreateIcon("IconCheck.png", new[]
        {
            new Vector4(0.16f, 0.52f, 0.38f, 0.30f), new Vector4(0.38f, 0.30f, 0.86f, 0.76f)
        }, 13f);

        RectTransform gameUI = canvas.transform.Find("GameUI") as RectTransform;
        if (gameUI == null)
        {
            gameUI = NewRect("GameUI", canvas.transform);
            Stretch(gameUI);
            gameUI.SetAsFirstSibling();
            Undo.RegisterCreatedObjectUndo(gameUI.gameObject, "Create GameUI");
        }

        NumericKeyboard keyboard = FindOne<NumericKeyboard>();
        GameObject manager;
        if (keyboard == null)
        {
            manager = new GameObject("NumericKeyboardManager");
            Undo.RegisterCreatedObjectUndo(manager, "Create NumericKeyboardManager");
            keyboard = manager.AddComponent<NumericKeyboard>();
        }
        else manager = keyboard.gameObject;
        NumericKeyboardLayoutAnimator animator = GetOrAdd<NumericKeyboardLayoutAnimator>(manager);
        NumericInputAutoBinder binder = GetOrAdd<NumericInputAutoBinder>(manager);

        RectTransform overlay = NewRect("OutsideClickOverlay", canvas.transform);
        Stretch(overlay);
        Image overlayImage = overlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0f);
        Button overlayButton = overlay.gameObject.AddComponent<Button>();
        overlayButton.targetGraphic = overlayImage;
        overlayButton.transition = Selectable.Transition.None;
        overlayButton.navigation = new Navigation { mode = Navigation.Mode.None };
        overlay.gameObject.SetActive(false);
        Undo.RegisterCreatedObjectUndo(overlay.gameObject, "Create OutsideClickOverlay");

        NumericKeyboardPanel androidPanel = BuildAndroidPanel(canvas, keyboard, keySprite, backspaceIcon, checkIcon);
        NumericKeyboardPanel iosPanel = BuildIosPanel(canvas, keyboard, keySprite, backspaceIcon);

        SetReference(keyboard, "androidPanel", androidPanel);
        SetReference(keyboard, "iosPanel", iosPanel);
        SetReference(keyboard, "outsideClickOverlay", overlay.gameObject);
        SetReference(keyboard, "layoutAnimator", animator);
        SetReference(animator, "gameUI", gameUI);
        SetReference(animator, "keyboardUI", androidPanel.transform);
        SetReference(animator, "canvas", canvas);
        SetReference(binder, "gameUI", gameUI);
        SetReference(binder, "numericKeyboard", keyboard);

        overlay.SetAsLastSibling();
        androidPanel.transform.SetAsLastSibling();
        iosPanel.transform.SetAsLastSibling();

        // only one panel is visible in the editor; the runtime picks the right one
        iosPanel.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = androidPanel.gameObject;

        EditorUtility.DisplayDialog("Keyboards built",
            "Move your game UI into the GameUI object and set your number fields to Integer or Decimal Number.\n\n" +
            "To try the iOS panel in Play mode, set Platform Choice to IOS on NumericKeyboardManager.", "OK");
    }

    private NumericKeyboardPanel BuildAndroidPanel(Canvas canvas, NumericKeyboard keyboard,
        Sprite keySprite, Sprite backspaceIcon, Sprite checkIcon)
    {
        float totalHeight = TopPadding + cardHeight + CardGap + keyHeight * 4f + BottomPadding;
        RectTransform panel = NewRect(AndroidPanelName, canvas.transform);
        DockToBottom(panel, totalHeight);
        panel.gameObject.AddComponent<Image>();
        Themed(panel.gameObject, NumericKeyboardRole.PanelBackground);
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "Create " + AndroidPanelName);

        RectTransform card = NewRect("VisualizerCard", panel);
        card.anchorMin = new Vector2(0f, 1f);
        card.anchorMax = new Vector2(1f, 1f);
        card.pivot = new Vector2(0.5f, 1f);
        card.sizeDelta = new Vector2(-2f * SidePadding, cardHeight);
        card.anchoredPosition = new Vector2(0f, -TopPadding);
        Image cardImage = card.gameObject.AddComponent<Image>();
        cardImage.sprite = keySprite;
        cardImage.type = Image.Type.Sliced;
        cardImage.raycastTarget = false;
        Themed(card.gameObject, NumericKeyboardRole.Card);

        TextMeshProUGUI value = NewText("ValueText", card, "0", 76f, Color.white);
        SetAnchors(value.rectTransform, 0f, 0.32f, 1f, 1f);
        value.enableAutoSizing = true;
        value.fontSizeMin = 36f;
        value.fontSizeMax = 76f;
        Themed(value.gameObject, NumericKeyboardRole.CardText);

        TextMeshProUGUI hint = NewText("HintText", card, "", 30f, Color.white);
        SetAnchors(hint.rectTransform, 0f, 0.04f, 1f, 0.32f);
        Themed(hint.gameObject, NumericKeyboardRole.CardHint);

        TextMeshProUGUI error = NewText("ErrorText", card, "", 30f, Color.white);
        SetAnchors(error.rectTransform, 0f, 0.04f, 1f, 0.32f);
        Themed(error.gameObject, NumericKeyboardRole.Error);
        error.gameObject.SetActive(false);

        RectTransform keys = NewRect("Keys", panel);
        keys.anchorMin = Vector2.zero;
        keys.anchorMax = Vector2.one;
        keys.offsetMin = new Vector2(SidePadding, BottomPadding);
        keys.offsetMax = new Vector2(-SidePadding, -(TopPadding + cardHeight + CardGap));

        // 1 2 3 backspace / 4 5 6 backspace / 7 8 9 done / . 0 C done
        const int cols = 4, rows = 4;
        for (int d = 1; d <= 9; d++)
        {
            Button key = MakeKey("Key" + d, keys, cols, rows, (d - 1) % 3, (d - 1) / 3, 1, 1,
                NumericKeyboardRole.DigitKey, keySprite, 1f);
            AddLabel(key, d.ToString(), 64f, NumericKeyboardRole.KeyText);
            UnityEventTools.AddStringPersistentListener(key.onClick, keyboard.PressDigit, d.ToString());
        }

        Button zero = MakeKey("Key0", keys, cols, rows, 1, 3, 1, 1, NumericKeyboardRole.DigitKey, keySprite, 1f);
        AddLabel(zero, "0", 64f, NumericKeyboardRole.KeyText);
        UnityEventTools.AddStringPersistentListener(zero.onClick, keyboard.PressDigit, "0");

        Button decimalKey = MakeKey("KeyDecimal", keys, cols, rows, 0, 3, 1, 1, NumericKeyboardRole.FunctionKey, keySprite, 1f);
        AddLabel(decimalKey, ".", 72f, NumericKeyboardRole.KeyText);
        decimalKey.gameObject.AddComponent<CanvasGroup>();
        UnityEventTools.AddPersistentListener(decimalKey.onClick, keyboard.PressDecimal);

        Button clearKey = MakeKey("KeyClear", keys, cols, rows, 2, 3, 1, 1, NumericKeyboardRole.FunctionKey, keySprite, 1f);
        AddLabel(clearKey, "C", 52f, NumericKeyboardRole.KeyText);
        UnityEventTools.AddPersistentListener(clearKey.onClick, keyboard.Clear);

        Button backspaceKey = MakeKey("KeyBackspace", keys, cols, rows, 3, 0, 1, 2, NumericKeyboardRole.FunctionKey, keySprite, 1f);
        AddIcon(backspaceKey, backspaceIcon, NumericKeyboardRole.KeyText, keyHeight * 0.45f);
        UnityEventTools.AddPersistentListener(backspaceKey.onClick, keyboard.Backspace);

        Button doneKey = MakeKey("KeyDone", keys, cols, rows, 3, 2, 1, 2, NumericKeyboardRole.ActionKey, keySprite, 1f);
        AddIcon(doneKey, checkIcon, NumericKeyboardRole.ActionKeyText, keyHeight * 0.45f);
        UnityEventTools.AddPersistentListener(doneKey.onClick, keyboard.Done);

        NumericKeyboardPanel component = panel.gameObject.AddComponent<NumericKeyboardPanel>();
        SetEnum(component, "platform", (int)NumericKeyboardPlatform.Android);
        SetReference(component, "valueText", value);
        SetReference(component, "hintText", hint);
        SetReference(component, "errorText", error);
        SetReference(component, "errorReplaces", hint);
        SetReference(component, "decimalButton", decimalKey);
        component.ApplyTheme(previewTheme == PreviewTheme.Dark);
        return component;
    }

    private NumericKeyboardPanel BuildIosPanel(Canvas canvas, NumericKeyboard keyboard,
        Sprite keySprite, Sprite backspaceIcon)
    {
        float totalHeight = iosToolbarHeight + IosTopPadding + iosKeyHeight * 4f + IosBottomPadding;
        RectTransform panel = NewRect(IosPanelName, canvas.transform);
        DockToBottom(panel, totalHeight);
        panel.gameObject.AddComponent<Image>();
        Themed(panel.gameObject, NumericKeyboardRole.PanelBackground);
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "Create " + IosPanelName);

        RectTransform toolbar = NewRect("Toolbar", panel);
        toolbar.anchorMin = new Vector2(0f, 1f);
        toolbar.anchorMax = new Vector2(1f, 1f);
        toolbar.pivot = new Vector2(0.5f, 1f);
        toolbar.sizeDelta = new Vector2(0f, iosToolbarHeight);
        toolbar.anchoredPosition = Vector2.zero;
        toolbar.gameObject.AddComponent<Image>();
        Themed(toolbar.gameObject, NumericKeyboardRole.ToolbarBackground);

        RectTransform divider = NewRect("Divider", toolbar);
        divider.anchorMin = new Vector2(0f, 0f);
        divider.anchorMax = new Vector2(1f, 0f);
        divider.pivot = new Vector2(0.5f, 0f);
        divider.sizeDelta = new Vector2(0f, 2f);
        divider.anchoredPosition = Vector2.zero;
        Image dividerImage = divider.gameObject.AddComponent<Image>();
        dividerImage.raycastTarget = false;
        Themed(divider.gameObject, NumericKeyboardRole.ToolbarDivider);

        TextMeshProUGUI value = NewText("ValueText", toolbar, "0", 54f, Color.white);
        SetAnchors(value.rectTransform, 0.30f, 0f, 0.70f, 1f);
        value.fontStyle = FontStyles.Bold;
        value.enableAutoSizing = true;
        value.fontSizeMin = 28f;
        value.fontSizeMax = 54f;
        Themed(value.gameObject, NumericKeyboardRole.ToolbarText);

        TextMeshProUGUI error = NewText("ErrorText", toolbar, "", 28f, Color.white);
        SetAnchors(error.rectTransform, 0.30f, 0f, 0.70f, 1f);
        error.enableAutoSizing = true;
        error.fontSizeMin = 18f;
        error.fontSizeMax = 28f;
        Themed(error.gameObject, NumericKeyboardRole.Error);
        error.gameObject.SetActive(false);

        Button cancel = MakeToolbarButton("CancelButton", toolbar, "Cancel", true, false);
        UnityEventTools.AddPersistentListener(cancel.onClick, keyboard.CloseKeyboard);
        Button done = MakeToolbarButton("DoneButton", toolbar, "Done", false, true);
        UnityEventTools.AddPersistentListener(done.onClick, keyboard.Done);

        RectTransform keys = NewRect("Keys", panel);
        keys.anchorMin = Vector2.zero;
        keys.anchorMax = Vector2.one;
        keys.offsetMin = new Vector2(IosSidePadding, IosBottomPadding);
        keys.offsetMax = new Vector2(-IosSidePadding, -(iosToolbarHeight + IosTopPadding));

        // 1 2 3 / 4 5 6 / 7 8 9 / . 0 backspace
        const int cols = 3, rows = 4;
        for (int d = 1; d <= 9; d++)
        {
            Button key = MakeKey("Key" + d, keys, cols, rows, (d - 1) % 3, (d - 1) / 3, 1, 1,
                NumericKeyboardRole.DigitKey, keySprite, IosCornerScale);
            AddLabel(key, d.ToString(), 66f, NumericKeyboardRole.KeyText);
            UnityEventTools.AddStringPersistentListener(key.onClick, keyboard.PressDigit, d.ToString());
        }

        Button zero = MakeKey("Key0", keys, cols, rows, 1, 3, 1, 1, NumericKeyboardRole.DigitKey, keySprite, IosCornerScale);
        AddLabel(zero, "0", 66f, NumericKeyboardRole.KeyText);
        UnityEventTools.AddStringPersistentListener(zero.onClick, keyboard.PressDigit, "0");

        Button decimalKey = MakeKey("KeyDecimal", keys, cols, rows, 0, 3, 1, 1, NumericKeyboardRole.FunctionKey, keySprite, IosCornerScale);
        AddLabel(decimalKey, ".", 72f, NumericKeyboardRole.KeyText);
        decimalKey.gameObject.AddComponent<CanvasGroup>();
        UnityEventTools.AddPersistentListener(decimalKey.onClick, keyboard.PressDecimal);

        Button backspaceKey = MakeKey("KeyBackspace", keys, cols, rows, 2, 3, 1, 1, NumericKeyboardRole.FunctionKey, keySprite, IosCornerScale);
        AddIcon(backspaceKey, backspaceIcon, NumericKeyboardRole.KeyText, iosKeyHeight * 0.42f);
        UnityEventTools.AddPersistentListener(backspaceKey.onClick, keyboard.Backspace);

        NumericKeyboardPanel component = panel.gameObject.AddComponent<NumericKeyboardPanel>();
        SetEnum(component, "platform", (int)NumericKeyboardPlatform.IOS);
        SetReference(component, "valueText", value);
        SetReference(component, "errorText", error);
        SetReference(component, "errorReplaces", value);
        SetReference(component, "decimalButton", decimalKey);
        component.ApplyTheme(previewTheme == PreviewTheme.Dark);
        return component;
    }

    private Button MakeKey(string name, RectTransform parent, int columns, int rows, int column, int row,
        int columnSpan, int rowSpan, NumericKeyboardRole role, Sprite background, float cornerScale)
    {
        RectTransform rect = NewRect(name, parent);
        rect.anchorMin = new Vector2(column / (float)columns, 1f - (row + rowSpan) / (float)rows);
        rect.anchorMax = new Vector2((column + columnSpan) / (float)columns, 1f - row / (float)rows);
        float half = keyGap * 0.5f;
        rect.offsetMin = new Vector2(half, half);
        rect.offsetMax = new Vector2(-half, -half);

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = background;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = cornerScale;
        image.color = Color.white;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        Themed(rect.gameObject, role);
        return button;
    }

    private static Button MakeToolbarButton(string name, RectTransform toolbar, string text, bool left, bool bold)
    {
        RectTransform rect = NewRect(name, toolbar);
        SetAnchors(rect, left ? 0f : 0.70f, 0f, left ? 0.30f : 1f, 1f);

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 40f;
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        label.color = Color.white;
        label.alignment = left ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
        label.margin = new Vector4(left ? 32f : 0f, 0f, left ? 0f : 32f, 0f);
        label.raycastTarget = true;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        Themed(rect.gameObject, NumericKeyboardRole.ToolbarButton);
        return button;
    }

    private static void AddLabel(Button key, string text, float size, NumericKeyboardRole role)
    {
        TextMeshProUGUI label = NewText("Label", key.transform, text, size, Color.white);
        Stretch(label.rectTransform);
        Themed(label.gameObject, role);
    }

    private static void AddIcon(Button key, Sprite sprite, NumericKeyboardRole role, float size)
    {
        RectTransform rect = NewRect("Icon", key.transform);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = Vector2.zero;

        Image icon = rect.gameObject.AddComponent<Image>();
        icon.sprite = sprite;
        icon.color = Color.white;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        Themed(rect.gameObject, role);
    }

    private static void Themed(GameObject go, NumericKeyboardRole role)
    {
        go.AddComponent<NumericKeyboardThemed>().Role = role;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color)
    {
        RectTransform rect = NewRect(name, parent);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rect, float minX, float minY, float maxX, float maxY)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void DockToBottom(RectTransform panel, float height)
    {
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.sizeDelta = new Vector2(0f, height);
        panel.anchoredPosition = Vector2.zero;
    }

    private static void SetReference(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null)
        {
            Debug.LogWarning($"Numeric Keyboard builder: couldn't find field '{field}' on {target.GetType().Name}.");
            return;
        }
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(Object target, string field, int index)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);
        if (property == null)
        {
            Debug.LogWarning($"Numeric Keyboard builder: couldn't find field '{field}' on {target.GetType().Name}.");
            return;
        }
        property.enumValueIndex = index;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static Canvas FindOrCreateCanvas()
    {
        foreach (Canvas existing in FindAll<Canvas>())
            if (existing.isRootCanvas) return existing;

        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = LayerMask.NameToLayer("UI");
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (FindOne<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem", typeof(EventSystem));
        Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    private static T[] FindAll<T>() where T : UnityEngine.Object
    {
#if UNITY_2022_2_OR_NEWER
        return UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return UnityEngine.Object.FindObjectsOfType<T>();
#endif
    }

    private static T FindOne<T>() where T : UnityEngine.Object
    {
        T[] all = FindAll<T>();
        return all.Length > 0 ? all[0] : null;
    }

    private static Sprite GetOrCreateKeySprite()
    {
        const int size = 96;
        const float radius = 30f;
        string path = GeneratedFolder + "/KeyRounded.png";

        if (!File.Exists(path))
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            float half = size * 0.5f;
            float inner = half - radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x + 0.5f - half) - inner;
                    float py = Mathf.Abs(y + 0.5f - half) - inner;
                    float outside = new Vector2(Mathf.Max(px, 0f), Mathf.Max(py, 0f)).magnitude;
                    float inside = Mathf.Min(Mathf.Max(px, py), 0f);
                    float distance = outside + inside - radius;
                    byte alpha = (byte)(Mathf.Clamp01(0.5f - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            WritePng(texture, path);
        }
        return ImportAsSprite(path, new Vector4(radius + 2f, radius + 2f, radius + 2f, radius + 2f));
    }

    private static Sprite GetOrCreateIcon(string fileName, Vector4[] segments, float thickness)
    {
        const int size = 128;
        string path = GeneratedFolder + "/" + fileName;

        if (!File.Exists(path))
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    float nearest = float.MaxValue;
                    foreach (Vector4 s in segments)
                    {
                        float d = DistanceToSegment(point, new Vector2(s.x, s.y) * size, new Vector2(s.z, s.w) * size);
                        if (d < nearest) nearest = d;
                    }
                    byte alpha = (byte)(Mathf.Clamp01(thickness * 0.5f - nearest + 0.5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            WritePng(texture, path);
        }
        return ImportAsSprite(path, Vector4.zero);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f));
        return Vector2.Distance(point, a + ab * t);
    }

    private static void WritePng(Texture2D texture, string path)
    {
        Directory.CreateDirectory(GeneratedFolder);
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.Refresh();
    }

    private static Sprite ImportAsSprite(string path, Vector4 border)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
