---
name: webgl-prep
description: Prepare the "You Are The Hero" Unity game for a WebGL build — audit large assets, cap/compress textures, apply lean WebGL player settings, and walk the performance checklist (bake lighting, disable NPC shadows, fix magenta trees, mark statics). Use when building for WebGL/web, shrinking load size or memory, or prepping a web deploy of the game.
---

# WebGL build prep for "You Are The Hero"

A repeatable pass to get the game ready for a WebGL/web deploy. The audience is a non-gamer on a browser, so load size, memory, and a clean first load matter more than max fidelity. Work in the live scene `Assets/JapaneseGarden.unity` (it IS the IQOS Lounge).

All editor work goes through the **Coplay MCP** server. Drive the project's own editor scripts via `execute_script`; don't hand-edit `.meta`/`.unity` YAML.

## Procedure

Do these in order. Report results after each step; pause if a step needs a human decision (e.g. which assets to strip).

### 1. Audit footprint first (always start here)
Run `ScanLargeFiles.Execute()` via `execute_script` and report the output. This tells you the committed size and whether any file is over 50 MB (>100 MB = GitHub rejects → Git LFS needed). Use the result to decide how aggressive the rest of the pass should be.

### 2. Cap & compress textures + set lean WebGL options
Run `PrepWebGL.Execute()` via `execute_script`. This caps every texture in `Assets/` to 512 px + Compressed, and sets: Gzip compression, **decompression fallback ON** (required for static hosts like Vercel without custom headers), exceptions OFF, strip engine code, High managed stripping, data caching. Report how many textures were capped.

### 3. Verify it still compiles
Run `check_compile_errors`. Stop and fix before continuing if there are errors.

### 4. Lighting — bake, don't run realtime
Realtime lights + shadows from 7 skinned avatars are the #2 cost. Confirm scene lighting is **baked** (not realtime), and that NPC characters are **not casting realtime shadows**. Bake lighting if stale. (Lightmap bake is the slow step — flag it to the user before kicking it off.)

### 5. NPC / avatar perf
The 7 Avaturn avatars (~10–13k poly, large textures) are the #1 risk:
- Ensure avatar textures got caught by step 2 (they live under `Assets/Characters`).
- Add LODs if not present; limit how many avatars animate at once.
- Disable Animators on idle NPCs where possible.

### 6. Fix magenta custom-shader trees
Waldemarst/Broccoli/Polytope trees use custom shaders that render **magenta in URP** and can fail in WebGL. Verify any in-scene trees have renderer materials overridden to URP/Lit (centre pine is done; check the corner maple and any others). Fix any that are still on the broken shader.

### 7. Mark environment Static + batching
Mark non-moving lounge geometry **Static** so occlusion culling / static batching apply. Bake occlusion culling.

### 8. Strip unused vendor assets
Cozy / Polytope / TextMesh Pro sample content and other unused vendor assets bloat the build. Identify clearly-unused folders and propose removal to the user before deleting (these are vendor imports, not our code).

### 9. Re-verify and summarize
Run `check_compile_errors` again, optionally re-run `ScanLargeFiles.Execute()` to show the new footprint, and give the user a short before/after summary plus any remaining manual decisions (LFS? which assets to strip? lightmap bake needed?).

## Notes
- The project scripts this skill relies on live at the repo root: `PrepWebGL.cs`, `ScanLargeFiles.cs` (also `TopView.cs` / `FrontView.cs` for interior captures if you need to visually verify).
- This pass is reversible at the texture/import level via git, but a lightmap bake and asset deletions are not — confirm before those.
- Target host is a static deploy (e.g. Vercel), which is why decompression fallback stays ON.
