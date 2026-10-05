(() => {
  const storageKey = "auditarium.color-mode";
  const supportedModes = Object.freeze(["dark", "light", "system"]);
  const systemPreference = window.matchMedia?.("(prefers-color-scheme: dark)");
  let mode = document.documentElement.getAttribute("data-aud-color-mode") || "dark";

  const isSupported = candidate => supportedModes.includes(candidate);
  const resolveMode = candidate => candidate === "system"
    ? systemPreference?.matches ? "dark" : "light"
    : candidate;

  const applyMode = candidate => {
    document.documentElement.setAttribute("data-bs-theme", resolveMode(candidate));
    document.documentElement.setAttribute("data-aud-color-mode", candidate);
    document.dispatchEvent(new CustomEvent("auditarium:themechange", {
      detail: { mode: candidate, resolvedMode: resolveMode(candidate) }
    }));
  };

  const persistMode = candidate => {
    try {
      window.localStorage.setItem(storageKey, candidate);
    } catch {
      // The current page retains its chosen mode when persistence is unavailable.
    }
  };

  if (!isSupported(mode)) {
    mode = "dark";
  }

  applyMode(mode);

  systemPreference?.addEventListener("change", () => {
    if (mode === "system") {
      applyMode(mode);
    }
  });

  window.AuditariumTheme = Object.freeze({
    modes: supportedModes,
    getMode: () => mode,
    setMode: candidate => {
      if (!isSupported(candidate)) {
        throw new TypeError("Unsupported Auditarium color mode.");
      }

      mode = candidate;
      persistMode(mode);
      applyMode(mode);
    }
  });
})();
