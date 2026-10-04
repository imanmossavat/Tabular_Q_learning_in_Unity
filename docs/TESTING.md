# GridLearn manual testing checklist

A quick walk-through that should take less than 30 minutes.

## Before you start

1. Open the project in Unity.
2. Open `Assets/Scenes/GridLearn.unity`.
3. Select the **Game** tab and set the resolution to **1280×720** or **Free Aspect**.

## Scene and play mode

1. Press **Play**.
2. Confirm the grid is visible and coloured (walls, start, goal, hazards, mud, agent).
3. Confirm the HUD is readable across the top of the screen.
4. Confirm the **Train**, **Watch**, **Play**, **Reset**, **Fast**, **Show Policy**, **Save**, **Load** and **CSV** buttons exist and react visually when hovered/clicked.

## Modes

1. **Play** – with the Game view focused, press the **arrow keys** and confirm the agent moves according to the rules (walls block, hazards/mud give penalties, goal ends the episode and resets the agent).
2. **Train** – press **Train**. Confirm the agent moves step by step, the episode counter increases, and the HUD updates.
3. **Watch** – after training a few episodes, press **Watch**. Confirm the agent follows a greedy path without random exploration.
4. **Fast** – press **Reset**, then **Fast**. Confirm 100 episodes run quickly and an arrow policy appears on the grid.
5. **Reset** – press **Reset**. Confirm the arrows disappear and the episode counter returns to 0.

## Extras

1. **Show / Hide Policy** – press the button and confirm arrows appear/disappear.
2. **Save** – press **Save**, then check the Console for the saved file path.
3. **Load** – press **Load** and confirm the HUD episode count is restored to the saved value.
4. **CSV** – press **CSV**, then check the Console for the exported file path and open it to confirm it contains episode data.

## Expected learning behaviour

For `Assets/Levels/level01.txt`:

- Shortest path: **18 steps**.
- After a **Fast** run (100 episodes) the agent often finds a good path; after another **Fast** run it usually finds the 18-step path.
- The Console should contain no red errors during any of the above steps.

## Automated tests

1. Open `Window > General > Test Runner`.
2. Run the **EditMode** tests; all should pass.
3. Run the **PlayMode** test; it should pass.
