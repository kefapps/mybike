import { existsSync, readFileSync } from 'node:fs';

const requiredFiles = [
  '_bmad-output/implementation-artifacts/myb-50-unity-asset-optimization-pipeline.md',
  'CONTEXT.md',
  'unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json',
];

const pipelineSections = [
  '## Pipeline Asset Unity',
  '## Provenance',
  "## Familles d'assets",
  '## Bandes techniques',
  '## Import settings Unity recommandes',
  '## Convention de rangement',
  "## Criteres d'acceptation",
  "## Validation pour les tickets d'application",
];

const contextTerms = [
  '### Pipeline Asset Unity',
  '### Asset Optimise Unity',
];

const failures = [];

for (const filePath of requiredFiles) {
  if (!existsSync(filePath)) {
    failures.push(`Missing required file: ${filePath}`);
  }
}

if (failures.length === 0) {
  const pipeline = readFileSync(requiredFiles[0], 'utf8');
  const context = readFileSync(requiredFiles[1], 'utf8');
  const manifest = readFileSync(requiredFiles[2], 'utf8');

  for (const section of pipelineSections) {
    if (!pipeline.includes(section)) {
      failures.push(`Missing MYB-50 pipeline section: ${section}`);
    }
  }

  for (const term of contextTerms) {
    if (!context.includes(term)) {
      failures.push(`Missing visual context term: ${term}`);
    }
  }

  const parsedManifest = JSON.parse(manifest);
  if (!Array.isArray(parsedManifest.assets)) {
    failures.push('ThirdPartyAssets manifest must expose an assets array.');
  }
}

if (failures.length > 0) {
  console.error('MYB-50 documentation validation failed:');
  for (const failure of failures) {
    console.error(`- ${failure}`);
  }
  process.exit(1);
}

console.log('MYB-50 documentation validation passed.');
