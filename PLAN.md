# Project 2 – FPS Treasure Hunt: Step-by-Step Plan

Env: Unity 6000.6.4f1, Input System 1.20, AI Navigation 2.0.14, uGUI 2.6. Deadline 11/5/2026.
Rule: new Input System only (no `UnityEngine.Input`), C# only, no visual scripting.

## Phase 0 – Project setup
1. Set Active Input Handling to "Input System Package (New)".
2. Create folders: Scripts/{Player,Weapons,Enemies,Pickups,UI,Core}, Prefabs, Audio, Materials, Scenes.
3. Create Input Actions asset (Move, Look, Sprint, Jump, Fire, SwitchWeapon).
4. Remove the sample Cube; delete unused packages near the end (visual scripting, timeline, etc.).

## Phase 1 – Player (15 pts: control + pickups)
5. Player prefab: CharacterController + camera child + weapon holder (gun, grenade models hidden at start).
6. `PlayerController`: walk, run (Shift), jump (Space), mouse look.
7. `PlayerHealth`: 3 lives, damage = lose 1 life, heal from health kit (max 3), game over at 0.
8. `PlayerInventory`: hasGun, ammo count, grenade count, current weapon; Q/E switches among owned weapons.

## Phase 2 – Level
9. Ground, outdoor area with cover, a house with one door opening and two rooms.
10. Place 3 treasure boxes (one inside the house), each with a protection-zone radius.
11. Bake NavMesh (NavMeshSurface); door opening must stay walkable.
12. Lighting: directional light + a few point lights inside the house.

## Phase 3 – Pickups
13. `Pickup` base (trigger): GunPickup, AmmoPickup, GrenadePickup, HealthKit, TreasureBox.
14. Gun pickup makes hidden gun model visible and gives 6 bullets; ammo pickups add bullets.

## Phase 4 – Weapons (20 pts)
15. `Gun`: left click fires a raycast/projectile, uses ammo; guard needs 3 hits.
16. `Grenade`: throw with arc, explode after delay/impact, area damage = instant kill.
17. `Guard` health: gun = 1/3 per hit, grenade = kill. Kill counter increments.

## Phase 5 – Guard AI (30 pts)
18. `GuardAI` state machine with NavMeshAgent: Patrol, Chase, Attack, Return, Flee.
19. Patrol: random point inside the protection circle, wait, repeat.
20. Chase/shoot: when player enters zone (distance check or trigger) chase and fire enemy bullets with a fire rate; return to patrol when the player leaves.
21. If its treasure was stolen: chase everywhere, ignore zone.
22. After all 3 treasures collected: all guards Flee (move away from player).
23. Guard placement: >=1 outside and >=1 inside the house; 1-2 for the others.

## Phase 6 – Game flow
24. `GameManager`: treasures collected, guards remaining, win (3 treasures + 0 guards), lose (0 lives), restart.

## Phase 7 – UI (5 pts)
25. HUD: lives, weapons collected, bullets left, kills, treasures x/3, win/lose panels, crosshair.

## Phase 8 – Audio and VFX (5 pts + bonus)
26. SFX: gun shot, grenade throw, explosion, guard hit, player hit, treasure, background music, footsteps (optional).
27. VFX (optional): muzzle flash, explosion particles, hit sparks.

## Phase 9 – Polish and test
28. Playtest all requirements against the PDF checklist; fix bugs.

## Phase 10 – Deliverables
29. Windows standalone build (File > Build Profiles), test the whole folder, zip.
30. Record gameplay video (one win, one loss) and narrated code walkthrough (<=15 min; use guard chase interaction).
31. Clean unused packages/assets, zip the project, upload to iCollege > Project 2.
