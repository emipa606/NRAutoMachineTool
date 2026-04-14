# Copilot Instructions for NR_AutoMachineTool

## Mod Overview and Purpose

**NR_AutoMachineTool** is a continuation of the original nullres mod dedicated to enhancing automation within the game RimWorld. This mod introduces a variety of machines designed to streamline your colony's operations, ranging from item handling to cleaning and mining. The primary objective is to reduce the manual workload by employing automated systems.

### Key Features and Systems

- **Automation Machines**: Includes AutoMachineTool, BeltConveyor, Item Puller, Planter, Harvester, Slaughterhouse, AnimalResourceGatherer, AutoMiner, AutoCleaner, WallLight, AutoRepairer, Stunner, ShieldGenerator, Material Converter, and PowerfulPowerPlant.
- **Visual Feedback**: Enhanced user experience with elements like a visible shield bubble for the ShieldGenerator.
- **Power Management**: Accurate power-output information for better resource planning.
- **Machine Translation**: Original content is in Japanese with machine translation to English, allowing broader accessibility.

### Coding Patterns and Conventions

- **Class Structuring**: Classes are designed with clear lines of inheritance, like `Building_BaseMachine<T>` extending to more specific machines.
- **Interface Implementation**: Interfaces such as `IMachineSetting`, `ITargetCellResolver`, and `IPowerSupplyMachine` are employed to ensure consistent behavior and functionalities across different machine types.
- **Private Methods**: Encapsulate functionalities that are not required to be exposed, enhancing the modular design.
- **Consistent Naming**: Classes and methods maintain descriptive and consistent naming conventions for easy understanding and maintenance.

### XML Integration

- Utilize XML to define machine properties and behaviors within the mod.
- XML files manage the definitions and settings for each machine type, facilitating ease of configuration changes without modifying the core logic.
- Ensure XML compatibility with existing game data structures to prevent errors.

### Harmony Patching

- Harmony is employed to modify existing game logic non-destructively, ensuring compatibility with updates and other mods.
- Implement Harmony patches to extend or override game behaviors to fit the automation mechanics of NR_AutoMachineTool.
- Ensure that patches do not conflict with other mods by checking method owners and implementation logic.

### Suggestions for Copilot

1. **Code Completion**: Assist with completing class methods that interface with RimWorld's engine, particularly focusing on machine logic.
2. **Pattern Recognition**: Identify repeating structures or logic to suggest refactoring or abstraction opportunities.
3. **Translation**: Provide native-level translations or corrections for machine-translated components.
4. **Error Handling**: Suggest robust error handling techniques, particularly around exceptions that could arise during automation processes.
5. **Performance Optimization**: Advise on optimizing the existing algorithms, especially those running in frequent intervals like ticks.
6. **Compatibility Suggestions**: Proactively suggest checks or balances to maintain compatibility with future RimWorld updates or community mods.

By leveraging these instructions, GitHub Copilot can effectively assist in further developing and refining the NR_AutoMachineTool mod, ensuring it's robust, efficient, and user-friendly.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
