# GridLearn — Future Ideas and Experiments

This document records visualization and interaction ideas that are **not yet implemented**.
They are kept here so they are not forgotten while the current milestone is being finished.

## Planned Visualisations

1. **Action-scoped Q-value heatmap**
   Let the user pick one action `a` (Up/Down/Left/Right) and draw the grid coloured by
   `Q(s, a)` for that action. This is different from the current value-function view because
   it shows how good each *individual* action is across all states, which is useful for
   understanding why the agent prefers one move over another in a particular cell.

2. **Confidence-weighted policy arrows**
   Instead of drawing every best-action arrow with the same size/opacity, scale each arrow
   by the softmax confidence of the best action:
   ```
   confidence(s) = exp(Q(s, a_best) / T) / Σ_a exp(Q(s, a) / T)
   ```
   where `T` is a controllable temperature (slider or +/- buttons). A high-confidence state
   gets a large, opaque arrow; a state where several actions have similar Q-values gets a
   small, faint arrow. This makes the policy's uncertainty visible at a glance.

---

Last updated: 2026-04-10
