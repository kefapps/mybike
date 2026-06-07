# Mock Route

Route de reference transcrite depuis `src/route/mockRouteDefinition.ts`:

- longueur: 1000 m
- points: `(0,0,0)`, `(24,1,148)`, `(-18,0,315)`, `(36,2,496)`,
  `(4,1,698)`, `(-30,0,845)`, `(0,1,1000)`
- biomes: `coast` de 0 a 0.01, `forest` de 0.01 a 1

Le code MYB-11 expose `RouteMath.CreateDefaultMockRoute()` pour eviter une scene
vide si l'asset Unity n'est pas encore cree.
