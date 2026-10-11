# ChangeLog
Versions are development milestones, not public releases.

## v0.3

**VR**
- Added partial VR support: `Museum_VR` scene is functional and ready for a self-contained, single-scene build
- Set up the XR stack: Unity 6.3 LTS (6000.3.8f1), XR Interaction Toolkit 3.2.3, OpenXR 1.16.1, Input System 1.18.0, XR Plugin Management 4.5.3 (installed as a dependency)
- Integrated VR with the existing player systems: the VR player is a Prefab Variant of the desktop Player, child of the XR Origin (XR Rig)
    - Reuses `PlayerContext`, `PlayerInput` and `PlayerInteraction`
    - Removed desktop-only components (`PlayerViewController`, `PlayerAnimation`, FP/TP movement)
- Locomotion uses the XRI Dynamic Move Provider (continuous move) with Snap Turn
- Added XR controller bindings to the existing input actions: Right Controller Primary Button -> Interact, Menu Button -> CloseUI/OpenPauseMenu
- Added a world-space VR Canvas (prefab variant) for the VR UI
- Added `VRFollowCamera`: the panel follows and orients to the head with frame-rate-independent smoothing
- Added a locomotion vignette
- Created a standalone VR (mobile) build profile with the museum scene only
- Optimized mobile_RP settings and baked lighting for the VR museum scene
- Tested in the XR Interaction Simulator

**Environment**
- Tried stone textures on `Museum_VR` scene; reverted to plain colors

**Input / Gamepad**
- Project is now playable with a gamepad from start to finish (drag-and-drop and hover info omitted)
- Fixed low gamepad look sensitivity with a Scale Vector2 processor on the binding
- Added `FixOpenXrDpad`, a runtime fix for the OpenXR/Input System `DPad` layout conflict that broke gamepad detection (Windows/Editor only)

**UI / Menus**
- Added the `IUIPageManager` interface (`OnPageChanged`, `OnUIClosed`, `CurrentPage`), implemented by `MainMenu` and `UIController`
- Added `UINavigationSelector`: auto-selects the first interactable on page change, remembers the last selection per page, and re-selects when selection is lost
- Added `UISelectableColorSetter`: applies the selected/highlighted colors to the selectables on each page
- Updated `UIController` and `MainMenu` to raise the new page events
- Moved cursor visibility and state handling to a static method in `ApplicationController.cs`
- Organization and readability improvements in `UIController.cs`

**Architecture / Refactoring**
- `PlayerContext` now initializes `InventoryViewModel` in `Awake()`; `GameManager` now initializes in `Start()` instead of `Awake()`
- Encapsulated `ObjectiveContext` initialization in its constructor, simplifying objective system setup in `GameManager.cs`
- Refactored pause handling: added `IPauseControl.cs` and `PauseControl.cs`; `GameManager` initializes and owns `PauseControl`, and `UIController` receives the `IPauseControl` reference and requests pauses through it

**Application / Builds**
- Added the static `ApplicationController` (resolutions, scene loading, exit) and moved non-UI logic out of the menu classes
- Added `MenuResolutionDropdown` and a resolution option in the main menu
- Made a standalone desktop build (with resolution settings, without VR)