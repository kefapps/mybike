# Mock Route

Route de reference Unity ajustee pour MYB-13:

- longueur: 1000 m
- points: `(0,0,0)`, `(24,3,148)`, `(-18,12,315)`, `(36,7,496)`,
  `(4,15,698)`, `(-30,6,845)`, `(0,0,1000)`
- biomes: `coast` de 0 a 0.01, `forest` de 0.01 a 1

Le code expose `RouteMath.CreateDefaultMockRoute()` pour fournir une route mock
avec montee et descente simples, sans generation terrain riche ni asset externe.
