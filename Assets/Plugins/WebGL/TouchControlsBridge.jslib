// jslib plugin: exposes two functions the C# side can DllImport to flip
// page-level body classes:
//   body.gameplay — show the touch joysticks/buttons overlay
//   body.combat   — page swallows raw canvas touches (combat scenes only,
//                   so taps in the hub still hit Unity UI buttons)
mergeInto(LibraryManager.library, {
    AIFG_SetTouchControls: function (visible) {
        if (typeof window.AIFG_setTouchControls === 'function') {
            window.AIFG_setTouchControls(!!visible);
        }
    },
    AIFG_SetCombat: function (active) {
        if (typeof window.AIFG_setCombat === 'function') {
            window.AIFG_setCombat(!!active);
        }
    },
});
