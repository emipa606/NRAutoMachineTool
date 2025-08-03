# GitHub Copilot Instructions for NR_AutoMachineTool (Continued) Mod

## Mod Overview and Purpose

The NR_AutoMachineTool mod aims to enhance automation within the RimWorld game by adding various machines that can perform tasks autonomously. This mod is an update and continuation of the original mod created by nullres, and it features improved power-output information and a visible shield bubble for the shield generator. The overall goal is to reduce micromanagement and improve efficiency in the colony with automated systems.

## Key Features and Systems

- **Automated Machines**: Introduce a suite of machines that perform specific tasks, such as AutoMachineTool, BeltConveyor, Item Puller, Planter, Harvester, Slaughterhouse, AnimalResourceGatherer, etc.
- **Shield Generator**: Provides a visible shield bubble for enhanced defense mechanisms.
- **Industrial Automation**: Implements system features inspired by mods such as S.A.L.: Auto-crafters, Industrial Rollers, and Project RimFactory to create a seamless automated workflow.
- **Visual Enhancements**: Includes graphical elements for various machines such as the WallLight and BeltConveyor to improve the in-game experience.

## Coding Patterns and Conventions

- **Class Inheritance**: Utilize C# inheritance with abstract base classes (e.g., `BaseMachineSetting`, `BaseTargetCellResolver`) to allow flexible and extendable machine logic.
- **Interface Implementation**: Follow a modular design by implementing interfaces (e.g., `IMachineSetting`, `ITargetCellResolver`) that can be shared across multiple classes for uniform behavior across diverse machine types.
- **Naming Conventions**: Classes and methods follow PascalCase naming conventions while interfaces begin with an 'I' prefix.

## XML Integration

- XML is used primarily for defining mod content such as machine definitions, properties, and recipes.
- Ensure that the XML files are properly structured and synchronized with C# code to maintain mod balance and performance.
- Use XML extensions when necessary to add custom properties to Defs.

## Harmony Patching

- Utilize Harmony to patch existing game methods for compatibility and new features integration. This allows the mod to inject additional logic without modifying the base game files.
- Ensure patches are well-documented and tested to avoid conflicts and maintain performance.
- Use Harmony prefixes and postfixes to execute code before or after original game methods.

## Suggestions for Copilot

1. **Boilerplate Code**: Assist with generating boilerplate code for new machines, using existing classes as a reference for patterns and interfaces.

2. **XML Definitions**: Propose properly structured XML definitions when introducing new machines or updating existing ones to ensure balance and functionality.

3. **Harmony Patches**: Provide examples and templates for common Harmony patch patterns to facilitate bug fixing and feature enhancements.

4. **Performance Optimization**: Suggest code optimizations, especially for loops and 3D vector calculations used in machine logic, to enhance game performance.

5. **Documentation**: Generate inline comments and summaries for better code readability, especially when introducing new logic or modifying existing systems.

---

For further assistance or questions related to mod development, feel free to reach out via the GitHub Issues section with feature requests or bug reports.

Special thanks to Zymex and the producers of the referenced mods for their inspiration and contributions. Enjoy automating your RimWorld colony!


This document is designed to assist developers in understanding the key technical elements of the NR_AutoMachineTool mod, ensuring effective use of GitHub Copilot for quick and efficient mod development.
