# Video notes (you must narrate in your own voice)

## Gameplay video checklist (one win + one loss)
Controls: WASD/arrows move, Shift run, Space jump, mouse look, LMB fire, Q/E switch weapon, R restart after game over.

1. Start: no weapons, HUD shows "Weapons: none". Walk over the black cube (gun, 6 bullets) and the green spheres (grenades), yellow cubes (ammo), red cubes (health kit, only used if a life is missing).
2. Show Q/E switching and the gun/grenade models appearing.
3. Enter a protection zone: the guard stops patrolling, chases, shoots.  Leave the zone: it goes back to patrol.
4. Kill one guard with 3 gun hits, another with 1 grenade. Kills counter goes up.
5. House: enter through the door (front, south side). One guard outside, one inside. Take the treasure.
6. Steal a treasure, run far away: its guard keeps chasing outside the zone.
7. WIN run: all 3 treasures, then hunt the guards (they flee). "YOU WIN!"
8. LOSS run: stand in a zone and let 3 hits land. "GAME OVER". Press R.

## Code walkthrough (max 15 min) - what each script is and where it is attached
| Script | Attached to | One-line job |
|---|---|---|
| PlayerController | Player | WASD/Shift/Space/mouse via new Input System, CharacterController |
| PlayerHealth | Player | 3 lives, hit grace period, Heal() for kits, Died event |
| PlayerWeapons | Player | owns ammo/grenades, Q/E switch, fires gun raycast, throws grenade |
| PlayerAudio | Player | footsteps + player-hit sound |
| GrenadeProjectile | Grenade prefab | fuse, OverlapSphere blast, calls OnGrenadeHit |
| Pickup | each pickup object | trigger; gun/ammo/grenade/health kit |
| TreasureBox | Treasure A / B / C (House) roots | protection-zone trigger (PlayerInside), Stolen flag |
| TreasurePickup | Chest child of each treasure | steals the treasure on touch |
| Guard | each guard under Guards | NavMeshAgent state machine: Patrol / Chase / Flee / Dead |
| EnemyBullet | EnemyBullet prefab | moves straight, costs the player one life |
| GameManager | GameManager | treasure/kill counts, win/lose, R to restart |
| GameHUD | GameManager | builds the uGUI HUD in code |
| MusicPlayer | GameManager | looping music |
| Vfx | GameManager | explosion + spark particles built in code |
| Sfx.cs | (static helper + IShootable interface) | null-safe PlayClipAtPoint |

## One complete interaction to walk through (guard chases and shoots)
1. Player walks into the zone sphere -> `TreasureBox.OnTriggerEnter` (TreasureBox.cs) sets `PlayerInside = true`. (`OnTriggerExit` clears it.)
2. Every frame `Guard.Update` -> `ChooseState()` reads `home.PlayerInside`; returns Chase, `SetState(State.Chase)` raises agent speed.
3. `UpdateChase()` calls `agent.SetDestination(player.position)` every 0.25 s (NavMesh path).
4. When within `attackRange` and `HasLineOfSight()` (raycast) is true the agent stops, turns to face the player and calls `TryShoot()`.
5. `TryShoot()` instantiates `EnemyBullet` and calls `Fire(direction)`.
6. `EnemyBullet.OnTriggerEnter` finds `PlayerHealth` -> `TakeHit()`: lives--, fires `Hit` (PlayerAudio plays the sound) and `LivesChanged` (GameHUD refreshes). At 0 lives `Died` -> `GameManager.EndGame(false)` -> GameOver panel.
7. Player leaves the zone -> `OnTriggerExit` -> `PlayerInside = false` -> `ChooseState()` returns Patrol -> `UpdatePatrol` picks random NavMesh points inside the zone.

Mention also: where the NavMesh is baked (NavMesh object, NavMeshSurface component, collects the Level children), and where input actions are created (`PlayerController.Awake`, `PlayerWeapons.Awake`).

## Be ready to explain
- Why the gun uses a camera raycast (hitscan) but grenades use physics + OverlapSphere.
- Why EnemyBullet has a kinematic Rigidbody (trigger events need one).
- Why `PlayerHealth` has a 1 s grace period.
- After all treasures are collected `GameManager.AllTreasuresCollected` makes every guard choose Flee.
