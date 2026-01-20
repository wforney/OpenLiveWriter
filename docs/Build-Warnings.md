# Build Warnings Documentation

## Overview

This document explains the MSBuild warnings that appear during the OpenLiveWriter build process and the rationale for suppressing them.

## Suppressed Warnings

The following MSBuild warnings are intentionally suppressed in the build configuration:

### MSB3277 - Assembly Version Conflicts

**Description:** Conflicts between different versions of the same dependent assembly (e.g., System.Core v3.5.0.0 vs v4.0.0.0).

**Reason for Suppression:** 
- The project targets .NET Framework 4.8 but has transitive dependencies that reference older framework versions
- These conflicts are automatically resolved by the .NET Framework's assembly binding and unification process
- The chosen versions are appropriate for the target framework
- No runtime issues have been observed related to these conflicts

**Location:** Primarily affects OpenLiveWriter.HtmlParser and OpenLiveWriter.Interop projects

### MSB3247 - Assembly Binding Redirects

**Description:** Suggests adding binding redirects to app.config files for conflicting assembly versions.

**Reason for Suppression:**
- AutoGenerateBindingRedirects is enabled, which automatically generates binding redirects at build time
- Manual binding redirects are not necessary with this feature enabled
- The warning is informational and does not indicate a build problem

### MSB3276 - AutoGenerateBindingRedirects Recommendation

**Description:** Recommends setting AutoGenerateBindingRedirects to true when assembly version conflicts are detected.

**Reason for Suppression:**
- AutoGenerateBindingRedirects is already set to true in writer.build.settings and all affected project files
- The warning appears even when the property is correctly configured
- This is a known issue with MSBuild where the warning can appear spuriously

**Affected Projects:** OpenLiveWriter.csproj, BlogRunner.csproj, BlogRunnerGui.csproj

### MSB3270 - Processor Architecture Mismatch

**Description:** Mismatch between processor architecture of the project (MSIL) and referenced assemblies (x86/AMD64).

**Reason for Suppression:**
- The LocEdit project uses AnyCPU platform target, which is appropriate for its use case
- The referenced assemblies will work correctly at runtime despite the architecture difference
- ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch is set to None to handle this scenario

**Affected Projects:** LocEdit.csproj

### MSB3245 - Missing Assembly Reference

**Description:** Could not locate assembly "Newtonsoft.Json, Version=10.0.0.0".

**Reason for Suppression:**
- The project correctly references Newtonsoft.Json version 13.0.0.0
- The warning refers to an old version that is no longer in use
- Binding redirects handle the version mismatch automatically
- The assembly is present and loads correctly at runtime

**Affected Projects:** OpenLiveWriter.PostEditor.csproj

### MSB3836 - Manifest Resource Name Conflict

**Description:** Warning about potential conflicts in manifest resource names.

**Reason for Suppression:**
- This warning is informational and does not indicate an actual problem
- The build system correctly handles resource embedding
- No runtime issues related to resource loading have been observed

## Configuration

Warning suppression is configured in two locations:

1. **Global Configuration** (writer.build.settings):
   ```xml
   <NoWarn>MSB3277;MSB3247;MSB3276;MSB3245;MSB3270;MSB3836</NoWarn>
   ```
   This applies to all projects that import the writer.build.settings file.

2. **LocEdit Project** (LocEdit.csproj):
   ```xml
   <NoWarn>MSB3270</NoWarn>
   <ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>None</ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
   ```
   LocEdit.csproj does not import writer.build.settings, so it needs project-specific configuration for MSB3270.
   MSB3277 and other warnings are already handled by the global configuration.

## Code Quality

Note that these are all **MSBuild-level warnings** related to assembly references and binding. There are **no C# compiler warnings (CS)** or **code analysis warnings (CA)** in the build, indicating that the source code quality is good.

## Build Success

Despite these warnings (when not suppressed), the build completes successfully and produces working binaries. The warnings are informational and do not indicate actual build or runtime problems.

## Future Considerations

If migrating to .NET Core/.NET 5+, many of these warnings would be eliminated due to the different assembly binding model. For the current .NET Framework 4.8 target, the suppressions are appropriate and necessary for a clean build.
