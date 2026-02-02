# Repository Guidelines

## Project Structure & Module Organization
- `MDCE.sln` is the Visual Studio solution entry point.
- `MDCE/` contains the application source (`Program.cs`, `Parameters.cs`, `Hash.cs`).
- `MDCE/Properties/` holds assembly metadata and the app manifest.
- `MDCE/lib/` includes Microsoft Dynamics connector DLLs used at runtime.
- `MDCE/App.config` stores runtime settings such as credentials and customer label.

## Build, Test, and Development Commands
- `msbuild MDCE.sln /p:Configuration=Debug` builds a debug executable to `MDCE/bin/Debug/`.
- `msbuild MDCE.sln /p:Configuration=Release` builds optimized binaries to `MDCE/bin/Release/`.
- Run the tool with the built EXE, for example `MDCE/bin/Debug/MicrosoftDynamicsConnectorExporter.exe`.
- Optional arguments: `integrationId` and `siteIntegrationId` GUIDs to skip interactive selection.

## Coding Style & Naming Conventions
- Follow standard C# conventions: 4-space indentation, `PascalCase` for types and public members, `camelCase` for locals and parameters.
- Keep method bodies short and avoid large blocks of inline string building when possible.
- No formatter or linter is configured; use Visual Studio defaults and keep changes minimal and consistent with existing files.

## Testing Guidelines
- There are no automated tests in this repository.
- If you add tests, document the framework and provide a command to run them in this file.

## Commit & Pull Request Guidelines
- Commit history uses short, imperative subjects (for example: `Update README.md`). Keep messages concise and scoped to one change.
- PRs should include a clear summary, manual test notes (if applicable), and any configuration changes (for example updates to `MDCE/App.config`).

## Security & Configuration Notes
- `MDCE/App.config` includes credentials (`Username`, `Password`, `Domain`). Do not commit real secrets.
- Prefer environment-specific overrides or local config files when running locally.
