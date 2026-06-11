/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { CreditsScreen } from "./CreditsScreen";
import type { AssetManifestEntry } from "../../manifest/assetManifestTypes";

const SAMPLE_ENTRY: AssetManifestEntry = {
  id: "sample-cc-by",
  name: "Sample rock",
  author: "Alice Artiste",
  source: "https://opengameart.org/example/sample-rock",
  category: "3d-model",
  license: "CC-BY",
  licenseUrl: "https://creativecommons.org/licenses/by/4.0/",
  modifications: "decoupe et retextured pour la route Echappee"
};

function renderCredits(entries: AssetManifestEntry[]) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);
  const onClose = vi.fn();

  act(() => {
    root.render(
      <CreditsScreen
        entries={entries}
        policyTicket="MYB-52"
        policySummary="Politique de credits des assets tiers."
        policyPath="THIRD_PARTY_ASSETS.md"
        onClose={onClose}
      />
    );
  });

  return { container, onClose, root };
}

describe("CreditsScreen", () => {
  let root: Root | undefined;

  beforeEach(() => {
    Object.assign(globalThis, { IS_REACT_ACT_ENVIRONMENT: true });
  });

  afterEach(() => {
    if (root) {
      act(() => {
        root?.unmount();
      });
    }

    document.body.replaceChildren();
    vi.restoreAllMocks();
    root = undefined;
  });

  it("shows the empty state and policy when the manifest has no entries", () => {
    const rendered = renderCredits([]);
    root = rendered.root;

    expect(rendered.container.textContent).toContain("Crédits");
    expect(rendered.container.textContent).toContain("Aucun asset tiers");
    expect(rendered.container.textContent).toContain("MYB-52");
    expect(rendered.container.textContent).toContain("THIRD_PARTY_ASSETS.md");
  });

  it("renders attribution lines and license URLs for CC-BY assets", () => {
    const rendered = renderCredits([SAMPLE_ENTRY]);
    root = rendered.root;

    expect(rendered.container.textContent).toContain("Sample rock");
    expect(rendered.container.textContent).toContain("Alice Artiste");
    expect(rendered.container.textContent).toContain("CC BY 4.0");
    expect(rendered.container.textContent).toContain("decoupe et retextured");
    expect(rendered.container.textContent).toContain("opengameart.org/example/sample-rock");
    const link = rendered.container.querySelector("a");
    expect(link?.getAttribute("href")).toBe(
      "https://creativecommons.org/licenses/by/4.0/"
    );
  });

  it("closes the overlay via the Fermer button", () => {
    const rendered = renderCredits([]);
    root = rendered.root;

    const button = Array.from(rendered.container.querySelectorAll("button")).find(
      (candidate) => candidate.textContent?.includes("Fermer")
    );

    if (!button) {
      throw new Error("Bouton Fermer introuvable");
    }

    act(() => {
      button.click();
    });

    expect(rendered.onClose).toHaveBeenCalledTimes(1);
  });
});
