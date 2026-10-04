# Instructions for the AI coding agent

You are building **GridLearn**, a small Unity teaching project. Read `README.md` and `docs/DESIGN.md` first. The design is the specification.

## Rules
- **Write everything from the design.** Do not look at, fetch or copy code from other Q-learning or grid-world repositories (including Unity's "Q-GridWorld"). This project must be original work.
- **Use our own names.** Do not use the class or method names `InternalAgent`, `GridEnvironment`, `Environment`, `EnvironmentParameters`, `SendState`, `collectState` or `MiddleStep`. They belong to another project.
- **Keep it simple.** Students will read this code. Short files, plain names, short comments, no clever tricks. No extra packages. Do not use ML-Agents.
- **Keep `Core` free of Unity code.** Put it in its own assembly definition with no engine references. Do not use `UnityEngine.Random` or `GameObject.Find`.
- **No magic numbers.** Use the config object.
- **Do not add features that are not in the design.** If you think something is missing, write it under "Open questions" in `docs/DESIGN.md` and ask.
- **Test as you go.** A step is only finished when its tests pass.

## Running Unity yourself (Unity CLI)
The human has the Unity CLI installed (`unity`, a beta version). Use it so the human does not have to open the editor.
- **Start with `unity skill show`** and read it. Use `unity <command> --help` for exact options. Do not guess flags.
- **Check the setup first:** `unity doctor`, `unity license`, `unity auth`. If sign-in or the licence is a problem, stop and tell the human.
- **Project:** if there is no Unity project yet, ask the human whether you should create an empty 2D project here (the CLI can do it) or whether they will.
- **Use it for:** `unity test` (EditMode tests), `unity recompile` or `unity build` (does it compile), `unity run` (batch runs, e.g. train and write a CSV).
- **Look at the result:** the CLI says it can drive a running editor and capture the game. After the screen work, take a screenshot and check the picture. Still list what you could not check by looking.
- **One editor per project.** Close the editor before batch runs, unless the CLI help says otherwise.
- **It is a beta.** If a command fails, report the exact command and the error. Do not work around it silently.

## Order of work

1. **Core and tests, without Unity.** Write `Core` and the EditMode tests. Also add a small plain .NET test project in `tools/CoreTests/` that uses the same Core files, so you can run `dotnet test` and check the logic without opening Unity. Finish when all tests in the design pass.
2. **Unity scene.** Build the grid view, the Theme, and a default theme (simple coloured squares are enough). Show `level01` and let the agent train in view.
3. **Modes and HUD.** Train, Watch, Play, the HUD, speed control, fast train.
4. **Extras.** Q view overlay, save and load, CSV export.
5. **Documentation.** Update `README.md` with real steps (how to run, how to change the theme and the level, where the tests are). Add `docs/TESTING.md`: a short checklist a person can follow in Unity in under 30 minutes (open the project, press play, what to see, which tests to run).

6. **Originality check.** Before you finish, search all your code for the names listed in the Rules and for anything else that looks borrowed. Write the result, and where each part of the code came from (the design, or your own general knowledge), in `docs/ORIGIN.md` in a few lines.

Stop after each step and summarise what you did, what you tested, and anything you are unsure about.

## What to tell the human
- Anything you could not run or check yourself (especially things that need the Unity editor).
- Any place where the design was unclear and what you decided.
- Whether any part of your code was based on an existing repository or example you remember, and the result of the originality check.
- Any numbers that differ from the "Expected behaviour" section, with the measured values.

## Done means
- All tests pass.
- `level01` trains and the agent finds the 18-step path.
- Changing the theme, the level file or the rewards needs no code change.
- A student can read `Core` in one sitting.
- `docs/ORIGIN.md` exists and says the code was written from the design.
