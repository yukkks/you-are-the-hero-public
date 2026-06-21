# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

**"You Are The Hero"** — a Unity 6 (URP, Linear) third-person narrative game built as a *farewell gift* for a colleague. The player is the gift recipient (currently a grey capsule, swap-ready for a real Avaturn avatar). The audience is a **non-gamer playing solo**, so controls must stay dead simple.

**True scope is ONE room only** — the IQOS Lounge. There are no trials, minigames, hub, or scene-hopping; anything suggesting that is bloat inherited from a previous game and was archived. Core loop: spawn in the lounge → walk up to 7 NPC colleagues for personal dialogue → click pictures on the wall to unveil them → when all are done, a gathering finale cutscene plays.

## Unity / editing workflow

- Unity **6000.3.10f1**, URP, Linear color space. WebGL is the build target.
- This project is edited primarily via the **Coplay MCP** server (`com.coplaydev.coplay` git package). Prefer Coplay tools (scene/object/material/animation ops, play-mode verification) over hand-editing `.unity`/`.meta` files, which are large generated YAML.
- After scene/script changes, verify in play mode (Coplay can auto-run a playthrough) and watch for compile errors. NavMesh must be **re-baked** after moving furniture/colliders.

## The live scene

The single playable scene is **`Assets/JapaneseGarden.unity`** (despite the name, it *is* the IQOS Lounge — it was relocated out of a vendor asset folder; Build Settings point only here). `Assets/Scenes/` and `_ARCHIVE_OUTSIDE_ASSETS/` hold archived/old scenes — not live.

## Code architecture (`Assets/Scripts/`)

Interaction is unified through one interface and one progress tracker — understand these three together before changing gameplay:

- **`IInteractable`** — both `NPCDialogue` and `PhotoFrameInteract` implement it. Clicking an interactable walks the player to it via `ClickToMove`, then engages.
- **`GameProgress`** — the spine. NPCs and photos register themselves; it marks greeted/viewed and fires `OnAllComplete` when everything is done. UI scripts (`DialogueUI`, `PhotoViewerUI`, `ProgressNudgeUI`, `FinaleController`) subscribe in `Start()` — no manual editor event wiring.
- **`ClickToMove`** — movement via `NavMeshAgent`. Click-to-move is the real control scheme (for the non-gamer recipient); keyboard/WASD is also enabled for desktop testing (`ClickToMove.enableKeyboard`). The player root has the NavMeshAgent + ClickToMove at the feet pivot; `HeroVisual` child is the swappable capsule (to use Avaturn: delete HeroVisual, drop avatar at local 0, assign its Animator to `ClickToMove.characterAnimator`).

`PlayerMovement.cs` is **unused** (superseded by ClickToMove) but still present — don't wire to it.

The finale (`FinaleController`) gathers the 7 colleagues to `NPC_Point01-07` waypoints, then shows the closing message. It is a **skeleton** — needs Timeline polish, real animations/camera move, and the real closing message.

## Editor utility scripts (project root, not in Assets)

Kept for working on an enclosed interior that defeats auto-framing:
- **`TopView.cs`** — `Hide` disables ceiling renderers + sets a top-down scene view for clean floor-plan captures (then call capture with no path); `Show` restores.
- **`FrontView.cs`** — interior view facing the front windows.
- **`PrepWebGL.cs`**, **`ScanLargeFiles.cs`**, **`SimulateInteractions.cs`** — WebGL prep, large-asset audit, and scripted interaction playthrough.

General pattern for an interior shot: set `SceneView.lastActiveSceneView` pivot/rotation/size, keep camera y < 16.5 (below the ceiling) using a small size.

## Gotchas (these have bitten before)

- **Magenta trees:** Waldemarst/Broccoli/Polytope trees use custom shaders that render **magenta in URP** and may fail in WebGL. Fix = override their renderer materials with URP/Lit (done for the centre pine; the corner maple may still need it).
- **Group transforms at origin:** many furniture sub-groups have their group transform at (0,0,0) with children at *world* coords. Scaling/moving the group transform is then a no-op. To move such a group rigidly, use the pivot trick (parent under a temp pivot at the renderer-bounds center, move/rotate the pivot, unparent). Props don't follow their parent furniture — move them separately.
- **3D TMP text** on signs viewed from the room side needs `localScale.x = -1` to un-mirror.
- **Negative-scale walls** (RightWall scale ≈ -0.2) produce harmless "BoxCollider negative scale" warnings.

## WebGL performance (target = web app)

Biggest risks, in order: (1) 7 realistic ~10–13k-poly Avaturn avatars with large textures (load size/memory) — compress/downsize textures, add LODs, limit how many animate at once; (2) realtime lights+shadows from skinned characters — **bake lighting**, disable character shadow-casting, disable Animators when idle; (3) the magenta custom-shader trees; (4) strip unused vendor assets (Cozy/Polytope/TMP samples) and enable build compression; (5) mark environment Static + bake occlusion/static batching. The project's own scripts (NavMesh, UGUI, click-to-move) are WebGL-safe.

## Still TODO / content needed from owner

Gameplay: finale polish, baked lighting + post-processing, perf pass. Content the owner must supply: the **4 distinct wall photos**, each colleague's **real dialogue lines** (currently placeholder `[Name's message goes here]`), and the **cutscene vision**.
