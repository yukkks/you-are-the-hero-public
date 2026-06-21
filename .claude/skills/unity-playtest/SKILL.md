---
name: unity-playtest
description: Run an automated play-mode playthrough of "You Are The Hero" via Coplay and report progress + errors (NPCs greeted, photos viewed, IsComplete, compile/runtime errors). Use to verify gameplay after edits, regression-check the interaction spine, or confirm the scene still works.
---

# Automated playtest — You Are The Hero

Verify the core loop still works after changes, without manual clicking. The loop: greet 7 NPC colleagues → view the wall photos → `GameProgress.OnAllComplete` fires → finale.

## Procedure (all via Coplay MCP)

1. **Confirm the live scene is open** — `JapaneseGarden.unity` (the IQOS Lounge). Open it if not.

2. **Check it compiles** — run `check_compile_errors`. Stop and fix if there are errors before playing.

3. **Run the playthrough:**
   - The project ships `SimulateInteractions.cs` at the repo root for a scripted interaction pass — use it via `execute_script` if it fits the change you're testing.
   - Otherwise `play_game`, let it run, then inspect.

4. **Inspect `GameProgress` state** — confirm the spine reached completion:
   - all 7 NPCs registered + greeted,
   - all wall photos registered + viewed,
   - `IsComplete == true` / `OnAllComplete` fired,
   - `FinaleController` triggered (gather to `NPC_Point01-07` + closing message shown).

5. **Pull logs** — `get_unity_logs`; flag any runtime errors, NullRefs, or missing-collider warnings (NPCDialogue/PhotoFrameInteract need colliders to be clickable).

6. **Stop play mode** and give a short pass/fail report: greeted X/7, viewed Y/N, IsComplete, errors.

## Notes
- Interaction is unified through `IInteractable` (NPCDialogue + PhotoFrameInteract) driven by `ClickToMove`; if clicks don't register, suspect a missing/disabled collider or a NavMesh gap.
- `PlayerMovement.cs` is dead code — don't test against it; movement is `ClickToMove`.
