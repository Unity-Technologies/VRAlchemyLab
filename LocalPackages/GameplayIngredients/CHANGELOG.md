# Changelog

## 2019.3.5

#### Added

- Discover : Tag Filtering System
- Discover : Images in Discover Window

## 2019.3.4

This version added the following package dependencies:

* Cinemachine 2.5.0 
* Timeline 1.3.0

#### Added

* Preferences for Advanced Hierarchy View
* Added API to AdvancedHierarchyView to add other Icons from component types
* Help Menu links (github, documentation, openUPM)
* Added support for SceneAsset and EditorSceneSetup in Discover
* Added an option to change the Game View Link camera behavior to Cinemachine Brain preview.

#### Changed

* Game Save manager now saves to the Application.persistentDataPath folder
* Renamed HierarchyHints to AdvancedHierarchyView + Changed behaviour to apply visibility from preferences
* Managers: Adjusted RuntimeInitializeOnLoad for new Quick Enter Play Mode Settings
* Updated Starter Packages for Built-in RP, HDRP and URP (removed LWRP)

#### Fixed

* Fixed FocusUIAction that did not update the focus of a button in certain conditions.
* Fixed possible null in discover components

## 2019.3.3

#### Added

- Added Counters : Integer counters stored in the scene
- Added CounterAction and CounterLogic
- Added Create State button to StateMachines
- Added State Machine templates in the GameObject Creation Menu

#### Changed

- Messager now send messages through a copy of its source listeners array instead of the actual enumerator. This prevents from modifying the call array at the same frame if a OnMessageEvent becomes disabled during execution.
- Changed SceneViewToolbar Accessibility (now public to give access to OnSceneViewToolbarGUI)

#### Fixed

- Fixes in changelog (bad changes in 2019.3.2)

## 2019.3.2

#### Changed

- **Delayed Logic** now has a mode that allows you to have a random delay within a range.
- **Audio Play Clip Action** now allows you to randomize volume and pitch within a range of values every time you play the clip.

#### Fixed

- Fixed selection of Call tree window that became broken somehow in 2019.3.0f1

## 2019.3.1

#### Changed

* **Messager** is now able to pass instigator Game Object through message broadcast.
* **OnMessageEvent** now passes the optional instigator instead of itself as instigator to the Calls. In order to pass itself use an intermediate **SetInstigatorLogic** that targets the OnMessageEvent owner to replicate the former behaviour.
* **SendMessageAction** now passes its instigator game object to the **Messager**

#### Added

* **Call Tree Explorer**: Added Category for Erroneous Calls
* Added **ToggleBehaviourAction** working the same as ToggleGameObjectAction, but for behaviour components instead.
* **SendMessageBehaviour** (Timeline Send Message Tracks/Clips) now displays an instigator game object field in the inspector to be attached to the sent message.
* Added **VFXSetPropertyAction**
* Added **VFXSendEventAction**

#### Fixed

* Fixed `OnValidate` for FirstPersonController leading to infinite import loop when displaying the inspector for a Prefab Asset.
* Fix for null Callables in Callable Tree Window.

## 2019.3.0

* Feature Release
* Requires Unity 2019.3.0 or newer

#### Added

* **Call Tree Explorer :** Using Window/Gameplay Ingredients/Call Tree Explorer , opens a window that lists the tree of Events, Logic and Actions, State Machines and Event Calling Actions
* **Folders:** In the Game Object creation Menu, Select folder to add a folder in the hierarchy. Automatically adds Static Game Objects with colored icon (Displayed using Advanced Hierarchy View)
* **Global Variables System**:
  - Added Global Variables (Globals + Local Scope)
  - Added Global Variable Debug Window (`Window/Gameplay Ingredients/Globals Debug`)
  - Added Global Variable Set Action
  - Added Global Variable Logic
  - Added Global Variables Reset Action
