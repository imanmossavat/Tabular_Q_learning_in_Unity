# GridLearn

A small Unity project where an agent learns to find its way across a grid by trial and error (Q-learning).
You give the grid your own story and look, then watch the agent learn it, or play it yourself.

> Status: working prototype. The Core logic, Unity scene, HUD and extras are implemented. See `docs/DESIGN.md` for the specification.

## The idea

For the computer, the game is a grid of numbers. Each kind of tile gives a reward:
a goal is good, a hazard is bad, mud costs a little. The agent does not know what these things *mean*.
It only tries to get a higher score. You decide the meaning: a drone avoiding toxic spills, a mouse looking for cheese, a knight crossing a trap room.

## What you can do with it

1. **Run it** and watch the agent learn.
2. **Reskin it:** change names, sprites and colours in a Theme asset.
3. **Change the rules:** edit the level file and the rewards. Does the agent still learn?
4. **Inspect the policy:** press **Show Policy** to see the best action the agent learned for every cell.
5. **Save and load** the learned Q-table, or **export** the episode history to a CSV file.
6. **Test and explain:** measure how fast it learns, and explain why.

## Quick start

1. Open the project in Unity 6000.x or later (2D template).
2. Open `Assets/Scenes/GridLearn.unity`.
3. Select the **Game** tab and pick **1280×720** or **Free Aspect**.
4. Press **Play**.
5. Use the top HUD buttons:
   - **Train** – watch the agent learn one episode at a time.
   - **Watch** – watch the agent follow its current best policy.
   - **Play** – control the agent with the arrow keys.
   - **Reset** – clear the learned Q-table and start over.
   - **Fast** – run 100 training episodes instantly, then show the policy.
   - **Show / Hide Policy** – toggle the arrow overlay.
   - **Save / Load** – save the Q-table to disk or restore it.
   - **CSV** – export the episode records to `GridLearnEpisodes.csv` in the persistent data path (logged to the Console).

Levels are plain text files (see `Assets/Levels/level01.txt`):

| Symbol | Meaning |
|---|---|
| `#` | wall |
| `.` | empty |
| `S` | start |
| `G` | goal (episode ends, big reward) |
| `H` | hazard (episode ends, big penalty) |
| `m` | mud (costs extra, episode continues) |

## Changing the level

1. Edit `Assets/Levels/level01.txt` (or create a new `.txt` file in `Assets/Levels/`).
2. Make sure there is exactly one `S`, one `G`, all rows have the same length, and a path exists from `S` to `G` through non-wall cells.
3. Select the **GameController** in the scene and drag the new level TextAsset into the **Level Text** field.
4. Press **Play**.

## Changing the theme

1. Select `Assets/Themes/DefaultTheme.asset`.
2. Change colours or assign your own sprites.
3. Press **Play**; the grid and agent use the new look immediately.

## Running tests

- **EditMode tests** (Core logic): open `Window > General > Test Runner`, choose **EditMode**, then **Run All**.
- **PlayMode test** (scene loads and trains): choose **PlayMode** in the Test Runner, then **Run All**.
- The tests can also be run from the command line with `Unity -batchmode -runTests -testPlatform EditMode` (or `PlayMode`).

## Project layout

```
Assets/Scripts/Core/       the world and the learning (plain C#, no Unity code)
Assets/Scripts/Unity/      showing it on screen, buttons, player control
Assets/Tests/EditMode/     automatic tests for Core logic
Assets/Tests/PlayMode/     automatic test for the scene
Assets/Themes/             sprites, names and colours per tile type
Assets/Levels/             level text files
docs/DESIGN.md             the design
docs/TESTING.md            short manual testing checklist
docs/ORIGIN.md             where the code came from
AGENTS.md                  instructions for the AI coding agent
```

## Licence and credits

Licence: MIT (see `LICENSE`). Copyright (c) 2026 Iman Mossavat.

The learning method is tabular Q-learning (Watkins, 1989; Sutton & Barto, *Reinforcement Learning: An Introduction*).
The idea of a Unity grid world for teaching it was inspired by Unity Technologies' Q-GridWorld demo (2017).
No code from that project is copied. The code is written independently from our own design; see `docs/ORIGIN.md`.
