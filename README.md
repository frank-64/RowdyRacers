# RowdyRacers

# About
Frankie Clipsham's final year Computer Science coursework for the Gaming Technologies and Simulations module at Newcastle University

# Installing
If you have GitBash installed then type the following into your terminal: 

```git clone https://github.com/frank-64/RowdyRacers```

# Libraries used
- Flocking algorithm - PSFlocking https://github.com/Floppi003/PSFlockingLibrary for the files PSFlockingUnit.cs and PSUnitManager.cs.
- LowPolyWater for water physics.
- Other various packages for visual assets such as cars, road generator, skyboxes and props.

# Code Sampled
- AI Waypoint Pathfinding - Unity standard assets

# Setup Instructions
- When playing the game in the Unity please use the Full HD (1920x1080) aspect ratio for the UI to be visible.
- Due to issues with lighting scenes will appear darker if loaded from the main menu.
- Any issues with the menu please click on to the scenes individually (Assets/Scenes).
- The minigame can be accessed through the Race mode 3/4 of the way round the track. You can either drive in the Abandonded Dungeon or access the scenes individually.

# Play Instructions Race Game
- Use the arrow keys left and right to steer.
- Use the arrow keys forward and backwards to accelerate and brake/reverse.
- 1 lap and Easy is the default race settings.

# Play Instructions Minigame
- Use the space button to jump.
- HINT: Don't press the space button too frequently and try specific periodic jumps.

# Known Issues
- There will be warnings in the RampMode scene as the tree's placed through the Terrain aren't successfully creating their colliders and placing 100s of tree prefabs with the colliders would take too long.
- The AI does not work as expected once the game has been built as all testing occured in Unity before being built.
