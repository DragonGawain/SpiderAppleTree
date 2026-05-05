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
