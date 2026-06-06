import type { RouteDefinition } from "./routeTypes";

export const mockRouteDefinition: RouteDefinition = {
  id: "echappee-mock-route",
  lengthMeters: 1000,
  points: [
    { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
    { distanceMeters: 150, position: { x: 24, y: 1, z: 148 } },
    { distanceMeters: 320, position: { x: -18, y: 0, z: 315 } },
    { distanceMeters: 500, position: { x: 36, y: 2, z: 496 } },
    { distanceMeters: 700, position: { x: 4, y: 1, z: 698 } },
    { distanceMeters: 850, position: { x: -30, y: 0, z: 845 } },
    { distanceMeters: 1000, position: { x: 0, y: 1, z: 1000 } }
  ],
  biomes: [
    { id: "coast", fromProgress01: 0, toProgress01: 0.01 },
    { id: "forest", fromProgress01: 0.01, toProgress01: 1 }
  ]
};
