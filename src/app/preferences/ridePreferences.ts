export type RidePreferences = {
  zenMode: boolean;
};

export const DEFAULT_RIDE_PREFERENCES: RidePreferences = Object.freeze({
  zenMode: false
});

export function createInitialRidePreferences(): RidePreferences {
  return { zenMode: false };
}

export function isZenMode(preferences: RidePreferences): boolean {
  return preferences.zenMode === true;
}

export function withZenMode(
  preferences: RidePreferences,
  zenMode: boolean
): RidePreferences {
  return { ...preferences, zenMode: zenMode === true };
}

export function toggleZenMode(
  preferences: RidePreferences
): RidePreferences {
  return withZenMode(preferences, !isZenMode(preferences));
}
