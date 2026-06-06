import type { RideFrameSnapshot, RideProgress } from "../ride";

export type Vec3 = {
  x: number;
  y: number;
  z: number;
};

export type RoutePoint = {
  distanceMeters: number;
  position: Vec3;
};

export type BiomeSegment = {
  id: string;
  fromProgress01: number;
  toProgress01: number;
};

export type RouteDefinition = {
  id: string;
  lengthMeters: number;
  points: RoutePoint[];
  biomes: BiomeSegment[];
};

export type RouteFallbackReason =
  | "missing-route"
  | "invalid-length"
  | "invalid-points"
  | "invalid-biomes";

export type RouteResolution = {
  route: RouteDefinition;
  usedFallback: boolean;
  reason?: RouteFallbackReason;
};

export type RouteSample = {
  routeId: string;
  progress01: number;
  distanceMeters: number;
  position: Vec3;
  direction: Vec3;
  lookAheadPosition: Vec3;
  segmentIndex: number;
  usedFallback: boolean;
  fallbackReason?: RouteFallbackReason;
};

export type CameraRailConfig = {
  heightMeters: number;
  lookAheadMeters: number;
  fovDegrees: number;
};

export type CameraRigSnapshot = {
  position: Vec3;
  lookAt: Vec3;
  fovDegrees: number;
  progress01: number;
  usedFallback: boolean;
  fallbackReason?: RouteFallbackReason;
};

export type RouteProgressInput = number | RideProgress | RideFrameSnapshot;

export type RouteFrameSnapshot = {
  sample: RouteSample;
  camera: CameraRigSnapshot;
  biomeId: string;
  usedFallback: boolean;
  fallbackReason?: RouteFallbackReason;
};
