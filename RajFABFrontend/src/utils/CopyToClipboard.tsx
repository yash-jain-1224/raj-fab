const copyToClipboard = async (text: string): Promise<boolean> => {
  if (!text) return false;

  if (navigator?.clipboard?.writeText) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch {
      console.error("Clipboard copy failed:");
      return false;
    }
  }

  try {
    const textarea = document.createElement("textarea");
    textarea.value = text;

    // Required for mobile & IP contexts
    textarea.setAttribute("readonly", "");
    textarea.style.position = "absolute";
    textarea.style.left = "-9999px";
    textarea.style.top = "0";

    document.body.appendChild(textarea);

    textarea.focus();
    textarea.select();

    const successful = document.execCommand("copy");

    document.body.removeChild(textarea);

    return successful;
  } catch (err) {
    console.error("Clipboard copy failed:", err);
    return false;
  }
};

export default copyToClipboard;
