(() => {
    "use strict";

    const gridTables = new Map();
    const pendingRedraws = new Set();
    let redrawFrame;
    const queueRedraw = (grid) => {
        pendingRedraws.add(grid);
        window.cancelAnimationFrame(redrawFrame);
        redrawFrame = window.requestAnimationFrame(() => {
            pendingRedraws.forEach(({ table }) => table.redraw(true));
            pendingRedraws.clear();
        });
    };
    const queueRedrawForChangedWidth = (target, width) => {
        const grid = gridTables.get(target);
        if (grid && grid.width !== width) {
            grid.width = width;
            queueRedraw(grid);
        }
    };
    const resizeObserver = typeof window.ResizeObserver === "function"
        ? new window.ResizeObserver((entries) => entries.forEach((entry) => queueRedrawForChangedWidth(entry.target, entry.contentRect.width)))
        : null;
    window.addEventListener("resize", () => gridTables.forEach((grid, target) => queueRedrawForChangedWidth(target, target.clientWidth)));

    const statusFormatter = (cell) => {
        const status = document.createElement("span");
        const tone = cell.getRow().getData()[`${cell.getField()}Tone`] || "neutral";
        const badgeTone = { neutral: "secondary", info: "info", warning: "warning", success: "success", danger: "danger" }[tone] || "secondary";
        status.className = `badge text-bg-${badgeTone}`;
        status.setAttribute("aria-label", `Status: ${cell.getValue()}`);
        status.textContent = cell.getValue() || "–";
        return status;
    };

    const linkFormatter = (cell, formatterParams) => {
        const link = document.createElement("a");
        const target = cell.getRow().getData()[formatterParams.linkField];
        link.href = target;
        link.textContent = cell.getValue() || "–";
        return link;
    };

    const actionControl = (row, definition) => {
        if (definition.visibleField && !row[definition.visibleField]) {
            return null;
        }

        const target = definition.urlField && row[definition.urlField];
        const action = definition.action && window.AuditariumGridActions?.[definition.action];
        if (!target && typeof action !== "function") {
            return null;
        }

        const control = target ? document.createElement("a") : document.createElement("button");
        control.className = definition.buttonClass || "btn btn-outline-secondary btn-sm aud-tabulator-action";
        control.title = definition.label;
        control.setAttribute("aria-label", definition.label);
        if (target) control.href = target;
        else {
            control.type = "button";
            control.addEventListener("click", () => action(row, control));
        }
        const icon = document.createElement("i");
        icon.className = definition.icon;
        icon.setAttribute("aria-hidden", "true");
        control.append(icon);
        return control;
    };

    const actionFormatter = (cell, formatterParams) => actionControl(cell.getRow().getData(), formatterParams) || "";
    const actionsFormatter = (cell, formatterParams) => {
        const actions = document.createElement("div");
        actions.className = "aud-tabulator-actions";
        formatterParams.actions.forEach((definition) => {
            const control = actionControl(cell.getRow().getData(), definition);
            if (control) actions.append(control);
        });
        return actions.childElementCount ? actions : "";
    };

    const makeTreeControlsAccessible = (element) => {
        element.querySelectorAll(".tabulator-data-tree-control").forEach((control) => {
            if (control.dataset.audTreeControl === "true") return;
            control.dataset.audTreeControl = "true";
            control.tabIndex = 0;
            control.setAttribute("role", "button");
            control.setAttribute("aria-label", "Untergeordnete Audit Units ein- oder ausblenden");
            control.setAttribute("aria-expanded", control.querySelector(".tabulator-data-tree-control-collapse") ? "true" : "false");
            control.addEventListener("keydown", (event) => {
                if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    control.click();
                }
            });
        });
    };

    const scopeTypeFormatter = (cell) => {
        const icons = { ORGANIZATION: "bi-building", SITE: "bi-geo-alt", BUILDING: "bi-buildings", ROOM: "bi-door-open", TECHNICAL_AREA: "bi-cpu", APPLICATION: "bi-window-stack", OTHER: "bi-tag" };
        const wrapper = document.createElement("span");
        const icon = document.createElement("i");
        icon.className = `bi ${icons[cell.getRow().getData().scopeTypeKey] || "bi-diagram-3"} me-1`;
        icon.setAttribute("aria-hidden", "true");
        wrapper.append(icon, document.createTextNode(cell.getValue() || "–"));
        return wrapper;
    };

    const flattenTree = (nodes, path = []) => nodes.flatMap((node) => {
        const hierarchyPath = [...path, node.name];
        const row = { name: node.name, hierarchyPath: hierarchyPath.join(" / "), scopeType: node.scopeType, usageState: node.usageState };
        return [row, ...flattenTree(node.children || [], hierarchyPath)];
    });
    const csv = (rows) => ["Hierarchy,Name,Scope Type,Status", ...rows.map((row) => [row.hierarchyPath, row.name, row.scopeType, row.usageState].map((value) => `"${String(value).replaceAll("\"", "\"\"")}"`).join(","))].join("\r\n");
    const download = (contents, type, name) => {
        const anchor = document.createElement("a");
        anchor.href = URL.createObjectURL(new Blob([contents], { type })); anchor.download = name; anchor.click(); URL.revokeObjectURL(anchor.href);
    };
    const printTree = (rows) => {
        const popup = window.open("", "_blank");
        if (!popup) {
            window.alert("Die Druckansicht konnte nicht geöffnet werden.");
            return;
        }
        popup.opener = null;
        const escape = (value) => String(value).replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
        popup.document.write(`<!doctype html><html lang="de"><head><title>Audit Units</title><style>body{font-family:system-ui,sans-serif;margin:2rem}table{border-collapse:collapse;width:100%}th,td{border:1px solid #777;padding:.4rem;text-align:left}th{background:#eee}</style></head><body><h1>Audit Units</h1><table><thead><tr><th>Hierarchie</th><th>Name</th><th>Typ</th><th>Status</th></tr></thead><tbody>${rows.map((row) => `<tr><td>${escape(row.hierarchyPath)}</td><td>${escape(row.name)}</td><td>${escape(row.scopeType)}</td><td>${escape(row.usageState)}</td></tr>`).join("")}</tbody></table></body></html>`);
        popup.onload = () => { popup.focus(); popup.print(); };
        popup.document.close();
    };

    const treeRows = (rows) => rows.flatMap((row) => [row, ...treeRows(row.getTreeChildren())]);
    const rootTreeRows = (table) => treeRows(table.getRows().filter((row) => !row.getTreeParent()));
    const captureTreeState = (table) => ({
        expandedIds: new Set(rootTreeRows(table)
            .filter((row) => row.getTreeChildren().length && row.isTreeExpanded())
            .map((row) => String(row.getData().id))),
        scrollY: window.scrollY
    });
    const restoreTreeState = (table, state) => {
        rootTreeRows(table).forEach((row) => {
            if (row.getTreeChildren().length && !state.expandedIds.has(String(row.getData().id))) {
                row.treeCollapse();
            }
        });
        const scrollingElement = document.scrollingElement;
        window.scrollTo(0, Math.min(state.scrollY, Math.max(0, scrollingElement.scrollHeight - window.innerHeight)));
    };

    const arrangeExternalPagination = (footer) => {
        if (!footer) {
            return;
        }

        const footerChildren = Array.from(footer.children);
        const pageSizeSelect = footerChildren.find((child) => child.matches(".tabulator-page-size"));
        const pageSizeLabel = footerChildren.find((child) => child.matches("label"));
        const paginationControls = footerChildren.filter((child) => child.matches(".tabulator-page, .tabulator-pages"));
        const pageSize = document.createElement("div");
        const pagination = document.createElement("div");
        pageSize.className = "aud-grid-page-size";
        pagination.className = "aud-grid-pagination";

        if (pageSizeSelect) {
            pageSize.append(pageSizeSelect);
        }
        if (pageSizeLabel) {
            pageSize.append(pageSizeLabel);
        }
        paginationControls.forEach((control) => pagination.append(control));
        footer.replaceChildren(pageSize, pagination);
    };

    document.querySelectorAll("[data-aud-tabulator]").forEach((element) => {
        if (typeof window.Tabulator !== "function") {
            return;
        }

        const columns = JSON.parse(element.dataset.audColumns);
        const isTree = element.dataset.audTree === "true";
        const sortFields = {};
        columns.forEach((column) => {
            column.headerSort = column.sortable === true;
            if (column.sortable === true) {
                sortFields[column.field] = column.sortField || column.field;
            }
            if (column.linkField) {
                column.formatter = linkFormatter;
                column.formatterParams = { linkField: column.linkField };
            }
            if (column.status) {
                column.formatter = statusFormatter;
            }
            if (column.scopeType) {
                column.formatter = scopeTypeFormatter;
            }
            if (column.action) {
                column.formatter = actionFormatter;
                column.formatterParams = {
                    action: column.action,
                    buttonClass: column.buttonClass,
                    icon: column.icon,
                    label: column.label,
                    visibleField: column.visibleField
                };
                column.headerSort = false;
            }
            if (column.actions) {
                column.formatter = actionsFormatter;
                column.formatterParams = { actions: column.actions };
                column.headerSort = false;
            }
        });

        const fallback = element.previousElementSibling;
        const footer = element.nextElementSibling?.matches("[data-aud-tabulator-footer]") ? element.nextElementSibling : null;
        const table = new window.Tabulator(element, {
            ajaxURL: element.dataset.audTableUrl,
            ajaxURLGenerator(url, _config, params) {
                const requestUrl = new URL(url, window.location.origin);
                requestUrl.searchParams.set("handler", isTree ? "Tree" : "Table");
                const sorter = params.sort?.[0];
                if (!isTree) { requestUrl.searchParams.set("page", params.page || "1"); requestUrl.searchParams.set("size", params.size || "25"); }
                requestUrl.searchParams.set("sort", sorter ? sortFields[sorter.field] : element.dataset.audDefaultSort);
                requestUrl.searchParams.set("direction", sorter?.dir || element.dataset.audDefaultDirection);
                return requestUrl.toString();
            },
            columns,
            layout: "fitDataStretch",
            pagination: !isTree,
            paginationMode: isTree ? undefined : "remote",
            paginationSize: isTree ? undefined : 25,
            paginationSizeSelector: isTree ? undefined : [25, 50, 100, 200],
            paginationElement: footer || false,
            sortMode: "remote",
            dataTree: isTree,
            dataTreeChildField: "children",
            dataTreeStartExpanded: isTree,
            dataTreeSort: !isTree ? undefined : false,
            initialSort: [{ column: element.dataset.audDefaultSort, dir: element.dataset.audDefaultDirection }],
            placeholder: "Keine Einträge gefunden.",
            langs: {
                de: {
                    pagination: {
                        first: "Erste", first_title: "Erste Seite", last: "Letzte", last_title: "Letzte Seite",
                        prev: "Zurück", prev_title: "Vorherige Seite", next: "Weiter", next_title: "Nächste Seite",
                        page_size: "Zeilen pro Seite"
                    }
                }
            },
            locale: "de"
        });

        if (isTree) {
            table.on("renderComplete", () => makeTreeControlsAccessible(element));
            let hasLoadedTree = false;
            let pendingTreeState;
            table.on("dataLoading", () => {
                if (hasLoadedTree) {
                    pendingTreeState = captureTreeState(table);
                }
            });
            table.on("dataProcessed", () => {
                if (pendingTreeState) {
                    restoreTreeState(table, pendingTreeState);
                    pendingTreeState = undefined;
                }
                hasLoadedTree = true;
            });
        }

        table.on("tableBuilt", () => {
            fallback?.setAttribute("hidden", "hidden");
            arrangeExternalPagination(footer);
            const container = element.parentElement || element;
            const grid = { table, width: container.clientWidth };
            gridTables.set(container, grid);
            resizeObserver?.observe(container);
        });
        if (isTree) {
            document.querySelectorAll("[data-aud-tree-export]").forEach((button) => button.addEventListener("click", () => {
                const rows = flattenTree(table.getData());
                if (button.dataset.audTreeExport === "csv") download(csv(rows), "text/csv;charset=utf-8", "audit-units.csv");
                else download(JSON.stringify(rows, null, 2), "application/json", "audit-units.json");
            }));
            document.querySelectorAll("[data-aud-tree-print]").forEach((button) => button.addEventListener("click", () => printTree(flattenTree(table.getData()))));
        }
    });
})();
