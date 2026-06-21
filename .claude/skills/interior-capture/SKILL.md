---
name: interior-capture
description: Capture clean screenshots of the IQOS Lounge interior in "You Are The Hero" using the TopView/FrontView helper scripts, working around Coplay's auto-framing which fails on enclosed rooms. Use to get a floor-plan or interior view, verify furniture placement, or show the user how the lounge looks.
---

# Interior capture — IQOS Lounge

`capture_scene_object` can't auto-frame an enclosed room (the ceiling/walls defeat it). This skill uses the project's purpose-built editor view scripts at the repo root.

## Procedure (via Coplay MCP `execute_script` + capture)

### Floor-plan / top-down (placement check)
1. `TopView.Hide()` — disables ceiling renderers and sets a clean top-down scene view.
2. `capture_scene_object` with **no path** (capture the current scene view, not an object).
3. `TopView.Show()` — restore the ceiling.

### Interior eye-level (look at the lounge)
1. `FrontView.Execute()` — sets an interior view facing the front windows.
2. `capture_scene_object` with no path.

### Custom angle
Write a tiny `execute_script` that sets `SceneView.lastActiveSceneView` pivot / rotation / size, then capture with no path. **Keep the camera y < 16.5** (below the ceiling) by using a small `size`, or you'll shoot through the roof.

## Notes
- Always call `TopView.Show()` after a top-down capture so you don't leave the ceiling hidden.
- Helper scripts live at the repo root: `TopView.cs`, `FrontView.cs`.
- Useful for verifying the signature centre piece (purple bench + tree), zone layout (lounge centre / bar-strip left wall / photo wall right / window seating front), and that nothing floats off a wall after a resize.
