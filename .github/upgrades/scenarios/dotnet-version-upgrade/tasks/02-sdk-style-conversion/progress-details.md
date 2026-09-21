# Progress: 02-sdk-style-conversion

## Summary

`ContosoUniversity.csproj` converted from legacy WAP (packages.config) to SDK-style, staying on `net48`. Project builds cleanly with MSBuild.exe (0 errors, 1 pre-existing warning). `packages.config` is gone; all 45 dependencies are `PackageReference`s.

## Steps taken

1. Ran the dedicated `convert_project_to_sdk_style` MCP tool (per `converting-to-sdk-style` skill) against `ContosoUniversity.csproj` — one project, no manual XML rewrite for the initial conversion.
2. Reviewed the tool's output; it flagged one globbing exclusion: `Services\LoggingService.cs` exists on disk but was **not** in the original legacy project's `<Compile>` list, so the tool added `<Compile Remove="Services\LoggingService.cs" />` to preserve original build behavior. **Left as-is** (not removed) per the skill's guidance to get user confirmation before dropping such exclusions — flagging this to the user/orchestrator now. If the file should be compiled, remove that `<Compile Remove>` line.
3. Built with `msbuild.exe` (per `building-projects` skill: legacy-turned-SDK-style net4x project → full VS MSBuild, not `dotnet build`) and iteratively fixed build breaks caused directly by the conversion:
   - **NU1605 package downgrade (error)**: `Microsoft.Extensions.Primitives 3.1.32` requires `System.Runtime.CompilerServices.Unsafe >= 4.7.1`, but packages.config had pinned `4.5.3`. PackageReference makes this transitive minimum a hard conflict (packages.config was flat, so this never surfaced). Fixed by bumping the explicit `PackageReference` for `System.Runtime.CompilerServices.Unsafe` from `4.5.3` → `4.7.1` (the exact version the graph requires — no further version drift).
   - **Missing framework `<Reference>` items**: the conversion tool dropped most plain-framework-assembly references (`System.Web`, `System.Web.Routing`, `System.Data`, `System.Drawing`, `System.Xml`, `System.EnterpriseServices`, `System.Web.Services`, `System.Web.ApplicationServices`, `System.Web.DynamicData`, `System.Web.Entity`, `System.Web.Extensions`, `System.Web.Abstractions`, `System.Net.Http.WebRequest`, `System.Xml.Linq`). SDK-style net48 does not implicitly reference these. Re-added them all as plain `<Reference>` items (restoring the original legacy project's reference list).
   - **Missing PackageReference**: the conversion tool silently dropped `Microsoft.AspNet.Web.Optimization` (present in `packages.config` and in the original `<Reference HintPath>` list) — this is what caused `System.Web.Optimization`/`BundleCollection` compile errors. Re-added `<PackageReference Include="Microsoft.AspNet.Web.Optimization" Version="1.1.3" />`.
   - **Root cause of the stubborn `System.Web`/`HttpApplication`/`RouteCollection`/`HttpPostedFileBase` "type has been forwarded" errors** (took the bulk of the investigation): the legacy `packages\` folder (434 MB, left on disk from the pre-conversion packages.config restore, and still repopulated by PackageReference restore because `NuGet.Config`'s `repositoryPath` points at it) was **not excluded from SDK-style implicit item globbing**. Its files — including `packages\NETStandard.Library.2.0.3\build\netstandard2.0\ref\*.dll` (compile-time facade DLLs with `TypeForwardedTo` stubs, bundled with the old `NETStandard.Library` package) — were swept in as default `<None>` items, which MSBuild's `ResolveAssemblyReference` task also consumes via `CandidateAssemblyFiles`. These facades shadowed the real `System.Web`/`System.Data`/`System.Drawing` framework assemblies for plain-name `<Reference>` items with no `HintPath`, so the compiler saw only forwarder stubs with no real type definitions. **Fixed** by adding `<DefaultItemExcludes>$(DefaultItemExcludes);packages\**</DefaultItemExcludes>` — this excludes the folder from implicit globbing only; it remains fully usable as the physical NuGet restore/repository path (confirmed: `CopySQLClientNativeBinaries` custom target and PackageReference restore both still work correctly against it). Verified two intermediate workarounds (pinning `NETStandard.Library` with `ExcludeAssets`, adding an explicit `HintPath` on the `System.Web` reference, adding `{GAC}` to `AssemblySearchPaths`) did **not** fix it — all were reverted once the actual glob-exclusion root cause was confirmed, to keep the diff minimal.
   - **`OutputType`**: the conversion tool set `<OutputType>Exe</OutputType>` (Microsoft.NET.Sdk.Web's default), but this is an IIS-hosted class library with no `Main` method (`CS5001`). Reverted to `<OutputType>Library</OutputType>` (the original legacy value).
4. Final clean build (`msbuild ContosoUniversity.csproj -restore -t:Clean,Build -p:Configuration=Debug`): **Build succeeded, 0 errors, 1 warning** (see below).

## Remaining/deferred items (not in scope for this task)

- **NU1903 warning**: `Microsoft.Data.SqlClient 2.1.4` has a known high-severity vulnerability advisory. This is a pre-existing condition (present before conversion too) and is explicitly the concern of the package-upgrade work in tasks 03/04 (`assessment.md` already flags a suggested version 7.1.0) — not fixed here per the "structural change only" scope of this task.
- **`Services\LoggingService.cs` exclusion** (see step 2): flagged for user confirmation, left excluded to preserve original build behavior.
- **`CopySQLClientNativeBinaries` custom target** is now partially redundant with `Microsoft.Data.SqlClient.SNI.runtime`'s own `buildTransitive` `CopySNIFiles` target (both now run and copy the SNI native DLL to `bin\net48\`), but the custom target additionally places copies under `x64\`/`x86\` subfolders that the package's own target doesn't create. Left in place (harmless duplication, no build conflict) — cleanup optional, out of scope.
- Output path changed from legacy `bin\` to `bin\net48\` (SDK-style always appends the TFM folder segment, even when `OutputPath` is explicitly set) — expected SDK-style behavior, not a defect for this task's scope.
- `Microsoft.WebApplication.targets` import, `MvcBuildViews`/WAP-specific behavior, and content globbing were **not carried over explicitly** — SDK-style implicit globbing (see `DefaultItemExcludes` note above) now handles `Views\*.cshtml`, `Web.config`/`Web.*.config` transforms, and static content automatically; no explicit `<Content>` entries needed for those beyond what remains in the file (build output confirms `Web.config` and `Views\Web.config` are still copied to output).

## Files modified

- [ContosoUniversity.csproj](../../../../../../ContosoUniversity/ContosoUniversity.csproj) — converted to SDK-style (`Microsoft.NET.Sdk.Web`, `net48`, `PackageReference`-based).
- `packages.config` — removed by the conversion tool.

## Build validation

```
msbuild ContosoUniversity.csproj -restore -t:Clean,Build -p:Configuration=Debug -v:normal
Build succeeded.
    1 Warning(s)   (NU1903 — pre-existing, deferred to package-upgrade tasks)
    0 Error(s)
ContosoUniversity -> ...\ContosoUniversity\bin\net48\ContosoUniversity.dll
```
