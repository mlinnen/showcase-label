# Project Context

- **Owner:** Mike Linnen
- **Project:** showcase-label — WPF desktop app (.NET 10) that prints QR code labels on thermal printers (4BARCODE QR-112D) for Charlotte Wood Carvers carving showcase entries
- **Stack:** C# / .NET 10 / WPF / Win32 P/Invoke / TSPL printer language / GitHub Actions CI/CD / NuGet (ESCPOS_NET, Microsoft.Extensions.Configuration)
- **Created:** 2026-04-01

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-01 — Orchestration phase complete

- Orchestration logs created in .squad/orchestration-log/ for each agent
- All decisions merged to .squad/decisions.md and inbox cleared
- Session log created documenting parallel work completion
- Test project successfully integrated with 17 passing unit tests

### 2026-04-01 — Test project setup for issue #15

- The main project's `AssemblyInfo.cs` lives at `src/ShowcaseLabel/AssemblyInfo.cs` (not under `Properties/`). The `InternalsVisibleTo` attribute was appended there.
- The test project targets `net10.0-windows` to match the main project (WPF dependency flows through the `ProjectReference`).
- `GetDivisionPrefix` was already implemented by Mr. Blonde when the test project was created — build succeeded immediately, confirming TDD readiness.
- xUnit 2.9.3 + xunit.runner.visualstudio 3.0.2 + Microsoft.NET.Test.Sdk 17.12.0 is the working combo for net10.0-windows.
- The branch `squad/15-add-division-to-label` already existed remotely when this work landed.
