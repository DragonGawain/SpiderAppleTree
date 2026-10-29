- Fallen branches do not correctly adjust the trunk segment touching their extremities to being horizontally walkable
    - This will also apply to trunk supports. Those side touching trunks do not apply a trunk support.

- Branch support overflow! Easiest way to reproduce: have a branch that is 1 segment long with trunks on both sides. This makes both trunk supports give support, leading to a support value of 12. But my current highest support value is 9, so this overflows to 2.5.
    - This is currently suppressed by setting maximum raw support to 9, but this might need to be fixed better.
- Portal does not swirl if there are no fruit in the level
