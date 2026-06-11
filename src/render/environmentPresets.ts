import { clampProgress01 } from "../route/routeMath";
import type { Vec3 } from "../route";

export type EnvironmentPresetId =
  | "morning"
  | "goldenHour"
  | "softNight"
  | "lightRain"
  | "lowPerformance";

export type EnvironmentWeatherKind = "none" | "rain" | "fog";

export type EnvironmentQuality = "low" | "high";

export type EnvironmentPreset = {
  id: EnvironmentPresetId;
  label: string;
  description: string;
  quality: EnvironmentQuality;
  weather: EnvironmentWeatherKind;
  shadows: boolean;
  weatherDensity: number;
  sky: number;
  fog: number;
  fogNear: number;
  fogFar: number;
  ambientColor: number;
  ambientIntensity: number;
  sunColor: number;
  sunIntensity: number;
  sunPosition: Vec3;
  horizon: number;
  horizonOpacity: number;
  preferredBiomes: string[];
  weatherParticleColor: number;
  weatherParticleOpacity: number;
};

export type EnvironmentPresetSummary = {
  id: EnvironmentPresetId;
  label: string;
  description: string;
  weather: EnvironmentWeatherKind;
  quality: EnvironmentQuality;
  preferredBiomes: string[];
};

export const ENVIRONMENT_PRESET_IDS: ReadonlyArray<EnvironmentPresetId> = [
  "morning",
  "goldenHour",
  "softNight",
  "lightRain",
  "lowPerformance"
];

const PRESETS: Record<EnvironmentPresetId, EnvironmentPreset> = {
  morning: {
    id: "morning",
    label: "Matin",
    description: "Ciel clair, lumiere douce, fog leger.",
    quality: "high",
    weather: "none",
    shadows: false,
    weatherDensity: 0,
    sky: 0xc7dcef,
    fog: 0xdfeaf2,
    fogNear: 60,
    fogFar: 520,
    ambientColor: 0xfdf6e6,
    ambientIntensity: 0.78,
    sunColor: 0xfff2d4,
    sunIntensity: 1.18,
    sunPosition: { x: -22, y: 44, z: -22 },
    horizon: 0x9bbfd6,
    horizonOpacity: 0.52,
    preferredBiomes: ["coast"],
    weatherParticleColor: 0xd0e1f0,
    weatherParticleOpacity: 0.18
  },
  goldenHour: {
    id: "goldenHour",
    label: "Golden hour",
    description: "Lumiere chaude, ombres longues, fog dore.",
    quality: "high",
    weather: "none",
    shadows: true,
    weatherDensity: 0,
    sky: 0xf2b46b,
    fog: 0xf5cd92,
    fogNear: 18,
    fogFar: 360,
    ambientColor: 0xffd8a1,
    ambientIntensity: 0.86,
    sunColor: 0xffc878,
    sunIntensity: 1.42,
    sunPosition: { x: -32, y: 18, z: -8 },
    horizon: 0xdd8a4b,
    horizonOpacity: 0.62,
    preferredBiomes: ["coast"],
    weatherParticleColor: 0xffe3a8,
    weatherParticleOpacity: 0.18
  },
  softNight: {
    id: "softNight",
    label: "Nuit douce",
    description: "Ciel profond, fog dense, lumiere lunaire lisible.",
    quality: "high",
    weather: "none",
    shadows: false,
    weatherDensity: 0,
    sky: 0x16243a,
    fog: 0x1d2f47,
    fogNear: 12,
    fogFar: 200,
    ambientColor: 0x9ab4d6,
    ambientIntensity: 0.62,
    sunColor: 0xc6dcff,
    sunIntensity: 0.7,
    sunPosition: { x: 18, y: 38, z: -28 },
    horizon: 0x2b3f5d,
    horizonOpacity: 0.78,
    preferredBiomes: ["forest"],
    weatherParticleColor: 0xc8dcf4,
    weatherParticleOpacity: 0.18
  },
  lightRain: {
    id: "lightRain",
    label: "Pluie legere",
    description: "Ciel gris, fog mouille, particules de pluie discretes.",
    quality: "high",
    weather: "rain",
    shadows: false,
    weatherDensity: 0.55,
    sky: 0x8a96a3,
    fog: 0x9aa6b3,
    fogNear: 8,
    fogFar: 160,
    ambientColor: 0xc8d0d8,
    ambientIntensity: 0.6,
    sunColor: 0xe6ecf2,
    sunIntensity: 0.72,
    sunPosition: { x: -8, y: 36, z: -22 },
    horizon: 0x6c7682,
    horizonOpacity: 0.7,
    preferredBiomes: ["forest", "coast"],
    weatherParticleColor: 0xc8d6e2,
    weatherParticleOpacity: 0.32
  },
  lowPerformance: {
    id: "lowPerformance",
    label: "Performance",
    description: "Preset leger sans particules ni ombres.",
    quality: "low",
    weather: "none",
    shadows: false,
    weatherDensity: 0,
    sky: 0xc8d4dc,
    fog: 0xd4dce2,
    fogNear: 22,
    fogFar: 320,
    ambientColor: 0xffffff,
    ambientIntensity: 0.72,
    sunColor: 0xffffff,
    sunIntensity: 1.0,
    sunPosition: { x: -14, y: 38, z: -18 },
    horizon: 0x9faab3,
    horizonOpacity: 0.5,
    preferredBiomes: ["coast", "forest"],
    weatherParticleColor: 0xeef4fa,
    weatherParticleOpacity: 0.1
  }
};

