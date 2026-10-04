# Origin of the code

This project was written from the specification in `docs/DESIGN.md`. No code was copied from other Q-learning or grid-world repositories, including Unity's Q-GridWorld demo.

## What was written from the design

- `Assets/Scripts/Core/` – the level parser, grid world rules, Q-agent update, trainer and `Config` are direct implementations of the rules in `docs/DESIGN.md`.
- `Assets/Scripts/Unity/GameController.cs` – implements the Train / Watch / Play modes, HUD, fast training, Q-table save/load, CSV export and policy overlay described in the design.
- `Assets/Scripts/Unity/GridView.cs` – builds the grid, moves the agent sprite, and draws the policy arrows from the Q-table.
- `Assets/Scripts/Unity/Theme.cs` – simple ScriptableObject colour/theme lookup.
- `Assets/Scripts/Unity/Editor/SceneBuilder.cs` – editor-only helper that creates the default scene from the level asset and theme.
- `Assets/Tests/` – cover level parsing, world rules, Q-learning updates, exploration, reproducibility and the scene load/training flow.

## General knowledge used

- Tabular Q-learning update rule (Watkins, 1989; Sutton & Barto, *Reinforcement Learning: An Introduction*). This is standard textbook material; the implementation here is written from the design equations.
- Unity API usage (`MonoBehaviour`, `Canvas`, `Button`, `SpriteRenderer`, `JsonUtility`, `Input System`, etc.) from Unity documentation and general Unity development knowledge.

## Originality check

The following names belonging to other projects were searched and are **not present** in the code:

- `InternalAgent`
- `GridEnvironment`
- `Environment`
- `EnvironmentParameters`
- `SendState`
- `collectState`
- `MiddleStep`

No ML-Agents package or API is used.
