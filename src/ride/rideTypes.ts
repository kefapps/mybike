export type RideInputSourceKind = "mock";

export type RideInputSample = {
  source: RideInputSourceKind;
  effort01: number;
  nowMs: number;
};

export type RideInputSource = {
  readonly kind: RideInputSourceKind;
  read(nowMs: number): RideInputSample;
};

export type MockRideInputSource = RideInputSource & {
  readonly kind: "mock";
  getEffort01(): number;
  setEffort01(effort01: number): void;
};

export type SpeedMappingConfig = {
  minSpeedMps: number;
  maxSpeedMps: number;
};

export type SpeedSmoothingConfig = {
  accelerationPerSecond: number;
  decelerationPerSecond: number;
};

export type SmoothedSpeedState = {
  speedMps: number;
};

export type RideProgress = {
  distanceMeters: number;
  progress01: number;
  completed: boolean;
};

export type RideProgressConfig = {
  totalDistanceMeters: number;
};

export type RideStats = {
  elapsedMs: number;
  distanceMeters: number;
  averageSpeedMps: number;
};

export type RideFrameState = {
  startedAtMs: number;
  speed: SmoothedSpeedState;
  progress: RideProgress;
};

export type RideFrameSnapshot = {
  source: RideInputSourceKind;
  input: RideInputSample;
  targetSpeedMps: number;
  speedMps: number;
  progress: RideProgress;
  elapsedMs: number;
  stats: RideStats;
  completed: boolean;
};

export type RideFrameConfig = {
  speedMapping: SpeedMappingConfig;
  speedSmoothing: SpeedSmoothingConfig;
  progress: RideProgressConfig;
};

export type RideFrameResult = {
  state: RideFrameState;
  snapshot: RideFrameSnapshot;
};
