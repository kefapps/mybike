import type { RouteDefinition } from "./routeTypes";

export const mockRouteDefinition: RouteDefinition = {
  id: "echappee-mock-route",
  lengthMeters: 1000,
  points: [
    { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
    { distanceMeters: 150, position: { x: 24, y: 5, z: 148 } },
    { distanceMeters: 320, position: { x: -18, y: 2, z: 315 } },
    { distanceMeters: 500, position: { x: 36, y: 10, z: 496 } },
    { distanceMeters: 700, position: { x: 4, y: 4, z: 698 } },
    { distanceMeters: 850, position: { x: -30, y: 8, z: 845 } },
    { distanceMeters: 1000, position: { x: 0, y: 3, z: 1000 } }
  ],
  biomes: [
    { id: "coast", fromProgress01: 0, toProgress01: 0.01 },
    { id: "forest", fromProgress01: 0.01, toProgress01: 1 }
  ],
  segments: [
    {
      id: "warmup",
      kind: "warmup",
      fromProgress01: 0,
      toProgress01: 0.15,
      targetEffort01: 0.3
    },
    {
      id: "climb",
      kind: "climb",
      fromProgress01: 0.15,
      toProgress01: 0.45,
      targetEffort01: 0.7
    },
    {
      id: "sprint",
      kind: "sprint",
      fromProgress01: 0.45,
      toProgress01: 0.55,
      targetEffort01: 0.95
    },
    {
      id: "recovery",
      kind: "recovery",
      fromProgress01: 0.55,
      toProgress01: 0.85,
      targetEffort01: 0.4
    },
    {
      id: "cooldown",
      kind: "cooldown",
      fromProgress01: 0.85,
      toProgress01: 1,
      targetEffort01: 0.25
    }
  ]
,  profile: {
    elevationPoints: [
      { distanceMeters: 0, elevationMeters: 0 },
      { distanceMeters: 150, elevationMeters: 5 },
      { distanceMeters: 320, elevationMeters: 2 },
      { distanceMeters: 500, elevationMeters: 10 },
      { distanceMeters: 700, elevationMeters: 4 },
      { distanceMeters: 850, elevationMeters: 8 },
      { distanceMeters: 1000, elevationMeters: 3 }
    ],
    difficultySegments: [
      { fromProgress01: 0, toProgress01: 0.15, segmentType: "flat" },
      { fromProgress01: 0.15, toProgress01: 0.5, segmentType: "climb" },
      { fromProgress01: 0.5, toProgress01: 0.7, segmentType: "descent" },
      { fromProgress01: 0.7, toProgress01: 0.85, segmentType: "sprint" },
      { fromProgress01: 0.85, toProgress01: 1, segmentType: "recovery" }
    ]
  }
};