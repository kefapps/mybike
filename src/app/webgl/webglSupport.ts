type WebGlProbeCanvas = {
  getContext: (contextId: string) => unknown;
};

type WebGlCanvasFactory = () => WebGlProbeCanvas;

function createBrowserCanvas(): WebGlProbeCanvas {
  if (typeof document === "undefined") {
    throw new Error("document is unavailable");
  }

  return document.createElement("canvas");
}

export function isWebGlAvailable(
  createCanvas: WebGlCanvasFactory = createBrowserCanvas
): boolean {
  try {
    const canvas = createCanvas();

    return Boolean(
      canvas.getContext("webgl2") ??
        canvas.getContext("webgl") ??
        canvas.getContext("experimental-webgl")
    );
  } catch {
    return false;
  }
}
