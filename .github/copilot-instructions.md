# GitHub Copilot Instructions for Dogs mate (Continued) Mod

## Mod Overview and Purpose

**Dogs mate (Continued)** is an update and continuation of the Revoluss mod for RimWorld. The primary purpose of this mod is to enable cross-breeding among different breeds of cats, dogs, bunnies, bears, foxes, wolves, hedgehogs, fishers, martens, horses, and platypusses. It extends the vanilla system introduced in RimWorld 1.6, allowing animals of different breeds to mate and produce offspring that inherit traits from both parents.

## Key Features and Systems

- **Cross-breeding**: Allows different breeds of various animal species to mate and produce offspring.
- **Can Mate With Information**: Displays "Can mate with" information in the info-card for better visibility.
- **Compatibility Fixes**: Ensures that all animals are correctly configured for cross-breeding, even if another mod has dynamically added animals.
- **Support for Many Modded Animals**: Integrates with numerous popular animal mods to provide a comprehensive cross-breeding system.

## Coding Patterns and Conventions

- **C# Naming Conventions**: Use PascalCase for class and method names, and camelCase for local variables.
- **Code Organization**: Keep related functionalities in separate classes, such as `DogsMate.cs` for main functionality and `CrossBreedStatWorker.cs` for handling cross-breeding statistics.
- **Mod Compatibility**: Ensure the mod is compatible with other mods that do not alter the animal pregnancy system. Replace or extend vanilla methods like `JobGiver_Mate.TryGiveJob` and `Pregnant.DoBirthSpawn` as needed.

## XML Integration

- **Animal Group Definitions**: XML files like `Canidae.xml` and `Felidae.xml` define animal groups and their compatibility. Ensure these files are correctly structured to avoid conflicts with other mods.
- **Patch Files**: Patches for modded animals are located in the `Patches` directory. These files use Harmony patching to modify vanilla behavior or add compatibility with other mods.

## Harmony Patching

- **Harmony Usage**: The mod uses Harmony for patching vanilla methods. Ensure that patches are correctly targeted to avoid breaking other mods.
- **Patch Structure**: Each patch file should be well-documented and organized. Use comments to explain the purpose of each patch and the changes it makes.

## Suggestions for Copilot

- **XML File Generation**: Generate XML files for new animal groups or patches. Ensure the structure is consistent with existing files.
- **Harmony Patch Templates**: Provide templates for creating new Harmony patches. Include comments explaining the purpose of each patch.
- **Code Snippets**: Offer code snippets for extending functionality, such as adding new animal types or modifying existing behavior.
- **Compatibility Checks**: Include checks for compatibility with other mods. Ensure that patches do not conflict with other modded systems.
- **Debugging Tips**: Provide tips for debugging Harmony patches and XML configurations. Include instructions for checking logs and using tools like RimSort to manage mods.

By following these guidelines, you can contribute effectively to the **Dogs mate (Continued)** mod, ensuring that the code remains clean, maintainable, and compatible with other RimWorld mods.

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
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

