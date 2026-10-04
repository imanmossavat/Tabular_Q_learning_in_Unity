# GridLearn: design

## Goal
A small, readable Unity project for teaching Q-learning. A student can change the story and the rules without
touching the learning code, and can see, test and explain how the agent learns.

## Principles
- **Separate the logic from the screen.** The world and the learning live in plain C# (`Core`) with no Unity code, so they can be tested fast and without Unity.
- **One idea per file.** Short files with short comments. A student must be able to read all of `Core` in one sitting (aim: under 400 lines in total).
- **No magic numbers.** Settings live in one config object.
- **Everything repeatable.** One seed makes a training run identical every time.

## Core (plain C#)

**Level:** parsed from text (symbols in `README.md`). Reject a level with no start, no goal, uneven rows, or no path from start to goal.

**Actions:** an enum `Up, Down, Left, Right`. The number of actions comes from the enum, never a typed-in number.

**World rules**
- State = the agent's cell, `y * width + x`. State count = `width * height`.
- Moving into a wall or off the grid: the agent stays where it is.
- Reward each step: `-0.04`. Entering `G`: `+1.0`, ends the episode. Entering `H`: `-1.0`, ends the episode. Entering `m`: extra `-0.30`.
- An episode also stops after `maxSteps` (default 200). This is a **time-out, not a real ending**: the agent should still learn from the next state's value.
- API: `Reset()`, `Step(action)` giving `(nextState, reward, isTerminal, isTimeout)`.

**Agent (Q-learning)**
- Q-table `float[stateCount, actionCount]`, all zero at the start.
- Update: `Q(s,a) += alpha * (target - Q(s,a))`, where `target = reward` if terminal, else `reward + gamma * max Q(s')`.
- Defaults: `alpha 0.3`, `gamma 0.95`, epsilon from `1.0` down to `0.05` in a straight line over `400` episodes.
- Exploring = pick uniformly among **all** actions. Exploiting = pick the best action; if several tie, pick randomly among them.
- Own seeded random generator (`System.Random`), never `UnityEngine.Random`, inside Core.

**Trainer:** runs episodes, records per episode: total reward, steps, reached goal (yes/no), epsilon. Can run many steps at once (no drawing) and can export a CSV.

**Storage:** save and load the Q-table and settings as JSON.

## Unity layer

- 2D project. Sprites on a grid. **No physics and no colliders**: all rules come from Core.
- **Theme** (ScriptableObject): per tile type a sprite, a display name and a colour. Changing the theme must never need a code change.
- **Modes:** `Train` (agent learns, visible), `Watch` (agent follows what it learned, no exploring), `Play` (a person moves with the arrow keys, same rules and rewards).
- **HUD:** episode, epsilon, last reward, success rate over the last 50 episodes, speed control, buttons for Reset, Save, Load, and level choice.
- **Q view:** an optional overlay that shows the best action (arrow) and value (colour) on each cell.
- **Fast train button:** trains many episodes without drawing, then returns to the picture.
- Do not use `GameObject.Find`. Use references set in the Inspector.

## Expected behaviour on `Assets/Levels/level01.txt` (10x10)

Checked by simulating this design in Python, not yet in Unity. Treat as targets:
- The shortest path from `S` to `G` is **18 steps**.
- With the default settings the agent found it in about **200 episodes** (about 12,000 steps); across 100 random seeds the slowest took 248.
- So a test may allow up to 600 episodes and should pass for every seed it tries.

## Tests (EditMode)
1. Level parser: valid level, and each rejection case.
2. World: wall bump, edge bump, goal, hazard, mud, timeout.
3. Q update: one hand-calculated example for a terminal and a non-terminal step.
4. Exploration: over many tries, every action is chosen at least once.
5. Learning: on `level01`, for 10 seeds, the greedy path reaches `G` in 18 steps within 600 episodes.
6. Same seed gives the same training result twice.
7. Save then load gives the same Q-table.

## Later (not now)
Ideas for student challenges, to be kept in mind but not built yet: a "key" the agent must pick up first (the state must then remember it), slippery tiles, moving hazards, comparing learning settings, a second algorithm.

## Open questions
_(The agent writes here when something in the design is unclear.)_
