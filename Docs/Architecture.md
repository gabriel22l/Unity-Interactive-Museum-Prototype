# Architecture

## 1. Overview

Unity 6 3D educational prototype: an interactive museum of pre-Hispanic cultures with a separate artifact-collection gameplay scene. It is built from Unity MonoBehaviours, ScriptableObject data definitions and event-driven communication between systems, with no singletons or global state.

The project follows a loosely coupled, component-driven design:

- Scene orchestration is handled by manager/controller classes.
- Player state and shared references are centralized with `PlayerContext`.
- Inventory and objective logic communicate through events.
- Enemy behavior is modeled as a finite state machine.
- Interaction is implemented through the `IInteractable` interface.
- Data definitions such as items and objectives are authored as ScriptableObjects.
- The museum scene also runs in VR, reusing the same player systems through a Prefab Variant of the desktop player.

This architecture makes the project easy to extend for additional museum objects, objectives, or enemy behaviors without introducing global state.

## 2. Runtime architecture

At runtime, the main flow is:

1. `GameManager` initializes the player and UI systems.
2. `PlayerContext` wires together input, interaction, inventory, health, and the player camera/view system.
3. `PlayerInput` listens to Unity Input System actions and raises gameplay events.
4. `PlayerInteraction` constantly scans for nearby interactables and executes them on input.
5. `InventoryController` tracks collected items and raises `OnInventoryChanged` for objectives and UI.
6. `ObjectiveTracker` evaluates the current objective and advances when complete.
7. `UIController` updates menus, health, inventory widgets, and objective labels.
8. `Health` drives death/win/lose conditions and invokes game-over flow.
9. `EnemyAIController` patrols, detects the player, chases, and damages the player while in contact.

## 3. Project structure

The game logic lives primarily under `Assets/Scripts` and is grouped by feature:

- `GameManager.cs` – overall game orchestration
- `ApplicationController.cs` – scene loading, resolution and exit helpers (static, at the scripts root)
- `PauseControl.cs`, `IPauseControl.cs` – pause state, owned by `GameManager`
- `VR/` – VR-only helper and platform fix
- `Player/` – input, movement, view switching, animation integration
- `Interaction/` – object interaction model and detection
- `Inventory/` – item data, storage, UI, and item world objects
- `Objectives/` – objective data model and tracker
- `Health/` – health management and feedback handling
- `AI/` – enemy behavior and projectile logic
- `UI/` – menus, gameplay interface, the page-manager interface and gamepad navigation helpers (`IUIPageManager`, `UINavigationSelector`, `UISelectableColorSetter`)
- `Audio/` – reusable sound playback helpers

## 4. Core systems

### 4.1 Game orchestration: `GameManager`

File: `Assets/Scripts/GameManager.cs`

What it does:
- Connects the main game systems at startup.
- Initializes `PlayerContext`, `InventoryViewModel`, `UIController`, and `ObjectiveTracker`.
- Watches player death and objective completion.
- Triggers win/lose menus.
- Owns pause state through PauseControl (IPauseControl), which UIController uses to request and release pauses.

How it does it:
- Runs in `Awake()` and subscribes to `Health.OnDeath` and `ObjectiveTracker.OnAllObjectivesCompleted`.
- Creates `ObjectiveContext` and passes the inventory into the objective system.
- Sets `isGameOver` to prevent duplicate end states.

Connected systems:
- `PlayerContext`
- `UIController`
- `ObjectiveTracker`
- `Health`
- `InventoryController`

This is the top-level coordinator that binds systems together without relying on a singleton pattern.

### 4.2 Player context and shared access: `PlayerContext`

File: `Assets/Scripts/Player/PlayerContext.cs`

What it does:
- Acts as the central access point for player-related systems.
- Gives other systems a single place to reach the player's input, inventory, interaction, view, health, and audio references.

How it does it:
- Exposes serialized subsystem references as properties.
- Calls `PlayerInput.Initialize(this)` and `playerInteraction.Initialize(this, PlayerInput, interactableIndicatorController)` in `Initialize()`.

Connected systems:
- `PlayerInput`
- `InventoryController`
- `PlayerInteraction`
- `PlayerViewController`
- `Health`
- `AudioEmitter`
- `UIController`

