// Platform and dark mode detection for the numeric keyboard.
mergeInto(LibraryManager.library, {
  // 0 unknown / desktop, 1 Android, 2 iOS
  NumericKeyboardDetectPlatform: function () {
    if (typeof navigator === "undefined") return 0;
    var ua = navigator.userAgent || "";
    // iPadOS reports itself as a Mac, but a real Mac has no touch screen
    var touchMac = navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1;
    if (/iPhone|iPad|iPod/i.test(ua) || touchMac) return 2;
    if (/Android/i.test(ua)) return 1;
    return 0;
  },

  // 1 dark, 0 light, -1 unknown
  NumericKeyboardIsDarkMode: function () {
    if (typeof window === "undefined" || !window.matchMedia) return -1;
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? 1 : 0;
  },

  NumericKeyboardWatchTheme: function (objectNamePtr) {
    if (typeof window === "undefined" || !window.matchMedia) return;

    var state = window.__numericKeyboardTheme;
    if (!state) state = window.__numericKeyboardTheme = { objectName: "", listening: false };
    state.objectName = UTF8ToString(objectNamePtr);
    if (state.listening) return;
    state.listening = true;

    var query = window.matchMedia("(prefers-color-scheme: dark)");
    var onChange = function (event) {
      if (!state.objectName) return;
      var value = event.matches ? "dark" : "light";
      if (typeof Module !== "undefined" && Module.SendMessage) {
        Module.SendMessage(state.objectName, "OnSystemThemeChanged", value);
      } else if (window.unityInstance && window.unityInstance.SendMessage) {
        window.unityInstance.SendMessage(state.objectName, "OnSystemThemeChanged", value);
      }
    };
    if (query.addEventListener) query.addEventListener("change", onChange);
    else if (query.addListener) query.addListener(onChange);
  },

  NumericKeyboardUnwatchTheme: function () {
    if (window.__numericKeyboardTheme) window.__numericKeyboardTheme.objectName = "";
  },
  Updatekeyboardstatus: function (status) {
    console.log("status Received in dll - ", status);
    UpdateKeyboardStatus(status);
  },
});
