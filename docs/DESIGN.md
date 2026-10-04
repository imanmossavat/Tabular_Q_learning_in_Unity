# GridLearn: design

## Goal

A small, readable Unity project for teaching tabular Q-learning.

A student can change the level, story and rewards without changing the Q-learning algorithm, and can see, test and explain how the agent learns.

The project should be roughly as simple as the original Unity Q-GridWorld project, but use a modern Unity version, an editable text level, clearer separation between environment and learning, and a few better teaching features.

## Principles

**Keep the learning visible.** A student should be able to find the Q-learning algorithm quickly and understand it without navigating a large architecture.

**Separate logic from the screen.** The world and Q-learning agent contain no Unity-specific logic. Unity displays and controls them.

**Keep the code small.** Use only a few meaningful classes. Do not create classes or abstractions merely for architectural purity.

**No magic numbers.** Learning and world settings live in one configuration object.

**Repeatable experiments.** A seed makes a training run deterministic.

## Learning and world logic

The environment and Q-learning agent are plain C# and contain no Unity code.

A student familiar with basic C# should be able to understand the complete Q-learning implementation by reading a small number of files.

### Level

Levels are read from a simple text file.

Symbols:

* `S` = start
* `G` = goal
* `H` = hazard
* `m` = mud
* `#` = wall
* `.` = ordinary floor

A level must have exactly one `S` and one `G`, rectangular rows, and a path from `S` to `G` through non-wall cells.

Hazards do not make a level invalid. They are part of what the agent must learn to avoid.

### Actions

Actions are an enum:

`Up`, `Down`, `Left`, `Right`.

The number of actions is obtained from the enum rather than being hard-coded.

### World rules

State is the agent's cell:

`state = y * width + x`

State count is:

`width * height`

Moving into a wall or outside the grid leaves the agent in the same cell.

Every movement starts with a reward of `-0.04`.

Entering `G` adds `+1.0` and ends the episode.

Entering `H` adds `-1.0` and ends the episode.

Entering `m` adds `-0.30`.

Rewards are additive. For example, entering mud gives `-0.34`.

An episode also stops after `maxSteps`, default `200`.

A timeout is **not** a terminal state for Q-learning. The update after a timeout still uses the value of the next state.

The environment provides:

`Reset()`

and

`Step(action)`

where `Step` returns the next state, reward, whether the episode is terminal, and whether it timed out.

## Q-learning agent

The agent has:

`Q[state, action]`

with all values initially zero.

The update is:

`Q(s,a) += alpha * (target - Q(s,a))`

where:

`target = reward` if the transition is terminal

otherwise:

`target = reward + gamma * max Q(s')`

Default settings:

* alpha = `0.3`
* gamma = `0.95`
* epsilon starts at `1.0`
* epsilon decreases linearly to `0.05` over `400` episodes
* epsilon remains at `0.05` afterwards

When exploring, choose uniformly from all actions.

When exploiting, choose the action with the highest Q-value. If several actions tie, choose randomly among the tied best actions.

Use a seeded `System.Random`. Do not use `UnityEngine.Random` for learning.

## Training

Training runs complete episodes using the environment and Q-learning agent.

For each episode record:

* total reward
* number of steps
* whether the goal was reached
* epsilon

Training must be possible without drawing the Unity scene so many episodes can be run quickly.

Do not build a separate training framework unless it is actually needed to keep the code readable.

## Unity

Use a simple 2D grid with sprites or UI images.

There is no physics and no colliders. All movement and rewards come from the environment logic.

### Modes

**Train**

The agent learns while its movement is visible.

**Watch**

The agent follows its learned greedy policy without exploration.

**Play**

The student controls the agent with the arrow keys. The same environment rules and rewards are used.

The three modes should make the learning concept clear:

* Play: "What would you do?"
* Train: "How does the agent learn?"
* Watch: "What did the agent learn?"

### HUD

Keep the HUD small and readable.

Show:

* episode
* epsilon
* last reward
* recent success rate
* training speed

Provide controls for:

* Train
* Watch
* Play
* Reset
* fast training

Fast training should run many episodes without animating each step and then return to the visual state.

Do not use `GameObject.Find`. Use Inspector references where Unity references are needed.

### Q-value view

Provide a simple optional view of the learned policy.

At minimum, show the best action in each cell with an arrow.

If practical, also show the value of the best action.

The visualization should help a student connect the agent's behaviour to the Q-table. It should not become a separate complex system.

## Expected behaviour

For `Assets/Levels/level01.txt`:

* grid size: 10 × 10
* shortest path: 18 steps

With the default settings, the agent should normally discover the shortest path in roughly 200 episodes.

A Python simulation of the design found the shortest path in about 200 episodes, with about 12,000 total steps. Across 100 random seeds, the slowest run took 248 episodes.

These numbers are targets rather than guarantees of identical Unity timing.

The automated learning test should allow up to 600 episodes.

## Tests

Test the important behaviour rather than testing every implementation detail.

1. Level parsing:

   * valid level
   * missing start
   * missing goal
   * multiple starts
   * multiple goals
   * uneven rows
   * no path

2. World:

   * wall bump
   * edge bump
   * goal
   * hazard
   * mud
   * timeout

3. Q-learning:

   * terminal update
   * non-terminal update
   * timeout uses the non-terminal update

4. Exploration:

   * over many selections, every action can be selected

5. Learning:

   * on `level01`, several fixed seeds learn a greedy 18-step path within 600 episodes

6. Reproducibility:

   * the same seed produces the same training result

## Later, not now

Possible student challenges:

* a key that must be collected before reaching the goal
* slippery tiles
* moving hazards
* changing alpha, gamma and epsilon
* comparing another algorithm

These should not be implemented in the first version.

## Scope rule

When deciding whether to add a feature, prefer the simpler implementation if it still supports the teaching goal.

The first version should teach:

**state → action → reward → next state → Q update**

Everything else is secondary.

## Open questions

The agent should add genuinely unresolved design decisions here before implementing them.
