# GitHub Copilot Instructions for Food Alert (Continued) Mod

## Overview
Food Alert (Continued) is a RimWorld mod that provides a persistent readout of the current amount of food in storage and an estimate of how long stocks will last. It helps prevent sudden starvation by tracking food supplies. This mod is an update of Mehni's original Food Alert mod and includes several new features and improvements.

## Key Features and Systems
- **Persistent Food Readout:** Displays the current food stock and estimated duration before running out.
- **Configuration Options:** Allows users to set the lowest preferability of food to be counted and configure meal ingredient inclusion.
- **Improved Food Calculations:** Uses exact amounts required by pawns and ignores those that don't eat.
- **Language Support:** Includes translations in various languages, including Russian, Italian, Spanish, German, and Chinese.
- **Compatibility with Other Mods:** Works with Vanilla Nutrient Paste Expanded and SOS2.
- **Reduced Update Frequency:** Calculates food less often when there is abundant food, with a slider to adjust the update interval.

## Coding Patterns and Conventions
- **Mod Structure:** The mod follows a typical mod structure with separate files for different functionalities, including Harmony patches, extensions, and settings.
- **Harmony Patches:** The mod uses Harmony to patch existing RimWorld code, allowing for custom behavior without modifying the core game files.
- **Settings Management:** Settings are managed using a settings class (`FoodAlertSettings`) that exposes data and handles configuration through XML and C#.
- **Code Reusability:** Extensions and utility classes (`VatNutritionCounter`, `NutritionCounter`) are used to handle specific tasks, promoting code reuse and maintainability.

## XML Integration
The mod does not currently include any XML files. However, it is structured to support XML integration if needed. Developers interested in adding XML definitions can follow these guidelines:

1. **Create XML Files:** Create XML files in the `Defs` directory of the mod.
2. **Define XML Structure:** Define the structure of the XML files based on the RimWorld XML format.
3. **Load XML Data:** Use Harmony patches or other methods to load and parse the XML data into the mod.
4. **Integrate with Code:** Integrate the parsed XML data into the existing codebase, ensuring that it interacts correctly with the rest of the mod.

## Harmony Patching
Food Alert (Continued) extensively uses Harmony patches to modify the behavior of existing RimWorld code. Here are some guidelines for patching:

1. **Identify Target Methods:** Identify the methods in the RimWorld codebase that need to be patched.
2. **Create HarmonyPatches Class:** Create a class named `HarmonyPatches` in a separate file, e.g., `HarmonyPatches.cs`.
3. **Define Patch Methods:** Define methods within the `HarmonyPatches` class that contain the custom behavior you want to inject.
4. **Apply Patches:** Use Harmony's `Patch` method to apply the patch to the target methods.

Example of a Harmony patch method:
csharp
public class HarmonyPatches
{
    [HarmonyPatch(typeof(FoodCounter), "NearDatePostfix")]
    public static void FoodCounter_NearDatePostfix(ref float __result, FoodCounter __instance)
    {
        // Custom behavior to modify the result
        __result *= 2.0f; // Example: Double the result
    }
}


## Suggestions for Copilot
1. **Explore Harmony Patches:** Use Copilot to explore different ways to apply Harmony patches to RimWorld methods. Experiment with different patch locations and behaviors.
2. **Implement New Features:** Use Copilot to help implement new features, such as additional configuration options or language translations.
3. **Optimize Code:** Use Copilot to review and optimize existing code for better performance and maintainability.
4. **Debugging:** Use Copilot to suggest debugging techniques and code snippets to help resolve issues in the mod.

By following these guidelines, developers can effectively use GitHub Copilot to enhance the Food Alert (Continued) mod and contribute to the RimWorld modding community.

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