This pattern is the project's main dependency hub: systems ask the player context for the subsystem they need instead of finding references ad hoc.

### 4.3 Input and control flow: `PlayerInput`

File: `Assets/Scripts/Player/PlayerInput.cs`

What it does:
- Maps Unity Input System actions to gameplay events.
- Tracks movement, sprinting, interaction input, menu toggles, view switching, and throwing.
- Switches between gameplay and UI action maps when menus open/close.
- Serves keyboard/mouse, gamepad and VR controller input through the same actions.

How it does it:
- Creates an `InputActions` instance in `Awake()`.
- Subscribes to per-action callbacks including `Movement`, `Run`, `Interact`, `OpenPlayerMenu`, `OpenPauseMenu`, and `ToggleView`.
- Raises game events such as `InteractEvent`, `OnPlayerMenuOpen`, `OnPauseMenuOpen`, `CloseUIPressed`, and `OnThrowInput`.
- Disables or enables Cinemachine input controllers based on UI open state.
- XR controller bindings (trigger for Interact, grip for CloseUI) are added to the existing actions, so no extra events or code paths are needed for VR.

Connected systems:
- `UIController`
- `PlayerViewController`
- `PlayerInteraction`
- `PlayerMovement`
- `PlayerMovementFP`

This is the game's primary input gateway and also enforces UI/gameplay mode transitions.

### 4.4 Player movement and camera view: `PlayerMovement`, `PlayerMovementFP`, `PlayerViewController`

Files:
- `Assets/Scripts/Player/PlayerMovement.cs`
- `Assets/Scripts/Player/PlayerMovementFP.cs`
- `Assets/Scripts/Player/PlayerViewController.cs`

What they do:
- Move the character in response to input.
- Support first-person and third-person camera modes.
- Preserve velocity when switching camera modes.

How they do it:
- `PlayerMovement` uses a `CharacterController`, camera-relative movement direction, gravity, acceleration, and sprint multiplier.
- `PlayerViewController` toggles between `FirstPerson` and `ThirdPerson` states by enabling/disabling different movement scripts and Cinemachine cameras.
- It swaps active render state and camera priorities while preserving movement velocity between views.

Connected systems:
- `PlayerInput`
- `PlayerAnimation`
- `Cinemachine` virtual cameras
- `Health` indirectly through game-over flow

This system gives the player both immersive FP gameplay and a more observant TP explore mode. These scripts are desktop-only: the VR player does not use them (see 4.14).

### 4.5 Interaction system: `PlayerInteraction`, `IInteractable`, indicator UI

Files:
- `Assets/Scripts/Interaction/PlayerInteraction.cs`
- `Assets/Scripts/Interaction/IInteractable.cs`
- `Assets/Scripts/Interaction/ItemIndicatorController.cs`
- `Assets/Scripts/Interaction/InteractableInfoPanel.cs`

What it does:
- Detects world objects the player can interact with.
- Selects the nearest valid interactable and executes its interaction logic.
- Displays indicator and info-panel feedback for nearby items.

How it does it:
- `PlayerInteraction.Update()` performs a forward spherical cast to find objects in range. In VR the ray origin points at the right controller transform.
- `GetInteractable()` chooses the closest valid target using `IInteractable`.
- `GetInteractablesInRange()` gathers nearby valid interactables and passes them to `InteractableIndicatorController`.
- `OnInteract()` invokes `currentInteractable.Interact(interactionContext)`.
- `InteractionContext` carries the `PlayerContext` so interactables can access inventory, health, or audio while interacting.

Connected systems:
- `ItemWorldObject`
- `InventoryController`
- `AudioEmitter`
- `UIController`
- `PlayerContext`

This is the project's object interaction layer. Most pickup, inspect, and world-object behaviors are implemented by objects that satisfy `IInteractable`.

### 4.6 Inventory system: `InventoryController`, `InventoryUIController`, `InventoryViewModel`, `InventorySlot`, `itemDataSO`

