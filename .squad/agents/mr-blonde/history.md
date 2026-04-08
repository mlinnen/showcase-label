# Project Context

- **Owner:** Mike Linnen
- **Project:** showcase-label — WPF desktop app (.NET 10) that prints QR code labels on thermal printers (4BARCODE QR-112D) for Charlotte Wood Carvers carving showcase entries
- **Stack:** C# / .NET 10 / WPF / Win32 P/Invoke / TSPL printer language / GitHub Actions CI/CD / NuGet (ESCPOS_NET, Microsoft.Extensions.Configuration)
- **Created:** 2026-04-01

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-01 — Orchestration phase complete

- Three agents completed parallel work on issue #15 and solution file relocation
- mr-blonde-issue-15: Division dropdown UI + logic on squad/15-add-division-to-label (PR #18)
- mr-orange-issue-15-tests: ShowcaseLabel.Tests xUnit project + 17 tests on squad/15-add-division-to-label
- mr-blonde-move-sln: Solution file relocated to src/ on chore/move-sln-to-src (PR #19)
- All decisions documented in .squad/decisions.md
- Orchestration logs created in .squad/orchestration-log/
- Session log created in .squad/log/

### 2026-04-01 — chore: move solution file into src/

- `showcase-label.sln` now lives at `src/showcase-label.sln`; project paths are relative to that folder (e.g. `ShowcaseLabel\ShowcaseLabel.csproj`).
- `ShowcaseLabel.Tests` did NOT exist on `dev` (only the folder, not the `.csproj`); it was omitted from the relocated sln.
- The redundant "src" virtual solution folder and `NestedProjects` section were removed since the sln already lives inside `src/`.
- `build.yml` required no changes — it references `.csproj` files directly, not the `.sln`.
- VS .sln files require a UTF-8 BOM; use `[System.Text.UTF8Encoding]::new($true)` when writing with PowerShell. The first content line after the BOM must be blank (matches VS convention of `\r\n\r\nMicrosoft Visual Studio...`). Double-BOM causes MSB5010 "No file format header found".

### 2026-04-01 — Issue #15: Division dropdown and label prefix

- `GetDivisionPrefix` is extracted as `internal static` so Mr. Orange can unit-test it without WPF plumbing.
- Division prefix flows as a plain `string` through `PrintLabels` → `PrintToUsb` → `BuildTsplLabel`; no new types needed.
- QR code URL deliberately excludes the prefix — it tracks by raw carver/entry ID only.
- Window height bumped from 380 → 420 to accommodate the extra row without clipping.
- Pre-existing NuGet vulnerability warnings on `SixLabors.ImageSharp` 2.1.3 are not related to this work.

### 2026-04-01 — Issue #17: QR code URL changed to query parameters

- `appsettings.json` `BaseUrl` changed from `https://charlottewoodcarvers.com/showcase/2026/` to `https://charlottewoodcarvers.com/showcase/`; year is no longer baked into the URL.
- New `"Event": "2026"` key added to `appsettings.json`; value can be updated each year without touching code.
- `LoadConfiguration()` now returns a `(string baseUrl, string evt)` tuple so both values are loaded and stored as `_baseUrl` and `_event` fields.
- `BuildTsplLabel` signature changed from `string labelId` to `string carver_id, int entryNumber`; QR URL is now `{_baseUrl}?event={event}&carver_id={carver_id}&entry={entryNumber}` (values URL-escaped via `Uri.EscapeDataString`).
- The TSPL `TEXT` command (human-readable label) is unchanged: `{divisionPrefix}C{carver_id}-{entryNumber}`.
- `PrintToUsb` loop updated to call `BuildTsplLabel(carver_id, i, labelSize, divisionPrefix)` with separate arguments.
