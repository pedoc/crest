# Unity 6000.6 compatibility branch

This is the `unity-6000.6` branch of [pedoc/crest](https://github.com/pedoc/crest), forked from [wave-harmonic/crest](https://github.com/wave-harmonic/crest) at `db0658ff0b2e93e4a9e28cc2867509658b0ecc00`.

## Scope

- Target Editor: **Unity 6000.6.1f1**. Open the `crest/` directory as the Unity project.
- Render pipeline: **Built-in**, as in the upstream open-source repository. This branch is **not a URP/HDRP port**, and does not include those commercial packages.
- The original MIT license and upstream attribution are retained.
- Upstream `master` is kept for synchronization. Compatibility changes live on this branch.

## Dependency migration

The upstream project pins Unity 2022.3-era packages. Some cannot compile or resolve on Unity 6000.6. This branch upgrades the affected packages rather than deleting their Crest integrations:

- Input System 1.20.1; Splines 2.9.1; Post-processing 3.5.4.
- AI Navigation 2.0.16; XR Management 4.6.1.
- XR Legacy Input Helpers **3.0.1**: the 2.x releases still depend on the retired `com.unity.modules.vr`, which Unity 6000.6 no longer ships. Modern `com.unity.modules.xr` remains enabled.
- Visual Studio integration 2.0.28.
- Unity 6000.6 supplies editor-bundled Burst 2.0.0, Mathematics 1.4.0, Cinemachine 6.6.0, Timeline 6.6.0, uGUI 2.6.0, and Test Framework 1.8.0. The resolver substitutes these for older requested pins. This branch explicitly records the actually compiled versions; it does not claim Cinemachine 2.x remained the resolved package. Original example camera data is not manually rewritten by this change.

See [dependency-upgrades.json](dependency-upgrades.json) for the exact pins. `crest/Packages/packages-lock.json` records the resolved dependency graph. Optional Crest asmdef references/version defines, Gaia integration constraints, and original runtime code are preserved unless a specific source fix is documented.

## Reproduce the Windows smoke test

A licensed local Unity Editor installation is required. Run from the repository root:

```powershell
.\compatibility\verify-unity6000.ps1
# Or build without opening the render window:
.\compatibility\verify-unity6000.ps1 -BuildOnly
# Override the Editor installation path when necessary:
.\compatibility\verify-unity6000.ps1 -UnityEditor 'D:\Unity\6000.6.1f1\Editor\Unity.exe'
```

The command compiles the project, creates a small compatibility scene, builds a Windows D3D11 player, and opens an auto-exiting render test. The generated scene/material/TerrainData stay under an ignored `Assets/Compatibility/Generated/` directory; builds and Editor caches are not committed.

The render test instantiates upstream `OceanRenderer`, `ShapeFFT`, and `OceanDepthCache`, performs real GPU wave-height queries, renders the actual camera into a render target for a screenshot (independent of window splash/occlusion), and checks runtime errors. Evidence is saved in `compatibility/evidence/`. A serialized Terrain dependency keeps the built-in terrain shader from being stripped in a runtime-generated scene.

## Verification boundaries

A successful full-project import/build demonstrates package and source compilation, not exhaustive runtime coverage of every integration. The small render smoke covers the core Built-in FFT/depth/terrain path. The evidence report must be read for the exact Editor, platform, observations, and result.

Not certified by this test: every example scene, XR hardware, Gaia, underwater stereo, every Spline/input/post-processing behavior, macOS/Linux/mobile, console platforms, long-run memory/power, or all Unity 6000.x releases. No Unity license or API credentials are committed. A package being retained or compiled is not a claim that every associated feature has been exercised.

## Upstream synchronization

Keep `upstream` pointing to the original repository and `origin` to this fork. Merge or rebase upstream deliberately; rerun the smoke after dependency/runtime changes. Do not silently replace this Built-in branch with a different render pipeline.
