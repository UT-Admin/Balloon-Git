using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NumericKeyboardLayoutAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent of your game UI. Must be a direct child of the Canvas.")]
    [SerializeField] private RectTransform gameUI;
    [Tooltip("NumericKeyboard swaps this for the Android or iOS panel at startup.")]
    [SerializeField] private RectTransform keyboardUI;
    [SerializeField] private Canvas canvas;

    [Header("Animation")]
    [Tooltip("Screen pixels kept between the field and the top of the keyboard.")]
    [SerializeField] private float visibleMarginPixels = 24f;
    [SerializeField] private float animationDuration = 0.22f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Cameras")]
    [Tooltip("Also shift Camera.main with the UI. Ignored if the scene has none.")]
    [SerializeField] private bool shiftMainCamera = true;
    [SerializeField] private Camera[] extraCameras;

    private RectTransform coordinateSpace;
    private Vector2 gameRestPosition;
    private Vector2 keyboardShownPosition;
    private Coroutine routine;
    private bool ready;
    private bool restCaptured;
    private readonly Vector3[] corners = new Vector3[4];

    private readonly List<Camera> shiftedCameras = new List<Camera>();
    private bool cameraShiftActive;

    private Vector2 KeyboardHiddenPosition => keyboardShownPosition - new Vector2(0f, keyboardUI.rect.height);

    private bool Prepare()
    {
        if (ready) return true;

        if (gameUI == null || keyboardUI == null)
        {
            Debug.LogError("NumericKeyboardLayoutAnimator: assign GameUI and KeyboardUI.", this);
            return false;
        }

        if (canvas == null) canvas = gameUI.GetComponentInParent<Canvas>();
        if (canvas != null) canvas = canvas.rootCanvas;
        coordinateSpace = gameUI.parent as RectTransform;
        if (canvas == null || coordinateSpace == null)
        {
            Debug.LogError("NumericKeyboardLayoutAnimator: GameUI must sit directly under a Canvas.", this);
            return false;
        }

        if (!restCaptured)
        {
            gameRestPosition = gameUI.anchoredPosition;
            restCaptured = true;
        }
        keyboardShownPosition = keyboardUI.anchoredPosition;
        ready = true;
        return true;
    }

    public void SetKeyboardPanel(RectTransform panel)
    {
        if (panel == null) return;
        keyboardUI = panel;
        ready = false;
    }

    public void OpenForInput(RectTransform inputRect)
    {
        if (!Prepare() || inputRect == null) return;

        bool wasVisible = keyboardUI.gameObject.activeSelf;
        Vector2 keyboardStart = keyboardUI.anchoredPosition;
        keyboardUI.gameObject.SetActive(true);
        if (!wasVisible) keyboardStart = KeyboardHiddenPosition;

        // park the keyboard where it will end up so we can measure its top edge
        keyboardUI.anchoredPosition = keyboardShownPosition;
        Canvas.ForceUpdateCanvases();
        float shift = CalculateShift(inputRect);
        keyboardUI.anchoredPosition = keyboardStart;

        RefreshCameras();
        cameraShiftActive = true;
        Animate(gameRestPosition + new Vector2(0f, shift), keyboardShownPosition, false);
    }

    public void CloseKeyboard()
    {
        if (!Prepare() || !keyboardUI.gameObject.activeSelf) return;
        cameraShiftActive = true;
        Animate(gameRestPosition, KeyboardHiddenPosition, true);
    }

    private float CalculateShift(RectTransform inputRect)
    {
        inputRect.GetWorldCorners(corners);
        Vector3 inputBottomWorld = (corners[0] + corners[3]) * 0.5f;
        keyboardUI.GetWorldCorners(corners);
        Vector3 keyboardTopWorld = (corners[1] + corners[2]) * 0.5f;

        float inputBottomY = coordinateSpace.InverseTransformPoint(inputBottomWorld).y;
        float keyboardTopY = coordinateSpace.InverseTransformPoint(keyboardTopWorld).y;

        float currentOffset = gameUI.anchoredPosition.y - gameRestPosition.y;
        float restingInputBottomY = inputBottomY - currentOffset;

        float margin = visibleMarginPixels / Mathf.Max(PixelsPerCanvasUnit(), 0.0001f);
        return Mathf.Max(0f, keyboardTopY + margin - restingInputBottomY);
    }

    private void Animate(Vector2 gameTarget, Vector2 keyboardTarget, bool hideWhenDone)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AnimateRoutine(gameTarget, keyboardTarget, hideWhenDone));
    }

    private IEnumerator AnimateRoutine(Vector2 gameTarget, Vector2 keyboardTarget, bool hideWhenDone)
    {
        Vector2 gameStart = gameUI.anchoredPosition;
        Vector2 keyboardStart = keyboardUI.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = animationCurve.Evaluate(Mathf.Clamp01(elapsed / animationDuration));
            gameUI.anchoredPosition = Vector2.LerpUnclamped(gameStart, gameTarget, t);
            keyboardUI.anchoredPosition = Vector2.LerpUnclamped(keyboardStart, keyboardTarget, t);
            yield return null;
        }

        gameUI.anchoredPosition = gameTarget;
        keyboardUI.anchoredPosition = keyboardTarget;
        if (hideWhenDone) keyboardUI.gameObject.SetActive(false);
        routine = null;
    }

    private void LateUpdate()
    {
        if (!cameraShiftActive) return;
        if (!ready || gameUI == null) { cameraShiftActive = false; return; }

        float pixels = (gameUI.anchoredPosition.y - gameRestPosition.y) * PixelsPerCanvasUnit();
        for (int i = shiftedCameras.Count - 1; i >= 0; i--)
        {
            Camera cam = shiftedCameras[i];
            if (cam == null) { shiftedCameras.RemoveAt(i); continue; }
            ShiftCamera(cam, pixels);
        }

        if (routine == null && Mathf.Abs(pixels) < 0.01f) cameraShiftActive = false;
    }

    // shifts the projection instead of the transform, so 2D and 3D cameras move exactly as far as the UI
    private static void ShiftCamera(Camera cam, float pixels)
    {
        cam.ResetProjectionMatrix();
        if (Mathf.Abs(pixels) < 0.01f) return;

        float ndc = 2f * pixels / Mathf.Max(1f, cam.pixelHeight);
        Matrix4x4 m = cam.projectionMatrix;
        if (cam.orthographic) m.m13 += ndc;
        else m.m12 -= ndc;
        cam.projectionMatrix = m;
    }

    private void RefreshCameras()
    {
        Camera uiCamera = GetUiCamera();
        var wanted = new List<Camera>();
        if (shiftMainCamera) AddCamera(wanted, Camera.main, uiCamera);
        if (extraCameras != null)
            foreach (Camera cam in extraCameras) AddCamera(wanted, cam, uiCamera);

        foreach (Camera old in shiftedCameras)
            if (old != null && !wanted.Contains(old)) old.ResetProjectionMatrix();

        shiftedCameras.Clear();
        shiftedCameras.AddRange(wanted);
    }

    private static void AddCamera(List<Camera> list, Camera cam, Camera uiCamera)
    {
        // the camera that draws the canvas is skipped, otherwise the UI would move twice
        if (cam == null || cam == uiCamera || list.Contains(cam)) return;
        list.Add(cam);
    }

    private Camera GetUiCamera()
    {
        switch (canvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay: return null;
            case RenderMode.ScreenSpaceCamera: return canvas.worldCamera;
            default: return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }
    }

    private void ResetCameras()
    {
        foreach (Camera cam in shiftedCameras)
            if (cam != null) cam.ResetProjectionMatrix();
        cameraShiftActive = false;
    }

    private void OnDisable() => ResetCameras();

    private float PixelsPerCanvasUnit()
    {
        if (canvas.renderMode != RenderMode.WorldSpace) return canvas.scaleFactor;

        Camera cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        if (cam == null || gameUI.rect.height <= 0f) return 1f;
        gameUI.GetWorldCorners(corners);
        float pixels = Mathf.Abs(cam.WorldToScreenPoint(corners[1]).y - cam.WorldToScreenPoint(corners[0]).y);
        return pixels / gameUI.rect.height;
    }
}
