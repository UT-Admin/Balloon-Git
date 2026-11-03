// Lets the browser back button close the keyboard instead of leaving the page.
mergeInto(LibraryManager.library, {

  ArmKeyboardBack: function (objectNamePtr) {
    var state = window.__numericKeyboardBack;
    if (!state) {
      state = window.__numericKeyboardBack = {
        marker: "__unity_numeric_keyboard_open__",
        active: false,
        ignoreNextPop: false,
        listening: false,
        objectName: ""
      };
    }

    state.objectName = UTF8ToString(objectNamePtr);
    if (state.active) return;

    // other popstate handlers on the page can check this flag and skip the event
    var markHandled = function () {
      window.__numericKeyboardHandledPop = true;
      setTimeout(function () { window.__numericKeyboardHandledPop = false; }, 0);
    };

    if (!state.listening) {
      state.listening = true;
      // capture phase + stopImmediatePropagation: the host page never sees popstate events we caused
      window.addEventListener("popstate", function (event) {
        if (state.ignoreNextPop) {
          state.ignoreNextPop = false;
          markHandled();
          event.stopImmediatePropagation();
          return;
        }
        if (!state.active) return;
        state.active = false;
        markHandled();
        event.stopImmediatePropagation();

        if (typeof Module !== "undefined" && Module.SendMessage) {
          Module.SendMessage(state.objectName, "OnBrowserBackRequested", "");
        } else if (window.unityInstance && window.unityInstance.SendMessage) {
          window.unityInstance.SendMessage(state.objectName, "OnBrowserBackRequested", "");
        }
      }, true);
    }

    state.active = true;
    // keep the host router's own state (React Router's idx / key) so it still knows where it is
    var base = (history.state && typeof history.state === "object") ? history.state : {};
    history.pushState(Object.assign({}, base, { numericKeyboard: state.marker }), "");
  },

  DisarmKeyboardBack: function () {
    var state = window.__numericKeyboardBack;
    if (!state || !state.active) return;
    state.active = false;

    if (history.state && history.state.numericKeyboard === state.marker) {
      state.ignoreNextPop = true;
      setTimeout(function () { state.ignoreNextPop = false; }, 400);
      history.back();
    }
  }
});
