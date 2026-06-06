# Copilot Instruction File for NR_AutoMachineTool (Continued)

## Mod Overview and Purpose
NR_AutoMachineTool (Continued) is a RimWorld mod focused on introducing advanced machinery to automate various tasks within the game. It is an update of the original mod by nullres, featuring enhanced functionalities and bug fixes. The mod aims to streamline in-game activities like crafting, farming, mining, and resource management, allowing players to automate these processes and focus on other strategic aspects of the game.

## Key Features and Systems
- **Automation Machines**: Introduces multiple new machines for automation, including AutoMachineTool, BeltConveyor, Item Puller, Planter, Harvester, Slaughterhouse, AnimalResourceGatherer, AutoMiner, AutoCleaner, WallLight, AutoRepairer, Stunner, ShieldGenerator, Material Converter, and PowerfulPowerPlant.
- **Shield Building**: Adds a visible shield bubble to the shield generator to provide a clear indication of protected areas.
- **Power Management**: Corrects and updates the power-output information for better resource management.

## Coding Patterns and Conventions
- **Naming Conventions**: Consistent use of PascalCase for type names and method names. Public properties and fields also follow PascalCase, while private fields use camelCase prefixed with an underscore (_).
- **File Organization**: C# source files are organized by functionality, with one major type per file.
- **Defining Methods**: Methods such as `DrawModSetting`, `DrawPower`, and `ExposeData` provide customization and data serialization for machines.

## XML Integration
- XML files are used extensively for defining new game elements and attributes specific to the mod.
- Various XML definitions include `DesignationCategoryDef`, `EffecterDef`, `HediffDef`, and `ResearchProjectDef` to integrate new machinery, effects, health conditions, special filters, and research projects into the game.
- Ensure correct XML syntax and structure for seamless integration with RimWorld’s game engine.

## Harmony Patching
- Harmony is used for runtime patching to modify existing game methods and behaviors.
- Carefully crafted prefix, postfix, and transpiler methods must be created to ensure compatibility with base game mechanics and other mods.
- Use appropriate attributes and method signatures to target necessary game functions for patching.

## Suggestions for Copilot
- **Machine Setup**: Assist in creating configuration and initialization methods for new machinery, including power specifications and automation logic.
- **Performance Optimization**: Recommend efficient coding practices to manage performance impact from multiple automated processes running simultaneously.
- **Error Handling**: Provide patterns and techniques for robust error handling, especially with integrating numerous machines and Harmony patches.
- **Localization Support**: Assist with adding proper localization support for new content to make the mod accessible to a wider audience.
- **Testing and Debugging**: Generate unit tests and debugging methods specific to the features and functionality of the NR_AutoMachineTool mod.

By following these instructions, developers working on NR_AutoMachineTool (Continued) can leverage GitHub Copilot to improve and extend the mod's capabilities, ensuring a rich and seamless automation experience for players of RimWorld.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).

