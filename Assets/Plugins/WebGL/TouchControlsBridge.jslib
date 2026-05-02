// jslib plugin: exposes a single function the C# side can DllImport to flip
// the body.gameplay class in the page so touch joysticks/buttons only show
// in actual gameplay scenes.
mergeInto(LibraryManager.library, {
    AIFG_SetTouchControls: function (visible) {
        if (typeof window.AIFG_setTouchControls === 'function') {
            window.AIFG_setTouchControls(!!visible);
        }
    },
});
