// wwwroot/js/drawflowInterop.js
// نیازمند: <script src="https://unpkg.com/drawflow/dist/drawflow.min.js"></script>
(function () {
    'use strict';

    const editors = new Map(); // containerId -> { editor, selectedId, dotNetRef }

    function init(containerId, dotNetRef, options) {
        const el = document.getElementById(containerId);
        if (!el) {
            console.error('Drawflow container not found', containerId);
            return;
        }

        const editor = new Drawflow(el, options || {});
        editor.reroute = true;
        editor.start();

        const ctx = {editor, selectedId: null, dotNetRef: dotNetRef || null};
        editors.set(containerId, ctx);

        // نگه‌داشتن آی‌دی نودِ انتخاب‌شده
        editor.on('nodeSelected', function (id) {
            ctx.selectedId = id;
        });
        editor.on('nodeUnselected', function () {
            ctx.selectedId = null;
        });

        // اگر خواستی هر تغییری را به .NET خبر بدهی:
        const notify = () => {
            if (!ctx.dotNetRef) return;
            try {
                ctx.dotNetRef.invokeMethodAsync('OnEditorChanged', JSON.stringify(editor.export()));
            } catch (e) { /* ignore */
            }
        };
        ['nodeCreated', 'nodeRemoved', 'nodeMoved', 'connectionCreated', 'connectionRemoved']
            .forEach(evt => editor.on(evt, notify));
    }

    function addNode(containerId, name, type, data, posX, posY, inputs, outputs) {
        const ctx = editors.get(containerId);
        if (!ctx) return null;
        let html = "";
        switch (type) {
            case "start":
                html = `<div class="node-start">${name}</div>`;
                break;
            case "end":
                html = `<div class="node-end">${name}</div>`;
                break;
            case "process":
                html = `<div class="node-process">${name}</div>`;
                break;
            case "decision":
                html = `<div class="node-decision">${name}</div>`;
                break;
            case "io":
                html = `<div class="node-io">${name}</div>`;
                break;
        }
        const id = ctx.editor.addNode(
            name || 'step',
            inputs ?? 1,
            outputs ?? 1,
            posX ?? 120,
            posY ?? 80,
            'custom-node',
            data || {},
            html
        );
        return id;
    }

    function exportJson(containerId) {
        const ctx = editors.get(containerId);
        if (!ctx) return null;
        return JSON.stringify(ctx.editor.export());
    }

    function importJson(containerId, json) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        try {
            const obj = typeof json === 'string' ? JSON.parse(json) : json;
            ctx.editor.clearModuleSelected();
            ctx.editor.import(obj);
        } catch (e) {
            console.error('drawflow import error:', e);
        }
    }

    function clear(containerId) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        ctx.editor.clear();
    }

    function removeSelected(containerId) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        if (ctx.selectedId != null) {
            // طبق داکیومنت باید رشته‌ی 'node-<id>' را بدهیم
            ctx.editor.removeNodeId('node-' + ctx.selectedId);
            ctx.selectedId = null;
        } else {
            console.warn('No node is selected');
        }
    }

    function setMode(containerId, mode) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        ctx.editor.editor_mode = mode || 'edit'; // 'edit' | 'fixed' | 'view'
    }

    function zoomIn(containerId) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        ctx.editor.zoom_in();
    }

    function zoomOut(containerId) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        ctx.editor.zoom_out();
    }

    function fit(containerId) {
        const ctx = editors.get(containerId);
        if (!ctx) return;
        ctx.editor.zoom_reset(); // Reset zoom/pan (جایگزین ساده‌ی fit)
    }

    // اکسپورت به window
    window.BlazorDrawflow = {
        init,
        addNode,
        exportJson,
        importJson,
        clear,
        removeSelected,
        setMode,
        zoomIn,
        zoomOut,
        fit
    };
})();
