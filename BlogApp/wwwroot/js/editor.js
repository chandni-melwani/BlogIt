// wwwroot/js/editor.js

let easyMDE = null;

window.initEditor = function (initialContent) {
    if (easyMDE) {
        try { easyMDE.toTextArea(); } catch (e) { }
        easyMDE = null;
    }

    easyMDE = new EasyMDE({
        element: document.getElementById("easymde-editor"),
        initialValue: initialContent || "",
        autofocus: true,
        spellChecker: false,
        placeholder: "Start writing your post...",
        toolbar: [
            "bold", "italic", "strikethrough", "|",
            "heading-1", "heading-2", "heading-3", "|",
            "quote", "unordered-list", "ordered-list", "|",
            "link", "image", "|",
            "side-by-side", "fullscreen", "|",
            "guide"
        ],
        renderingConfig: {
            singleLineBreaks: false,
            codeSyntaxHighlighting: false,
            markedOptions: { gfm: true, tables: true, breaks: false }
        },
        status: false,
        minHeight: "460px",
        sideBySideFullscreen: false,
    });
};

window.toggleEditorPreview = function () {
    if (!easyMDE) return Promise.resolve(false);

    return new Promise((resolve) => {
        // Measure the toolbar height NOW, before toggling, while the toolbar is
        // guaranteed to be rendered and laid out. We write it as a CSS custom
        // property so the fullscreen-preview absolute-position rule can reference
        // it without any hardcoded pixel value.
        //
        // Why measure here instead of once at init:
        //   Toolbar height can change with viewport width (responsive wrapping),
        //   so a cached value from init time could be stale. Measuring fresh on
        //   every toggle ensures the property is always current.
        const toolbar = easyMDE.codemirror
            ? easyMDE.codemirror.getWrapperElement().parentElement
                  ?.querySelector('.editor-toolbar')
            : document.querySelector('.editor-toolbar');

        if (toolbar) {
            const h = toolbar.getBoundingClientRect().height;
            document.documentElement.style.setProperty('--editor-toolbar-height', `${h}px`);
        }

        // Trigger the toggle.
        EasyMDE.togglePreview(easyMDE);

        // EasyMDE applies 'editor-preview-active' inside its own setTimeout(fn, 1).
        // We wait 20 ms — safely past that internal delay — before reading DOM state.
        setTimeout(() => {
            // In normal (non-fullscreen) mode the preview element is a direct child of
            // .EasyMDEContainer and our grid CSS positions it correctly.
            //
            // In fullscreen mode EasyMDE reparents the preview element: it moves from
            // being a direct child of .EasyMDEContainer to a descendant of
            // .CodeMirror-fullscreen (which sits inside .EasyMDEContainer). The grid
            // rules that use the `>` (direct-child) combinator therefore stop matching,
            // EasyMDE's own default CSS takes over with `position: absolute; top: 0`,
            // and the preview renders underneath the toolbar — visually hiding the
            // first ~50 px of content (where the H1 sits). This is the confirmed root
            // cause; scrollTop was verified to be 0 throughout, so this is a layout/
            // overlap bug, not a scroll bug.
            //
            // The fix is in the CSS (descendant combinator + explicit absolute position
            // using --editor-toolbar-height). The scrollTop = 0 write below is kept as
            // lightweight defensive insurance for any genuine future scroll edge case,
            // but it is not solving the visual problem described in this bug.
            const container = (easyMDE.codemirror
                ? easyMDE.codemirror.getWrapperElement().parentElement
                : null) || document.querySelector('.EasyMDEContainer');

            const previewEl = container
                ? (container.querySelector('.editor-preview-active') ||
                    container.querySelector('.editor-preview-active-side'))
                : null;

            const isPreview = !!previewEl;

            if (isPreview) {
                // Apply the shared markdown-content typography class directly to
                // EasyMDE's preview element — same class used on PostView's
                // .blog-content div — so both surfaces share one CSS source.
                previewEl.classList.add('markdown-content');

                // Defensive scroll reset.
                previewEl.scrollTop = 0;
            } else {
                // Returning to edit mode — clean up the class so it doesn't
                // linger if EasyMDE reuses the element on a subsequent toggle.
                const stalePreview = container
                    ? container.querySelector('.editor-preview')
                    : document.querySelector('.editor-preview');
                if (stalePreview) stalePreview.classList.remove('markdown-content');

                // Restore CodeMirror focus.
                setTimeout(() => {
                    if (easyMDE && easyMDE.codemirror) {
                        easyMDE.codemirror.refresh();
                        easyMDE.codemirror.focus();
                    }
                }, 50);
            }

            resolve(isPreview);
        }, 20);
    });
};

window.clearEditorContent = function () {
    if (!easyMDE) return;
    easyMDE.value("");
    if (easyMDE.codemirror) {
        easyMDE.codemirror.refresh();
        easyMDE.codemirror.focus();
    }
};

window.getEditorContent = function () {
    return easyMDE ? easyMDE.value() : "";
};

window.setEditorContent = function (content) {
    if (!easyMDE) return;
    easyMDE.value(content || "");
    if (easyMDE.codemirror) {
        easyMDE.codemirror.refresh();
    }
};

let _beforeUnloadHandler = null;

window.setBeforeUnloadDirty = function (isDirty) {
    if (isDirty) {
        if (!_beforeUnloadHandler) {
            _beforeUnloadHandler = function (e) {
                e.preventDefault();
                e.returnValue = "";
                return "";
            };
            window.addEventListener("beforeunload", _beforeUnloadHandler);
        }
    } else {
        if (_beforeUnloadHandler) {
            window.removeEventListener("beforeunload", _beforeUnloadHandler);
            _beforeUnloadHandler = null;
        }
    }
};