const FALLBACK_PRESET: EnvironmentPresetId = "morning";

export function getEnvironmentPreset(
  idInput: string | null | undefined
): EnvironmentPreset {
  if (idInput !== null && idInput !== undefined) {
    const match = PRESETS[idInput as EnvironmentPresetId];
    if (match !== undefined) {
      return match;
    }
  }

  return PRESETS[FALLBACK_PRESET];
}

export function listEnvironmentPresets(): EnvironmentPresetSummary[] {
  return ENVIRONMENT_PRESET_IDS.map((id) => {
    const preset = PRESETS[id];
    return {
      id: preset.id,
      label: preset.label,
      description: preset.description,
      weather: preset.weather,
      quality: preset.quality,
      preferredBiomes: [...preset.preferredBiomes]
    };
  });
}

export function getDefaultPresetForBiome(
  biomeId: string
): EnvironmentPresetId {
  for (const id of ENVIRONMENT_PRESET_IDS) {
    const preset = PRESETS[id];
    if (preset.preferredBiomes.includes(biomeId)) {
      return preset.id;
    }
  }

  return FALLBACK_PRESET;
}

export type EnvironmentAutoCycle = {
  presetId: EnvironmentPresetId;
  startProgress01: number;
  endProgress01: number;
};

export function buildAutoPresetCycle(
  stepsInput?: number
): EnvironmentAutoCycle[] {
  const cycle: EnvironmentPresetId[] = [
    "morning",
    "goldenHour",
    "softNight",
    "lightRain"
  ];

  const safeSteps =
    stepsInput !== undefined && Number.isFinite(stepsInput) && stepsInput > 0
      ? Math.min(Math.floor(stepsInput), cycle.length)
      : cycle.length;

  const sliced = cycle.slice(0, safeSteps);
  const out: EnvironmentAutoCycle[] = [];

  for (let index = 0; index < sliced.length; index += 1) {
    const startProgress01 = sliced.length === 1 ? 0 : index / sliced.length;
    const endProgress01 =
      sliced.length === 1 ? 1 : (index + 1) / sliced.length;

    out.push({
      presetId: sliced[index],
      startProgress01,
      endProgress01
    });
  }

  return out;
}

export function selectAutoPreset(
  cycle: EnvironmentAutoCycle[],
  progress01: number
): EnvironmentPresetId {
  if (cycle.length === 0) {
    return FALLBACK_PRESET;
  }

  const safeProgress = clampProgress01(progress01);

  const match = cycle.find(
    (entry) =>
      safeProgress >= entry.startProgress01 &&
      safeProgress < entry.endProgress01
  );

  if (match !== undefined) {
    return match.presetId;
  }

  return cycle[cycle.length - 1].presetId;
}

export function isWeatherEnabled(
  preset: EnvironmentPreset,
  weatherToggle: boolean
): boolean {
  if (!weatherToggle) {
    return false;
  }

  return preset.weather !== "none" && preset.weatherDensity > 0;
}

export function isValidEnvironmentPresetId(
  value: string | null | undefined
): value is EnvironmentPresetId {
  if (value === null || value === undefined) {
    return false;
  }

  return ENVIRONMENT_PRESET_IDS.includes(value as EnvironmentPresetId);
}
