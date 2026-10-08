(() => {
    "use strict";

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

    const actionFormatter = (cell, formatterParams) => {
        const row = cell.getRow().getData();
        const action = window.AuditariumGridActions?.[formatterParams.action];
        if (typeof action !== "function" || (formatterParams.visibleField && !row[formatterParams.visibleField])) {
            return "";
        }

        const button = document.createElement("button");
        button.type = "button";
        button.className = formatterParams.buttonClass || "btn btn-outline-secondary btn-sm aud-tabulator-action";
        button.title = formatterParams.label;
        button.setAttribute("aria-label", formatterParams.label);
        button.innerHTML = `<i class="${formatterParams.icon}" aria-hidden="true"></i>`;
        button.addEventListener("click", () => action(row, button));
        return button;
    };

    document.querySelectorAll("[data-aud-tabulator]").forEach((element) => {
        if (typeof window.Tabulator !== "function") {
            return;
        }

        const columns = JSON.parse(element.dataset.audColumns);
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
        });

        const fallback = element.previousElementSibling;
        const table = new window.Tabulator(element, {
            ajaxURL: element.dataset.audTableUrl,
            ajaxURLGenerator(url, _config, params) {
                const requestUrl = new URL(url, window.location.origin);
                requestUrl.searchParams.set("handler", "Table");
                const sorter = params.sort?.[0];
                requestUrl.searchParams.set("page", params.page || "1");
                requestUrl.searchParams.set("size", params.size || "25");
                requestUrl.searchParams.set("sort", sorter ? sortFields[sorter.field] : element.dataset.audDefaultSort);
                requestUrl.searchParams.set("direction", sorter?.dir || element.dataset.audDefaultDirection);
                return requestUrl.toString();
            },
            columns,
            layout: "fitColumns",
            pagination: true,
            paginationMode: "remote",
            paginationSize: 25,
            paginationSizeSelector: [25, 50, 100, 200],
            sortMode: "remote",
            initialSort: [{ column: element.dataset.audDefaultSort, dir: element.dataset.audDefaultDirection }],
            responsiveLayout: "collapse",
            responsiveLayoutCollapseStartOpen: false,
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

        table.on("tableBuilt", () => fallback?.setAttribute("hidden", "hidden"));
    });
})();
