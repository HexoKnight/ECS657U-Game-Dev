mergeInto(LibraryManager.library, {
    // adapted from here:
    // https://discussions.unity.com/t/cursor-lockstate-not-reflect-the-actual-lockstate-of-webgl-canvas/913338
    RegisterCursorLockChangedCallback: function(callback) {
        document.addEventListener('pointerlockchange', function() {
            Module['dynCall_vi'](callback, document.pointerLockElement == Module['canvas']);
        });
    }
});
