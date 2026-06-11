import { useEffect, useId, useRef } from "react";

import type { AssetManifestEntry } from "../../manifest/assetManifestTypes";

type CreditsScreenProps = {
  entries: AssetManifestEntry[];
  policyTicket: string;
  policySummary: string;
  policyPath: string;
  onClose: () => void;
};

const LICENSE_LABELS: Record<AssetManifestEntry["license"], string> = {
  "CC0": "CC0 1.0 (domaine public)",
  "CC-BY": "CC BY 4.0",
  "CC-BY-SA": "CC BY-SA 4.0",
  "CC-BY-NC": "CC BY-NC 4.0",
  "CC-BY-NC-SA": "CC BY-NC-SA 4.0",
  "CC-BY-ND": "CC BY-ND 4.0",
  "CC-BY-NC-ND": "CC BY-NC-ND 4.0",
  "MIT": "MIT",
  "Apache-2.0": "Apache 2.0",
  "OFL": "SIL Open Font License 1.1",
  "Public Domain": "Domaine public",
  "Other": "Licence specifique"
};

const CATEGORY_LABELS: Record<AssetManifestEntry["category"], string> = {
  "3d-model": "Modele 3D",
  "texture": "Texture",
  "hdri": "HDRI",
  "audio": "Audio",
  "font": "Police",
  "sprite": "Sprite",
  "code": "Code",
  "other": "Autre"
};

function attributionLine(entry: AssetManifestEntry): string {
  const licenseLabel = LICENSE_LABELS[entry.license] ?? entry.license;
  const modificationSuffix = entry.modifications
    ? `, ${entry.modifications}`
    : "";

  return `${entry.name} par ${entry.author}, ${licenseLabel}${modificationSuffix}. Source : ${entry.source}.`;
}

export function CreditsScreen({
  entries,
  policyTicket,
  policySummary,
  policyPath,
  onClose
}: CreditsScreenProps) {
  const titleId = useId();
  const closeButtonRef = useRef<HTMLButtonElement | null>(null);

  useEffect(() => {
    closeButtonRef.current?.focus();
  }, []);

  return (
    <div
      className="credits-overlay"
      role="dialog"
      aria-modal="true"
      aria-labelledby={titleId}
    >
      <div className="credits-panel">
        <header className="credits-header">
          <p className="eyebrow">Crédits</p>
          <h2 id={titleId}>Assets tiers et licences</h2>
        </header>

        <p className="credits-policy">
          {policySummary} Voir la note detaillee dans{" "}
          <code>{policyPath}</code> (ticket {policyTicket}).
        </p>

        {entries.length === 0 ? (
          <p className="credits-empty">
            Aucun asset tiers integre a la vertical slice mock pour le moment.
            Le manifest <code>src/manifest/assets-manifest.json</code> est pret a
            recevoir la liste des futures contributions CC0 ou CC-BY.
          </p>
        ) : (
          <ul className="credits-list">
            {entries.map((entry) => (
              <li key={entry.id} className="credits-list__item">
                <h3>{entry.name}</h3>
                <p className="credits-list__category">
                  {CATEGORY_LABELS[entry.category] ?? entry.category}
                </p>
                <p>{attributionLine(entry)}</p>
                {entry.licenseUrl ? (
                  <p className="credits-list__license-url">
                    Texte integral :{" "}
                    <a
                      href={entry.licenseUrl}
                      target="_blank"
                      rel="noreferrer noopener"
                    >
                      {entry.licenseUrl}
                    </a>
                  </p>
                ) : null}
                {entry.notes ? (
                  <p className="credits-list__notes">{entry.notes}</p>
                ) : null}
              </li>
            ))}
          </ul>
        )}

        <div className="credits-actions">
          <button
            ref={closeButtonRef}
            className="primary-action"
            type="button"
            onClick={onClose}
          >
            Fermer
          </button>
        </div>
      </div>
    </div>
  );
}
