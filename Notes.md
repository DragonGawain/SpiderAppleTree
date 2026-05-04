## Organization

- One file per level (JSON?)
- World map
    - Lots of booleans for the meta paths that have been built?
    - Also booleans for EVERY DARN LEVEL to check what the player has access to? (surely nah...) Surely I can do a single int and the player has access to every level number <= level tracker int.

# Scripts

## Managers

### GameManager

- Handles general game states (i.e. misc stuff)

### PlayerManager

- Handles player movement
- Handles player stats (web count, weight, length, etc)
- Should NOT handle chacking if a branch should snap

### SaveManager

- Handles creating and loading saves
    - Game file
    - Solved level solution (Read/write)

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