Files:
- `Assets/Scripts/Inventory/InventoryController.cs`
- `Assets/Scripts/Inventory/InventoryUIController.cs`
- `Assets/Scripts/Inventory/InventoryViewModel.cs`
- `Assets/Scripts/Inventory/InventorySlot.cs`
- `Assets/Scripts/Inventory/ScriptableObjects/itemDataSO.cs`
- `Assets/Scripts/Inventory/ResourceCost.cs`

What it does:
- Stores and manages the player's inventory.
- Adds, stacks, drops, swaps, removes, and counts items.
- Supports resource cost checks for crafting or other gameplay rules.
- Provides a view model to the UI layer.

How it does it:
- `InventoryController.Awake()` initializes an array of `InventorySlot` entries.
- `AddItem()` attempts to stack into existing slots and then fills new ones.
- `TryStacking()`, `SwapItem()`, `TryMerging()`, and `RemoveResource()` maintain slot consistency.
- `itemDataSO` acts as a data definition for a type of item with a name, icon, stack size, description, prefab, and category.
- `OnInventoryChanged` is raised after inventory changes so objectives and UI can react.

Connected systems:
- `ItemWorldObject` for pickups
- `ObjectiveTracker` / `CollectArtifactsObjective`
- `UIController`
- `InventoryUIController`
- `ObjectiveContext`

This is the project's main stateful game data container, and it acts as the source of truth for collectible progression.

### 4.7 Objective system: `ObjectiveTracker`, `Objective`, `ObjectiveSO`, `CollectArtifactsObjective`

Files:
- `Assets/Scripts/Objectives/ObjectiveTracker.cs`
- `Assets/Scripts/Objectives/Objective.cs`
- `Assets/Scripts/Objectives/CollectArtifactsObjective.cs`
- `Assets/Scripts/Objectives/ObjectiveScriptableObjects/ObjectiveSO.cs`
- `Assets/Scripts/Objectives/ObjectiveScriptableObjects/CollectArtifactsObjectiveData.cs`
- `Assets/Scripts/Objectives/UI/ObjectiveUI.cs`

What it does:
- Tracks and sequences gameplay goals.
- Keeps the active objective up-to-date.
- Advances to the next objective or emits a completion event when the last objective is complete.

How it does it:
- `ObjectiveTracker.Initialize()` creates objective instances from a list of `ObjectiveSO` scriptable objects.
- Each objective derives from the abstract `Objective` class and implements `Initialize()`, `Cleanup()`, `GetProgressText()`, and `Description`.
- `CollectArtifactsObjective` listens to `InventoryController.OnInventoryChanged` and updates progress based on the count of artifact items in the inventory.
- `OnObjectiveChanged` and `OnObjectiveProgress` drive the UI, while `OnAllObjectivesCompleted` triggers game win flow.

Connected systems:
- `InventoryController`
- `UIController`
- `GameManager`
- `ObjectiveUI`

The objective framework is data-driven and extensible: adding a new objective mainly requires a new `ObjectiveSO` subclass and matching `Objective` implementation.

### 4.8 Health and damage system: `Health`, `EnemyDamage`, `UIHealthBar`, `PlayerDamageFeedback`

Files:
- `Assets/Scripts/Health/Health.cs`
- `Assets/Scripts/Health/UI/UIHealthBar.cs`
- `Assets/Scripts/Health/PlayerDamageFeedback.cs`
- `Assets/Scripts/AI/EnemyDamage.cs`

What it does:
- Stores and modifies health values.
- Issues damage, healing, death event callbacks, and temporary invulnerability.
- Displays health UI and triggers visual/audio damage feedback.

How it does it:
- `Health` uses a current/max health pair, clamps values, and raises `OnHealthChanged`, `OnDamageTaken`, and `OnDeath` events.
- `TakeDamage()` blocks if the unit is invulnerable, dead, or forbidden from taking damage.
- `EnemyDamage.OnTriggerStay()` applies damage to a `Health` component if the enemy collider is touching the player and `CanDamage` is enabled.
- `UIHealthBar` listens to `OnHealthChanged` and updates the display.

Connected systems:
- `GameManager` (death -> game over)
- `UIController`
- `EnemyAIController`
- `PlayerDamageFeedback`

This is the main gameplay survival layer. A low-health or zero-health state leads directly to the end-game flow.

### 4.9 Enemy AI system: `EnemyAIController`

