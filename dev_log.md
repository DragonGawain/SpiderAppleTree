## May 04 2026

- Project start!
- Initial setup
- Brainstorming and baseline implementation for walkables (level)

## May 05 2026

- Refinement of walkables system
- Baseline for save system
- Planning on having every level be loaded from a JSON, so I need the save system a lot earlier this time.
- Setup serialization and deserialization for levels and their basic components (trunks and branches)
- ILevelElement interface. IWalkables inherits from ILevelElement. ILE will hold all level elements, including web reinforcements, web strings, etc. Not the player.
- Created LevelStateDictionary class along with its titular dictionary to hold the level state (fresh, partial, solved) for every level (the int levelAccess dream is dead)
- All this serialization stuff is untested. Next step is creating some objects to represent these things, trying to make a save file, and loading a file. Creating the json from scratch without a sample is surely doable, but sounds like a pain.
- Renamed the SampleScene to LevelEditor. I'll use it to make the levels (paint the levels, then assign the level ID and hit a button to create a new/overwrite an existing file)
- Starting to work on level editor system (painting!)
- Successfully wrote a painted level to a file!
    - This means that I have the paint-based level editor that I wanted!
- Loaded a level successfully!!!
- Data is currently all being populated to a tilemap. I may want to change that at some point (when I make it such that branches can fall).
    - That being said, manipulating targetted cells with SetTile is pretty easy (can `SetTile(V3I, null)` to clear a cell)

## May 06 2026

- Made a second tilemap in the editor scene for elements (aka "interactables" cause walkabales are also technically elements)
- Make a second tile palette to go along with the second tilemap
- Renamed all the Tiles to have better names (e.g. trunk_src)
- Postulated having `WindController.cs` be an inner class of `Level.cs`
- Created InputManager and started to populate it
- Made GameManager a `DontDestroyOnLoad` object
- Made GameManager (and therefore all of its required managers) singleton
- Made WebMenu action map in the InputActions
- Expanded IWalkable.CanWalkHorizontal to take in an exact direction so that I can handle moving from a branch back onto the trunk
- Also allows me to have two trunks next to each other with a branch sticking off of one trunk while simultaneously disallowing movement from one trunk to the other
- Updated `Level.cs` with new public readonly struct `InitalLevelDataContainer` to store initial level data and easily pass it off to the Player object.
- Updated `Level.cs`, `LevelManager.cs`, `SaveManager.cs` to handle the new struct
- `LevelManager` should now spawn in a player prefab at the designated spawn coordiantes upon loading a level
- Made movement logic - player should now be able to move. Movement is being checked via IWalkable CanWalk.. methods.
- Set up InputAction maps for movement (Player) and web menu (WebMenu) and gave the `InputManager` callbacks for all the inputs
- Created new dictionary in `Level.cs`, interactables. Filled with elements that inherit from IInteractable (which inherits from ILevelElement)
- Thought up that each ILevelElement should _not_ be a monobehaviour, but should instead instantiate the corresponding prefab at the end of its constructor.
    - This will let me have the elements be animated while easily maintaining my current detection system (which is based on the tilemap coordinates)
- `Fruit.cs` and `FruitAdapted.cs` set up.
- Successfully saved and loaded more complex level structure, including fruits, spawn point, and goal! (i.e. saved and loaded interactables)

## May 07 2026

- Fixed trunk horizontal walkable status
- Fixed fruits only being collected when the cell it is on was left, instead of when the cell is entered.
- Added web menu UI
- Set up web menu controls (both keyboard and mouse)
- Web menu visuals and selection is working as intended!

## May 09 2026

- Figured out how to branch weights, and by relation (somewhat) refined the branch snapping system.
- Decided that fruits will have a weight that can affect branches
- Added the third tilemap, the numbers map!
    - This tilemap will display the amount of support (or how much weight a branch segment can still accept).
- Added comments in the manager scripts about what task they handle. Going forward, I should try to describe the task of any given file before writing it. (for the important files at least. It's kinda just bloat if I have these javadoc-esque comments for every class)
- Simplified verifying movement input. All checks are now performed in the `VerifyAndCompleteMove(Direction dir)` method.
- In order to enable that, the Direction enum was shifted slightly to be in a clockwise order.
- Fetching all the numbers for the number tilemap straight from the TilePalette prefab. Doing it this way so I don't need to individually set up all the SerializeFields (it would be 19 at the moment, and that number is almost guaranteed to go up as my need for more decimal places goes up. I'll want to try to find a better solution that having wayyy too many numbers prepared).
- Initial weights calc set up!

## May 11-14 2026

- Fiddled around with how branch weight calculations work. Finally settled on an algorithm that I'm satisfied with. (for now at least).
- I decided that weight deltas would only be propagated up (towards nearest supports) only! Down propagation would could a branch segment to instantly snap the moment the player stepped onto the source of the branch, or never (at that player weight).
- More setup for the new support calculation system:
    - Fixed/added save adapters as needed
    - Added Supports Tilemap (editor) to the tilemaps so I can place supports in the level editor
    - Can now properly save and load levels with the new support system
- Updated Player to call the new weight methods of Branch.
- Fixed a bug in player where it would not set the branch it occupied to null when leaving the branch, which would cause a crash.

## May 15 2026

- Continued work on level editor
- Set default player weight to be 2 instead of 0
- Set up systems to allow me to customize aspects of trunk supports and fruits from the editor
- Fruits now apply their weight to branches that they spawn on
- Fruits now correctly remove their weight from branches when collected

## May 16 2026

- Fixed decimal weights not displaying
- Fixed branches displaying no player load during the move transition time when the player is moving from one branch segment to another
- Fixed bug that caused web menu buttons to sometimes be highlighted as if selected when they were not.
    - This bug sometimes displayed multiple buttons as being selected
    - This bug only occured when clicking an option with the mouse
    - I fixed it by stopping the automatic colour shifting that the Button component has built in.
- Added double-click confirmation when trying to overwrite a save file (console message)
