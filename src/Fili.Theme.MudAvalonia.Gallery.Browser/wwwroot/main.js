import { dotnet } from './_framework/dotnet.js'

// Browser shortcuts belong to the browser.
//
// Avalonia's browser host listens for keydown on its own element and calls preventDefault on
// effectively every key, so without this Ctrl+F, Ctrl+P, zoom, reload and the dev tools all
// did nothing on the gallery page. A capture listener on window runs before Avalonia's, and for
// the shortcuts below it stops the event there: Avalonia never sees it, so the browser's own
// action goes ahead. Editing keys - Ctrl+A, C, V, X, Y, Z, arrows, Tab - are deliberately NOT
// here; the text fields need them.
//
// Note that the gallery is drawn on a canvas, so the find bar opens but has no page text to find.
const isBrowserShortcut = (e) => {
    if (['F1', 'F3', 'F5', 'F6', 'F11', 'F12'].includes(e.key)) {
        return true;
    }

    const command = e.ctrlKey || e.metaKey;
    const key = e.key.length === 1 ? e.key.toLowerCase() : e.key;

    // Back and forward.
    if (e.altKey && !command && ['ArrowLeft', 'ArrowRight', 'Home'].includes(key)) {
        return true;
    }

    if (!command || e.altKey) {
        return false;
    }

    // Developer tools: Ctrl+Shift+I / J / C, Cmd+Option+... is covered by F12 above.
    if (e.shiftKey && ['i', 'j', 'c'].includes(key)) {
        return true;
    }

    // Find, print, reload, address bar, tabs and windows, history, bookmarks, downloads, zoom.
    return ['f', 'g', 'p', 'r', 'l', 'k', 't', 'n', 'w', 'h', 'd', 'j', 'o', 's', 'u',
            '+', '=', '-', '_', '0', 'Tab', 'PageUp', 'PageDown'].includes(key);
};

for (const type of ['keydown', 'keyup']) {
    window.addEventListener(type, (e) => {
        if (isBrowserShortcut(e)) {
            e.stopImmediatePropagation();
        }
    }, { capture: true });
}

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
