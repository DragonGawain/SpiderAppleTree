## Inspiration

- I originally had the idea of the branch breaking idea many years ago. I estimate that I thought it up around 2019 or so.
- The core gameplay loop looks similar to Snakebird at first glance. (Which is interesting, because I had thought up the idea before I player Snakebird. I don't think I had seen trailers of Snakebird previously?)
    - Anyway, anyone looking at my gameplay loop who knows of Snakebird will likely see similarities, and that's fine with me. By this point, Snakebird _is_ a partial inspiration to my game after all.
- Though this may be surprising to some (i.e. future me lol), I'm also taking some inspiration from Lara Croft. Specifically, I remember a clip from I think GMTK about a puzzle where you had to intentially touch some walls that could be crossed twice once, and then lure an enemy to also cross them behind you. Since the walls were already touched, the enemy falls through and you can clear the level!
- I'm also taking inspiration from Baba Is You and A Monster's Expedition. My puzzle design for the meta levels will show this more clearly later.
- I don't know why I'm so hung up on the idea of the player being a spider, but that's been the design since the very beginning.

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
- Should NOT handle checking if a branch should snap

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

### InputManager

- Handles reading inputs

## Entities

### IWalkable (interface)

- Anything that the spider (player) can walk on should inherit from this interface

### Trunk

- Trunk sections

### Branch

- Branches
- Support level
    - `calcFall` (name pending) method? (`LevelManager.cs` or even `GameManager.cs` is likely a more appropriate place)

#### Idea: branch types

- Crushing branch -> will apply extra weight to branches it falls onto, potentially causing a cascade
- Hollow branch -> will always be caught
- Spinning branch -> flips vertically, can be caught and acts like a trunk after wards
- _Grid breaker -> lands diagonally, allowing for diagonal movement along its path_ (Room to Grow inspiration)

## Level elements

### Player

### ILevelElement (interface)

- Root of all level elements
- Empty. Serves as a contract (similar to Serializable) so that I can store all unique elements in a set.
- Also makes it easier to set up the level adapter

### IWalkable (interface)

- Walkable contract. Enforces that all walkables state if they can be stepped on from a horizontal or vertical direction
    - May later expand to enforce stating from a specific direction?
- Houses the `Direction` enum

### Level

- Contains raw level data including:
    - Level id
    - Level size (bottom left corner should always be (0,0))
    - Spawn point coord (coord where the player starts)
    - WindController(?) -> inner class (not sure if this should be its own file)
    - Turn counter (used for wind)

#### WindController

- Wind can blow the spider in the following scenarios:
    - Hanging from a string
    - Floating via webrella
- Wind direction (mainly left/right, but maybe up winds increases all support levels and down winds decreases support levels?)
- Wind counter (how often does wind blow)
    - To make cyclical wind, each row of any given level would need its own wind counter
    - Wind counter will be a constant number. Checking `turnNumber % windFrequency == 0` will determine when wind should blow on that row

#### Water controller

- Water can wash away webs

# Ideas

- This is a collection of ideas that are potentially outside the scope of this project, but that I don't want to forget about.

### Wind

- Pushes player when hanging from string or using webrella
- Can be in any direction, and any row, happening every N actions

### Water

- Washes away webs
- Insta-kills player
- Due to insta kill, can also act as a wall

### Storm

- After N actions, a storm knowcks down a tree, taking all of its branches with it,

### Length

- Yeah, still not sure if I want to do length..
- Change the number of cells the player walks with each movement action
- Can potentially allow moving across gaps
    - If this would allow for movement to the middle of trunk segments, I would need to change something with my current logic...

### Vertical supports

- Making a web string that connects two branches would cause the lower branch to gain some extra support in the middle

### Screen wraparound?

# THOUGHTS

- The shorter a branch is, the higher its initial support value should be.
- Support value represents the amount of weight that portion of the branch can take before snapping.
- So, since a long branch puts more strain on the connecting point, a shorter branch is easier to support and therefore should start with a higher support value!

## String supports

- A string support is built by stringing down from one branch all the way until a lower branch, **and then building a web support at that location**.
- Only one string support can affect a branch at any given time
- The string support that is farthest from the source of the branch has priority. If there are multiple supports, the ones closer to the source do nothing (other than provide verticle walkable passages)
- **QUESTION**: Can a branch be suspended by string supports alone?
    - YES! By 'adding slack' (new web menu option!) to the string, you can create a suspension bridge!
- **QUESTION**: Do the strings on both sides of the suspended branch need to be even?
    - NO! These suspension branches can form diagonals! (room to grow inspo showing itself here!)
- **QUESTION**: What happens to a branch that is only being held by a single string support?
    - Uhh, it goes vertical?
- The weight of the susepnsion branch (and the stuff on it) shout be split 50/50 to the two strings (so if the suspension bridge is 3 units long, each string transfers 1.5 weight to its upper connection. The segment that the string is attached to needs to be able to support this value).

- In order for suspension branches to be valuable, dropped branches need to have a length equal to or greater than (1 + gap length) in order to form a bridge.
    - If the dropped branch is (gap size + odd_number), the branch will shift to occupy 0.5 of a cell on both sides

#### Fruits

- Fruits can maybe have more affects on branches beyond just having a weight?
    - Maybe fruits can be sources of wind/water?
    - I will NIX the idea of fruits giving the player temporary special powers. That is outside scope and I do not think it will add to the game in a positive manner. The web menu is plenty of powers to create good puzzles.
    - Decaying fruit? -> fruit will disappear if not collected in N moves
        - This might not necessarily lead to a loss? But it usually will
    - Optional fruit -> fruit that does not need to be collected to access the level goal portal
        - Actually, I kinda think it should be the other way around. By default, you don't need to collect all the fruit. But, some fruit can have a 'golden aura' or something. The level goal portal will remain inactive until all golden fruit are collected.
