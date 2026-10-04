# GridLearn

**Build your own GridWorld in Unity. Have an AI agent learn to play it with tabular Q-learning.**

GridLearn is a small, visual project for learning and experimenting with Q-learning. You create the world, give it a story, and choose its rewards. The agent learns from experience.

A hazard could be a toxic spill, a monster, or a trap. A goal could be cheese, treasure, or a delivery point. The Q-learning algorithm stays the same.

> **Status:** working prototype.

## How it works

The agent repeatedly:

**state → action → reward → Q-table update**

It does not know what the objects in the world mean. It learns which actions are useful from the rewards it receives.

Because the Q-table stores values for each **state-action pair**, you can inspect what the agent has learned and see how its policy changes as it trains.

## What you can do

* **Train** the agent through repeated episodes.
* **Watch** it follow its current learned policy.
* **Play** the GridWorld yourself.
* **Change the world** by editing a simple text level.
* **Change the rewards** and see how behaviour changes.
* **Change the look and story** using the Unity theme.
* **Inspect the policy** with the policy arrows.
* **Save and load** the learned Q-table.
* **Export** episode results to CSV.

## Quick start

1. Open the project in **Unity 6000.x or later**.
2. Open `Assets/Scenes/GridLearn.unity`.
3. Press **Play**.
4. Use the HUD:

   * **Train**: train one episode.
   * **Watch**: watch the current policy.
   * **Play**: control the agent with the arrow keys.
   * **Reset**: clear the Q-table.
   * **Fast**: train 100 episodes.
   * **Show / Hide Policy**: show the learned best action.
   * **Save / Load**: save or restore the Q-table.
   * **CSV**: export episode results.

## GridWorld

A GridWorld is a small world made of a grid of squares. The agent moves from square to square using simple actions such as up, down, left, and right. Each square can have different rules or rewards.

Levels are simple text files. Each character represents a tile:

| Tile | Meaning                 |
| ---- | ----------------------- |
| `S`  | Start                   |
| `.`  | Empty floor             |
| `m`  | Mud / costly terrain    |
| `H`  | Hazard / terminal state |
| `G`  | Goal / terminal state   |
| `#`  | Wall                    |

See **[docs/GRIDWORLD.md](docs/GRIDWORLD.md)** for the full tile, reward, transition, and state details.

## Make your own world

Edit:

`Assets/Levels/level01.txt`

You can create your own GridWorld using the available tiles, then give it your own visual style through:

`Assets/Themes/DefaultTheme.asset`

The learning code does not need to change.

## Project layout

```text
Assets/Scripts/Core/       Tabular Q-learning and GridWorld
Assets/Scripts/Unity/      Unity presentation and interaction
Assets/Levels/             Text-based GridWorld levels
Assets/Themes/             Visual theme
Assets/Tests/              EditMode and PlayMode tests

docs/DESIGN.md             Project design
docs/GRIDWORLD.md          GridWorld reference
docs/TESTING.md            Testing checklist
docs/ORIGIN.md             Project origin
```

## Learn more

GridLearn uses **tabular Q-learning**, a model-free reinforcement learning algorithm introduced by Watkins (1989).

The project is a small educational implementation, not a general-purpose reinforcement learning framework.

The GridWorld teaching concept was inspired by Unity Technologies' Q-GridWorld demo (2017). No code from that project is copied. The implementation was written independently. See `docs/ORIGIN.md`.

## Licence

MIT License. See `LICENSE`.

Copyright (c) 2026 Iman Mossavat.
