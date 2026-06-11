import { describe, expect, it } from "vitest";

import {
  availableRoutes,
  DEFAULT_ROUTE_ID,
  getDefaultRouteEntry,
  getRouteDefinitionById,
  getRouteEntryById
} from "./availableRoutes";

describe("availableRoutes", () => {
  it("exposes at least three configured routes", () => {
    expect(availableRoutes.length).toBeGreaterThanOrEqual(3);
  });

  it("contains the short, climb, and zen routes with expected metadata", () => {
    const ids = availableRoutes.map((entry) => entry.id);

    expect(ids).toEqual(
      expect.arrayContaining(["echappee-mock-route", "echappee-montee", "echappee-zen"])
    );

    const climb = availableRoutes.find((entry) => entry.id === "echappee-montee");
    const zen = availableRoutes.find((entry) => entry.id === "echappee-zen");

    expect(climb?.profile).toBe("climb");
    expect(climb?.definition.lengthMeters).toBe(1500);
    expect(zen?.profile).toBe("zen");
    expect(zen?.definition.lengthMeters).toBe(600);
  });

  it("uses a valid default route id that resolves through the registry", () => {
    const entry = getDefaultRouteEntry();

    expect(entry.id).toBe(DEFAULT_ROUTE_ID);
    expect(getRouteEntryById(DEFAULT_ROUTE_ID)).toBe(entry);
    expect(getRouteDefinitionById(DEFAULT_ROUTE_ID)).toBe(entry.definition);
  });

  it("returns undefined for unknown route ids without throwing", () => {
    expect(getRouteEntryById("does-not-exist")).toBeUndefined();
    expect(getRouteDefinitionById("does-not-exist")).toBeUndefined();
  });

  it("derives the ride distance from the route file, not from a hardcoded constant", () => {
    const climb = getRouteDefinitionById("echappee-montee");
    const zen = getRouteDefinitionById("echappee-zen");

    expect(climb?.lengthMeters).not.toBe(1000);
    expect(zen?.lengthMeters).not.toBe(1000);
  });
});
