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
- Loading a level now clears all level elements from the old level, regardless of if the old level existed or not.
- Updated branch calculations to now factor in a backwards pass to handle cases where a branch segment has no supports to its left (but yes support to the right).
- Branch snapping is working!
    - Falling branch segment is currently just getting deleted

## May 17 2026

- Added some story/lore to the notes document
- Preemptively fixed a bug where I was ignoring the possibility of a snapped branch only having supports to the right of the snap
- Branches can fall and land on other branches, forming a bridge!
    - Branches falling on top of a trunk should also work, but this is untested.

## May 18 2026

- Continued to refine falling branch calculations
- Fallen branches now check for any weights on them (as do branch remnants)
- Branch remnants also check for player weight
- Added log to inform me when the player should die due to a fallen branch (verify that the check for such a thing is easy)
- This check is built-in to checking for player weight on the remnants
- Fixed bug where the player would remember the old branch, causing it to display its would-be support values the next time the player moved
- Fixed issue where fallen branch segments that are directly on top of trunks were not vertically walkable.

## May 21 2026

- Completed making trunk supports fully automatic
- Fixed trunk supports getting added twice for branches affected by a snap
- A LOT of minor bug fixing/tweaks to branch snapping logic

## May 26 2026

- Overhauled save adapters for branch and fruit.
- Refined level adapter slightly (moved some stuff around so level info is displayed before level elements in the save file)
- Confirmed that removing the TrunkSupportAdapter is OK (became obsolete - trunk supports are now determined automatically.)
- Removed FruitType enum. (it may return, but would only change the fruit visually)
- Updated fruits to apply their changes (weight, web count) to the player.
- Confirmed that win condition works!
    - Detects when the player walks onto the goal point, and that all needed fruits have been consumed
    - Just logs a message to the console. The goal point still isn't even visible.

## May 27 2026

- Pulled placeholder goal sprite from default unity sprites
- Made goal get coloured green when the conditions to activate it are true (all needed fruit consumed)
- Finished deleting TrunkSuportEditor file
- Updated adapter logic/save manager logic to function with updates
- Upadted adapters so levels can now be saved to file again
- Changed test level id's (names) to be in the 1000 range. (standard level numbers should not exceed 999)

## May 28 2026

- Disable movement inputs on player death
- Make it so that movement inputs cannot be re-enabled post death
- Phasing out LevelStateDictionary cause it's dumb. (Unless that dict is stored to a file, it is useless. Also thinking of abandoning the idea of saving partial/solved level states.)

## May 29 2026

- Imported some [tree/branch tile assets](https://assetstore.unity.com/packages/2d/environments/too-cube-forest-the-free-2d-platformer-game-tile-set-117493#asset_quality) from the unity asset store. They'll be better than my coloured squares for now.
- Started working on a real in-game level editor. I'm gonna want this at some point. I remember a GDC talk from CD Projekt RED about the importance of having good tools, so I'm gonna make me some convenient tools!
- More level editor work
    - Sketched a plan for UI interface.
    - Set up UI interface
    - Starting scripting (LevelEditorManager.cs)
- Added LevelEditorManager to required managers (via GameManager RequireComponent)
- Imported new `fruit` sprite for the fruits. Planning on the fruit tile in the editor being invisible, but spawning a visible `Fruit_editor` gameobject allowing for stat edits, but the fruit tile for the game will have this fruit sprite.
- Initial level values are now taken from input fields, not `SaveManager` inspector (SerializeField) fields.
- Started converting things to the new sprites - loaded level uses new trunk, branch, and fruit sprites. Still need to set up connecting branch sprite (for when a branch is supported by a trunk from underneath)
- Set up so `SaveManager.cs` will recognize the names of the tiles with the new sprites when scanning
- **Fixed bug where eating a weight changing fruit would leave a negative ghost of itself (that could potentially make a weight delta go negative), as well as would always leave a ghost of the players old weight when consumed on a branch.** (this was a NASTY bug. Kinda tough to fix, and NAZ-TEE in nature!)
    - To fix this, I changed the branch weight updating system. Now, when a branch needs to be recalculated, it is added to a set. After all weight modifications are done, all branches will update their weights.
    - This also solves the problem of weight update orders. All weight changes are applied before the branch is recalculated. This prevents branch snapping from mid-stages.
    - This fix caused another problem that had to do with branch weight recalculations when there is a snap. When a branch snaps, we've already moved past the post-move phase, so each branch has to call its own recalculations. This includes the (potential) new branch that the player is on.
- Pulled a picture of a cartoon spider from google images to act as a #temporary character model
- Redefined the level boundary limits so that there's no overlap with the level editor UI buttons. (If I need more space, I'll deal with it then, but I don't want to start moving the camera. Yet.)
- Set up boundary variables in saveManager

## June 08 2026

- Set up level id auto-incrementer tickbox attached to the save manager boolean
- Work on level editor, specifically the 'test level' and 'continue editing level' features.
- Made the palceholder and input text for initial player values be big. (at default size, they were too small to be legible)
- Level editor semi functional! Detecting clicks. Can place and erase entities. Next step will be creating interactable editors (fruit editors only at this point), and then having fruits be selectable so that the values can be edited.
- Removed erase button (erasing is done via 'adding' a null tile)
- Added icon to inform user what tile they are currently adding.
- Created FruitEditorController to allow editing of FruitEditors
- Continued work on Select mode. Select mode will now target a specific selectable tile map (interact/support) based on an enum. Continuously clicking the Select button will cycle the enum.
- The selected entity icon now also shows which tilemap is being actively targeted when in select mode.
- Started work on editor controllers/what happens when you click on an editable entity in the level editor

## June 09 2026

- Continued working on editor controllers (really just the `FruitEditorController`, but I'm building the system to be able to expand to handle all potential editor controllers easily)
- Fruit editor controller working!

## June 10 2026

- Got chatGPT to create sprites for hollow and web logs (yes this was a pain and required editing. In the future, only sak chatGPT for a single sprite at a time)
- Converted scanning for branches to use a regex instead of a strict exact string comparison

## June 12 2026

- Updated branch discovery algorithm to find branches adjacent to trunks (including on top) as opposed to scanning the tilemap. This allows me to remove the need for source points.
