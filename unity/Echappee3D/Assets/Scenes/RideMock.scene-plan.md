# RideMock.unity - Plan de scene

MYB-11 commit `Assets/Scenes/RideMock.unity` generee par Unity `6000.4.10f1`
via `Assets/Echappee/Editor/RideMockSceneBuilder.cs`.

La scene contient:

- `RideSession`
  - `RideSessionController`
  - references vers camera, route renderer, fog, HUD et input mock
- `Main Camera`
  - tag `MainCamera`
  - `RideCameraController`
- `Route`
  - `RouteRendererPlaceholder`
  - `LineRenderer`
  - route mock avec elevation simple controlee
  - materiau opaque simple sous `Assets/Materials/`
- `Fog`
  - `DepthFogController`
- `Canvas`
  - slider effort 0..1 branche sur `MockRideInput`
  - textes vitesse, distance, temps, source `mock`, phase branches sur
    `HudController`

Le rendu vise seulement une route stable visible, une camera rail simple qui suit
les montees/descentes, un HUD minimal et le brouillard/profondeur preserve du
playtest.
