# GridLearn

A small Unity project where an agent learns to find its way across a grid by trial and error (Q-learning).
You give the grid your own story and look, then watch the agent learn it, or play it yourself.

> Status: starting material. The code is not written yet. See `AGENTS.md` and `docs/DESIGN.md`.

## The idea

For the computer, the game is a grid of numbers. Each kind of tile gives a reward:
a goal is good, a hazard is bad, mud costs a little. The agent does not know what these things *mean*.
It only tries to get a higher score. You decide the meaning: a drone avoiding toxic spills, a mouse looking for cheese, a knight crossing a trap room.

## What you can do with it

1. **Run it** and watch the agent learn.
2. **Reskin it:** change names, sprites and colours in a Theme.
3. **Change the rules:** edit the level file and the rewards. Does the agent still learn?
4. **Make it yours:** add a new tile type, or more things the agent has to remember.
5. **Test and explain:** measure how fast it learns, and explain why.

Levels are plain text files (see `Assets/Levels/level01.txt`):

| Symbol | Meaning |
|---|---|
| `#` | wall |
| `.` | empty |
| `S` | start |
| `G` | goal (episode ends, big reward) |
| `H` | hazard (episode ends, big penalty) |
| `m` | mud (costs extra, episode continues) |

## Getting started

1. Install Unity Hub and the current Unity LTS version.
2. Create an empty **2D** project *inside this repo's folder* (so `Assets/` and the other folders end up here).
3. Open the project. Everything else is described in `docs/DESIGN.md`.

## Project layout

```
Assets/Scripts/Core/       the world and the learning (plain C#, no Unity code)
Assets/Scripts/Unity/      showing it on screen, buttons, player control
Assets/Tests/EditMode/     automatic tests
Assets/Themes/             sprites, names and colours per tile type
Assets/Levels/             level text files
docs/DESIGN.md             the design
AGENTS.md                  instructions for the AI coding agent
```

## Licence and credits

Licence: MIT (see `LICENSE`). Copyright (c) 2026 Iman Mossavat.

The learning method is tabular Q-learning (Watkins, 1989; Sutton & Barto, *Reinforcement Learning: An Introduction*).
The idea of a Unity grid world for teaching it was inspired by Unity Technologies' Q-GridWorld demo (2017).
No code from that project is copied. The code is written independently from our own design.