* **Timers**:
  * Added Timer Component
  * Added TimerAction to control Timer
  * Added TimerDisplayRig
* Added option in GameplayIngredientsSettings to disable visibility of Callable[] bound to Update Loops.
* Added OnUpdate Event : Perform calls every update
* Added OnColider Event :  Perform calls upon collisions
* Added OnJoinBreak Event : Perform calls upon Rigid body joint break
* Added FlipFlop Logic : Two-state latch logic
* Added State Logic : Perform logic based on State Machine current state.
* Added Audio Mix Snapshot Action : Set Mixer Snapshots
* Added RigidBody Action : Perform actions on a rigidbody
* Added SetAnimatorParameterAction : Perform parameter setting on Animators
* Added Sacrifice Oldest option to Factory : When needing a new spawn but no slots left, sacrifices the first spawn of the list
* Added Context Menu in ToggleGameObjectAction to update entries based on current enabled state in scene. 

#### Changed

- Improved **Find & Replace** window, with a selection-based criteria.
- Moved Menu Items in Window menu into a Gameplay Ingredients Subfolder
- GameManager Resets Global Variables Local Scope on Level Load
- Updated NaughtyAttributes to a more recent version
- Renamed the Add Game Object menu category from `'GameplayIngredients' to 'Gameplay Ingredients'` and adjusted its contents

#### Fixed

* Fixed LinkGameView not working in play mode when excluding VirtualCameraManager.
* Fixed Performance issue in GameplayIngredientsSettings when having a big list of Excluded managers.
* Fixed ApplicationExitAction : Exits play mode when in Editor.

## 2019.1.2

#### Changed

* **[Breaking Change]** Discover Assets now reference many Scenes/SceneSetups 
  * Action to take: have to re-reference scenes in Discover Asset

#### Added

* Added Screenshot Manager (Defaults to F11 to take screenshots)
* Added OnMouseDownEvent
* Added OnMouseHoverEvent
* Added OnVisibilityEvent
* Added SaveDataSwitchOnIntLogic

#### Fixed

* Fixed warning in CycleResolutionsAction



## 2019.1.1

#### Changed

#### Added

* Log Action
* Added Playable Director to objects in discover (to open atimeline at a give playable director)
* Added support of Game Save Value index for Factories (in order to select a blueprint object from a saved value)

#### Fixed

* Fixed Import Errors at first project load, including the way we load discover and GameplayIngredients project settings

* Secure checks in Gathering Manager classes from assembly (skips protected assemblies now)

  

## 2019.1.0

#### Changed

* Removed counts in OnTriggerEvent
* Callables can now be friendly-named (with default formatting)
* Updated Starter Packages

#### Added

- Added NTimesLogic (split from OnTriggerEvent)
- Added Replace Mode for Level Streaming Manager
- Added UIToggle Action and Property Drawer
- Added Audio Play Clip Action
- Added Platform Logic

- New Welcome Screen, with Wizard
- New optional GameplayIngredients Project Configuration asset 
  - Toggles for verbose callable logging
  - Manager Exclusion List
- New Scene from Template Window + Config SceneTemplateLists Assets
  - Helps creating new scenes from user-made templates
- New Discover Window System:
  - Adds a new DiscoverAsset to reference Levels / Scene Setups
  - Adds new Discover components in scenes
  - Discover window helps navigate scenes while in editor and discover content.
- Added improved Game Manager
  - Manages loading of main menu & levels directly instead of using LevelStreamingManager
  - Manages Level Startup in sync after all scenes have started.

#### Fixed

* Fixed code to run on Unity 2019.1
* Fixed factory managed objects upon destroy
* Fixes in LinkGameView when application is not playing
* Fix in LevelStreamingManager incorrect computation of Scene Counts
* Fixes in VirtualCameraManager
* Fixes in Find/Replace window
* Fixes in Hierarchy View Hints for Unity 2019.3 new skin



## 2018.3.0

Initial Version
