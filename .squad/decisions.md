# Squad Decisions

## Active Decisions

### Division Prefix on Label Text, Not QR URL

**Date:** 2026-04-01  
**Author:** Mr. Blonde  
**Issue:** #15

The division prefix (N-, I-, O-, or empty) is applied **only to the human-readable label text** printed via the TSPL `TEXT` command. The QR code URL remains unchanged and continues to encode the raw `{carverId}-{entry}` identifier.

**Rationale:**
- QR codes are scanned by the judging system to look up entries by ID. Embedding a division prefix in the URL would break existing lookups and require backend changes outside this scope.
- The division is a display/sorting concern for the carver and judges reading the physical label, not a data identity concern.

**Implementation:**
- `GetDivisionPrefix(string division)` → `internal static` switch expression returning `"N-"`, `"I-"`, `"O-"`, or `""`.
- Prefix is threaded as a `string` parameter through `PrintLabels` → `PrintToUsb` → `BuildTsplLabel`.
- TEXT command format: `"{divisionPrefix}C{labelId}"` (e.g. `N-C10-1`).

---

### Solution File Moved Into src/

**Date:** 2026-04-01  
**Author:** Mr. Blonde  
**Branch:** chore/move-sln-to-src

Relocated `showcase-label.sln` from the repository root to `src/showcase-label.sln`.

**Rationale:**
- All source code and projects live under `src/`; keeping the solution at the root was inconsistent.
- With the sln inside `src/`, project paths become shorter (`ShowcaseLabel\ShowcaseLabel.csproj` instead of `src\ShowcaseLabel\ShowcaseLabel.csproj`).
- The redundant "src" virtual solution folder and `NestedProjects` section are no longer needed and were removed.

**Impact:**
- `build.yml` is **unaffected** — it references `.csproj` files directly.
- `ShowcaseLabel.Tests` was **not** included in the relocated sln because the `.csproj` file does not yet exist on `dev`. It should be re-added when the Tests project is merged to dev.
- Anyone who has the solution open in Visual Studio will need to re-open it from its new location.

---

### Test Project Setup — ShowcaseLabel.Tests

**Author:** Mr. Orange  
**Date:** 2026-04-01  
**Issue:** #15 — Add Division to Label

Establish `ShowcaseLabel.Tests` as the single xUnit test project for the solution, living at `src/ShowcaseLabel.Tests/`.

**Rationale:**
- Zero tests existed prior to this PR. A standalone test project (not in-lining tests into the main assembly) keeps production and test code cleanly separated.
- `InternalsVisibleTo` on the main assembly allows testing `internal` helpers (like `GetDivisionPrefix`) without making them `public`.
- Targeting `net10.0-windows` mirrors the main project, ensuring no TFM mismatch when the `ProjectReference` pulls in WPF types.

**Conventions Established:**

| Concern | Choice |
|---|---|
| Test framework | xUnit 2.9.3 |
| Test runner | xunit.runner.visualstudio 3.0.2 |
| SDK | Microsoft.NET.Test.Sdk 17.12.0 |
| Test project location | `src/ShowcaseLabel.Tests/` |
| AssemblyInfo location | `src/ShowcaseLabel/AssemblyInfo.cs` (not under Properties/) |
| Internal visibility | `[assembly: InternalsVisibleTo("ShowcaseLabel.Tests")]` in AssemblyInfo.cs |

**Impact:**
- All future tests go in `ShowcaseLabel.Tests`.
- Any new `internal` helpers that need testing should follow the same `InternalsVisibleTo` pattern already in place.
- `dotnet test` from the solution root will pick up all tests automatically.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
