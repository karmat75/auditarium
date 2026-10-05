(() => {
  const storageKey = "auditarium.color-mode";
  const supportedModes = new Set(["dark", "light", "system"]);
  let mode = "dark";

  try {
    const storedMode = window.localStorage.getItem(storageKey);
    if (supportedModes.has(storedMode)) {
      mode = storedMode;
    }
  } catch {
    // Storage can be unavailable in private or restricted browser contexts.
  }

  const resolvedMode = mode === "system" && window.matchMedia?.("(prefers-color-scheme: dark)").matches
    ? "dark"
    : mode === "system"
      ? "light"
      : mode;

  document.documentElement.setAttribute("data-bs-theme", resolvedMode);
  document.documentElement.setAttribute("data-aud-color-mode", mode);
})();
