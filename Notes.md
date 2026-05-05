## Organization

- One file per level (JSON?)
- World map
    - Lots of booleans for the meta paths that have been built?
    - Also booleans for EVERY DARN LEVEL to check what the player has access to? (surely nah...) Surely I can do a single int and the player has access to every level number <= level tracker int.

- Tilemap just for snapping purposes. Might not even use it?

- Level will be enumerated as triple digits (ex: 001)
- Bonus (meta) levels wll be enumerated with an extra flag in the thousandths digit (ex: 1001, 2002, etc)
- This will allow me to more easily determine level access rights.
    - umm, this doesn't fully matter. I wrote this when thinking about loading a level
- In the extremely unlikely case where I manage to create over 999 puzzles, I'll deal with it then, but like, I don't think that'll happen. My target goal is ~100 levels (inluding meta/bonus). I might not even need the 3rd digit.

# Scripts

## Managers

### GameManager

- Handles general game states (i.e. misc stuff)
- Level select from world map

### PlayerManager

- Handles player movement
- Handles player stats (web count, weight, length, etc)
- Should NOT handle chacking if a branch should snap

### SaveManager

- Handles creating and loading saves
    - Game file
    - Solved level solution (Read/write) [Might move this to LevelManager]
    - Level creation file saving

### LevelManager

- Handles loading a level (read only)

### UI Manager

- Main menu
- Pause menu
- Web menu
- HUD (web count, weight, length, etc)

## Entities

### IWalkable (interface)

- Anything that the spider (player) can walk on should inherit from this interface

### Trunk

- Trunk sections

### Branch

- Branches
