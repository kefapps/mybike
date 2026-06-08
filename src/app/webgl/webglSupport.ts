type WebGlProbeCanvas = {
  getContext: (contextId: string) => unknown;
};

type WebGlCanvasFactory = () => WebGlProbeCanvas;

export type WebGlProbeResult = {
  available: boolean;
  contextId?: "webgl2" | "webgl" | "experimental-webgl";
  reason?: string;
};

function createBrowserCanvas(): WebGlProbeCanvas {
  if (typeof document === "undefined") {
    throw new Error("document is unavailable");
  }

  return document.createElement("canvas");
}

export function probeWebGlAvailability(
  createCanvas: WebGlCanvasFactory = createBrowserCanvas
): WebGlProbeResult {
  try {
    const canvas = createCanvas();
    const contexts = ["webgl2", "webgl", "experimental-webgl"] as const;

    for (const contextId of contexts) {
      const context = canvas.getContext(contextId);

      if (context) {
        return { available: true, contextId };
      }
    }

    return {
      available: false,
      reason: "Aucun contexte WebGL (webgl2/webgl/experimental-webgl) n'est disponible."
    };
  } catch {
    return {
      available: false,
      reason: "La creation du canvas WebGL a echoue."
    };
  }
}

export function isWebGlAvailable(
  createCanvas: WebGlCanvasFactory = createBrowserCanvas
): boolean {
  return probeWebGlAvailability(createCanvas).available;
}