File: `Assets/Scripts/AI/EnemyAIController.cs`

What it does:
- Controls enemy patrol, chase, search, and idle behavior.
- Detects the player via sphere overlap and field-of-view checks.
- Uses navigation to move between points and respond to losing sight of the player.
- Reacts to a thrown offering (OfferingProjectile) by returning to its spawn point and going idle.

How it does it:
- Uses a `NavMeshAgent` and a finite state machine with states: `Patrol`, `Chase`, `Search`, and `Idle`.
- `HandleVision()` tests whether the player appears inside a vision radius and not blocked by obstacles.
- `Patrol()` picks random waypoints.
- `Chase()` moves toward the player and exits to `Search` after a lose-sight delay.
- `Search()` creates random nearby search targets and tries to follow them before returning to patrol.
- `OnOfferingHit()` teleports the enemy back to its spawn point and sets the state to `Idle`.

Connected systems:
- `Health` via `EnemyDamage`
- `OfferingProjectile`
- `NavMesh` navigation
- `Player` transform / player layer mask
- `GameManager` indirectly via death flow

This is the project's primary gameplay hazard and the main reason the player must manage threat and objective progression together.

### 4.10 Throwable/distraction mechanics: `OfferingProjectile`

File: `Assets/Scripts/AI/OfferingProjectile.cs`

What it does:
- Represents a thrown offering object.
- When it collides with an enemy, OnOfferingHit() sends the enemy back to its spawn point in the Idle state.

How it does it:
- `Initialize(force, direction)` applies impulse to a Rigidbody.
- `OnCollisionEnter()` checks whether the collider belongs to an enemy; if so, it calls `enemyAIController.OnOfferingHit()` and destroys itself.
- If it hits a non-player world object and is not recoverable, it is destroyed; otherwise, it can become interactable again.

Connected systems:
- `EnemyAIController`
- `ItemWorldObject`
- `PlayerThrow` / player throw input
- `InventoryController` indirectly via item data and throwable items

This is a lightweight stealth/trap interaction.

### 4.11 Audio system: `AudioEmitter`, `FootstepAudioEvents`

Files:
- `Assets/Scripts/Audio/AudioEmitter.cs`
- `Assets/Scripts/Audio/FootstepAudioEvents.cs`

What it does:
- Plays one-shot sound effects for pickups, movement, and other object events.
- Prevents sound spam with a minimum time-between-sounds threshold.

How it does it:
- `AudioEmitter` chooses a random audio clip and uses an `AudioSource` to play it.
- `FootstepAudioEvents` attaches to a character and triggers footsteps based on animation events, sampling terrain/ground audio based on raycast hit results.

Connected systems:
- `ItemWorldObject` (pickup sounds)
- `PlayerAnimation` / animation events
- `Player` movement and environment surfaces

This is a lightweight sound-service layer reused by many object types.

### 4.12 UI and menu system: `UIController`, `MenuController`, `MainMenu`, `MenuResolutionDropdown`, `ObjectiveUI`, `UIHealthBar`

Files:
- `Assets/Scripts/UI/UIController.cs`
- `Assets/Scripts/UI/MenuController.cs`
- `Assets/Scripts/UI/MainMenu.cs`
- `Assets/Scripts/UI/MenuResolutionDropdown.cs`
- `Assets/Scripts/UI/IUIPageManager.cs`
- `Assets/Scripts/UI/UINavigationSelector.cs`
- `Assets/Scripts/UI/UISelectableColorSetter.cs`
- `Assets/Scripts/Health/UI/UIHealthBar.cs`
- `Assets/Scripts/Objectives/UI/ObjectiveUI.cs`

What it does:
- Manages the HUD, menus, info panel and end-game windows.
- Connects player input to menu opening/closing.
- Displays health and objective progress in real time.
- Supports gamepad menu navigation without per-menu code.

