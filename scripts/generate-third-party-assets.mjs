#!/usr/bin/env node
/**
 * Generate the THIRD_PARTY_ASSETS.md file from the manifest JSON.
 * Ticket: MYB-52.
 *
 * Usage: node scripts/generate-third-party-assets.mjs
 *        npm run credits:gen
 */
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(__dirname, "..");

const MANIFEST_PATH = resolve(repoRoot, "src/manifest/assets-manifest.json");
const OUTPUT_PATH = resolve(repoRoot, "THIRD_PARTY_ASSETS.md");

const LICENSE_LABELS = {
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

const CATEGORY_LABELS = {
  "3d-model": "Modele 3D",
  "texture": "Texture",
  "hdri": "HDRI",
  "audio": "Audio",
  "font": "Police",
  "sprite": "Sprite",
  "code": "Code",
  "other": "Autre"
};

function escapeCell(value) {
  if (value === undefined || value === null || value === "") {
    return "_aucun_";
  }
  return String(value).replace(/\|/g, "\\|");
}

export function buildCreditsMarkdown(manifest) {
  const ticket = manifest.policy?.ticket ?? "MYB-52";
  const summary =
    manifest.policy?.summary ??
    "Politique d'attribution des assets tiers MyBike.";

  const lines = [];
  lines.push("# Crédits des assets tiers — MyBike / Echappee 3D");
  lines.push("");
  lines.push(`> Ticket source : **${ticket}** ([MYB-020] Creer un ecran / document d'attribution des assets tiers).`);
  lines.push(`> Statut : regenere automatiquement depuis le manifest.`);
  lines.push("> Manifest source : [`src/manifest/assets-manifest.json`](./src/manifest/assets-manifest.json).");
  lines.push("");
  lines.push("## Politique");
  lines.push("");
  lines.push(summary);
  lines.push("");
  lines.push("Chaque entree du manifest est reportee dans le tableau ci-dessous avec nom, auteur, source, categorie, licence et modifications. Les URLs completes et le texte integral des licences restent accessibles depuis l'overlay in-app **Credits** et depuis le champ `source` du manifest.");
  lines.push("");
  lines.push("## Tableau des assets");
  lines.push("");
  lines.push("| ID | Nom | Auteur | Source | Categorie | Licence | Modifications |");
  lines.push("|---|---|---|---|---|---|---|");

  if (!Array.isArray(manifest.assets) || manifest.assets.length === 0) {
    lines.push("| _aucun_ | _aucun_ | _aucun_ | _aucun_ | _aucun_ | _aucun_ | _aucun_ |");
  } else {
    for (const entry of manifest.assets) {
      const categoryLabel =
        CATEGORY_LABELS[entry.category] ?? entry.category ?? "";
      const licenseLabel =
        LICENSE_LABELS[entry.license] ?? entry.license ?? "";
      lines.push(
        `| ${escapeCell(entry.id)} | ${escapeCell(entry.name)} | ${escapeCell(entry.author)} | ${escapeCell(entry.source)} | ${escapeCell(categoryLabel)} | ${escapeCell(licenseLabel)} | ${escapeCell(entry.modifications)} |`
      );
    }
  }

  lines.push("");
  lines.push("## Hors scope");
  lines.push("");
  lines.push("- Audit juridique externe.");
  lines.push("- Verification automatisee du respect des licences.");
  lines.push("- Generation d'un site web dedie aux credits (la note markdown et l'overlay in-app suffisent pour la V1 mock).");
  lines.push("");
  lines.push("## Liens utiles");
  lines.push("");
  lines.push("- [OpenGameArt FAQ](https://opengameart.org/content/faq)");
  lines.push("- [Poly Haven licence](https://polyhaven.com/license)");
  lines.push("- [Kenney support / licence](https://kenney.nl/support)");
  lines.push("");

  return lines.join("\n");
}

export function generateCreditsMarkdown({
  manifestPath = MANIFEST_PATH,
  outputPath = OUTPUT_PATH
} = {}) {
  const raw = JSON.parse(readFileSync(manifestPath, "utf8"));
  const markdown = buildCreditsMarkdown(raw);
  writeFileSync(outputPath, markdown, "utf8");
  return { outputPath, assetCount: Array.isArray(raw.assets) ? raw.assets.length : 0 };
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const result = generateCreditsMarkdown();
  process.stdout.write(
    `THIRD_PARTY_ASSETS.md mis a jour (${result.assetCount} asset(s)) -> ${result.outputPath}\n`
  );
}
