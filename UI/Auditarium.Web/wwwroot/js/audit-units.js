(() => {
    "use strict";

    window.AuditariumGridActions = window.AuditariumGridActions || {};
    window.AuditariumGridActions.deleteAuditUnit = (row) => {
        const modal = document.querySelector("#delete-audit-unit-modal");
        if (!modal || !window.bootstrap) {
            return;
        }

        modal.querySelector("[data-aud-delete-name]").textContent = row.name;
        modal.querySelector("[data-aud-delete-id]").value = row.id;
        modal.querySelector("[data-aud-delete-version]").value = row.concurrencyVersion;
        window.bootstrap.Modal.getOrCreateInstance(modal).show();
    };
})();