How it does it:
- `UIController.Initialize()` subscribes to player input events and sets up the inventory, health and objective widgets.
- `SetActiveMenu()` swaps the active menu, locks/unlocks the cursor, optionally requests a pause through IPauseControl when allowPause is set and raises `OnUIOpen` / `OnPageChanged`.
- `OnMenuClose()` hides the current menu, releases the pause request, restores the gameplay cursor state, and raises `OnUIClose` / `OnUIClosed`.
- `OpenInfoPanelMenu(itemDataSO)` shows the info panel for an inspected item. The panel logic is the same on desktop and VR; only the Canvas differs.
- `OpenLoseGameOverMenu()` and `OpenWinGameOverMenu()` trigger a delayed fade-in panel, and `isGameOver` blocks other menus afterwards.
- `MainMenu` is a page-based menu (main, play, options, controls). It generates the controls list from the `InputActionAsset`, delegates scene loading and exit to `ApplicationController`, and raises `OnPageChanged` on each page switch.
- `MenuResolutionDropdown` fills a `TMP_Dropdown` with the available resolutions, selects the current one and applies changes through `ApplicationController`.

Connected systems:
- `PlayerInput`
- `GameManager`
- `InventoryUIController`
- `ObjectiveTracker`
- `Health`
- `ApplicationController`

This is the presentation and state-flow layer for the game's user experience.

#### Page manager contract: `IUIPageManager`

```csharp
event Action OnPageChanged;
event Action OnUIClosed;
GameObject CurrentPage { get; }
```

Implemented by `UIController` (`CurrentPage` is the active menu) and `MainMenu`. UI helpers depend on this interface instead of on either concrete class.

#### Gamepad navigation: `UINavigationSelector`, `UISelectableColorSetter`

Both components must sit on the same GameObject as the `IUIPageManager`.

- `UINavigationSelector`
    - On `OnPageChanged`, selects the page's last-selected `Selectable` (validated as active and interactable), falling back to the first valid `Selectable`. If none exists, it clears the selection.
    - Tracks the last selection per page in `Update()`.
    - On `OnUIClosed`, clears the selection.
    - If the selection is lost (e.g. a mouse click on empty space), it reselects on the next move/submit input from `InputSystemUIInputModule`.
- `UISelectableColorSetter`
    - Applies a shared selected/highlighted color to every `Selectable` on a page the first time that page is shown.

Gamepad support is partial by design: inventory slots have no navigation, and drag-and-drop and hover info are mouse-only.

### 4.13 Application services: `ApplicationController`

File: `Assets/Scripts/ApplicationController.cs`

What it does:
- Holds non-UI application logic so menu MonoBehaviours only handle UI concerns.
- Resolution: `GetAvailableResolutions()`, `SetResolution()`, `GetCurrentResolution()`, `GetNativeResolution()`.
- Scene flow: `LoadScene()`, `ReloadScene()`, `ExitGame()`.

How it does it:
- It is a static class with no state, which fits the project's no-singleton approach.
- `GetAvailableResolutions()` returns resolutions matching the native display's aspect ratio (tolerance 0.02), deduplicated by width x height and ordered highest first.
- `SetResolution()` keeps the current fullscreen mode.
- `LoadScene()` and `ReloadScene()` reset `Time.timeScale` to 1 so a paused game cannot carry over into the next scene.
- `ExitGame()` stops play mode in the Editor and calls `Application.Quit()` in builds.

Connected systems:
- `MainMenu`
- `MenuResolutionDropdown`

### 4.14 VR integration

Files:
- `Assets/Scripts/VR/VRFollowCamera.cs`
- `Assets/Scripts/VR/FixOpenXrDpad.cs`

Stack: Unity 6.3 LTS (6000.3.8f1), XR Interaction Toolkit 3.2.3, OpenXR 1.16.1, Input System 1.18, XR Plug-in Management 4.5.3.

What it does:
- Runs the museum scene in VR (`Museum_VR`) using the existing player systems.
- Keeps desktop and VR behavior in sync by reusing the shared scripts.

How it does it:
- The VR player is a Prefab Variant of the desktop Player, parented under the XR Origin (XR Rig). It reuses `PlayerContext`, `PlayerInput` and `PlayerInteraction`.
- Desktop-only components are removed from the variant: `PlayerViewController`, `PlayerAnimation` and the FP/TP movement scripts.
- Movement and turning come from the XRI Dynamic Move Provider (continuous move) and Snap Turn, with a locomotion vignette for comfort.
- XR controller bindings are added to the existing input actions (trigger -> Interact, grip -> CloseUI), so no gameplay code changed.
- The info panel uses a separate world-space Canvas (prefab variant), which also accepts mouse/gamepad clicks for use without a headset.
- `VRFollowCamera` keeps that Canvas in front of the head at a fixed offset. It uses frame-rate-independent exponential smoothing for position and rotation, updated in `LateUpdate()`.
- `FixOpenXrDpad` is a runtime fix (`BeforeSceneLoad`, Windows/Editor only). OpenXR's D-Pad feature overrides the Input System `DPad` layout, so gamepads fail to register. When `DPad` is found to derive from `XRController`, the fix removes the layout, re-registers `DpadControl` and re-adds the affected devices.

Connected systems:
- `PlayerContext`, `PlayerInput`, `PlayerInteraction`
- `UIController` (info panel)
- XR Interaction Toolkit and the Input System

## 5. Cross-system relationships

The project is built around a few key dependency flows:

- `GameManager` -> `PlayerContext` -> `PlayerInput`, `InventoryController`, `Health`, `PlayerInteraction`, `PlayerViewController`
- `PlayerInteraction` -> `IInteractable` -> `ItemWorldObject`
- `InventoryController` -> `OnInventoryChanged` -> `CollectArtifactsObjective` -> `ObjectiveTracker` -> `GameManager`
- `Health` -> `OnDeath` -> `GameManager` -> `UIController` -> end-game menus
- `PlayerInput` -> `UIController` action map switching and menu events
- `EnemyAIController` -> `Health` through `EnemyDamage` contact damage
- `OfferingProjectile` -> `EnemyAIController.OnOfferingHit()`
- `UIController` -> `ObjectiveUI`, `UIHealthBar`, `InventoryUIController`
- `MainMenu` / `UIController` -> `IUIPageManager` -> `UINavigationSelector`, `UISelectableColorSetter`
- `MainMenu`, `MenuResolutionDropdown` -> `ApplicationController`
- `PlayerInput` (XR bindings) -> `PlayerInteraction` / `UIController`, using the same events as keyboard and gamepad
- `VRFollowCamera` -> `Camera.main` -> VR info panel Canvas

## 6. Data-driven design

A significant architectural feature of the project is its use of ScriptableObjects:

- `itemDataSO` defines all item metadata.
- `ObjectiveSO` defines objective configuration.

This lets the project create inventory and objective content without hard-coded checks in scene logic. It also makes the project easier to extend for new item types or new objectives.

## 7. Architectural patterns used

- Event-driven communication: inventory updates, objective completion, UI state changes, and health events.
- Component-driven gameplay: player and world objects are mostly composed from Unity components.
- Data-driven content: items and objectives are scriptable content assets.
- Manager/controller orchestration: `GameManager`, `UIController`, `ObjectiveTracker`, `InventoryController` coordinate subsystems.
- Finite state machine: `EnemyAIController` uses explicit patrol/chase/search/idle states.
- Context-based dependency access: `PlayerContext` centralizes subsystems for scene objects.
- Interface-based UI helpers: `IUIPageManager` lets navigation and styling components work with any page-based UI.
- Prefab Variants for platform differences: the VR player and VR Canvas are variants of their desktop counterparts, so shared scripts stay in sync.

## 8. Technical constraints and design assumptions

- No backend or save system is present.
- The game is scene-local and prefab-based, not modularly separated into services or assemblies.
- Most communication is synchronous and direct, not asynchronous or message-bus based.
- The game is designed as a presentation prototype, so runtime architecture favors simplicity and readability over enterprise-scale patterns.
- VR support covers the museum scene only (standalone mobile build profile). The gameplay scene and main menu are not VR-compatible.
- Gamepad support is partial: inventory drag-and-drop and hover info are mouse-only.
- Resolution settings are not persisted between sessions.

## 9. Summary

A small set of managers coordinates a player-centric system, a data-driven inventory and objective model, a state-machine enemy controller and a menu/HUD layer. The museum scene also runs in VR by reusing the same player systems. The result is a cohesive museum-exploration prototype with a clear separation between gameplay simulation, UI, and world interactions, now extended with gamepad-friendly menus, a static application-services class, and a VR path for the museum scene that reuses the same player systems.