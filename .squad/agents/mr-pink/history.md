# Project Context

- **Owner:** Mike Linnen
- **Project:** showcase-label — WPF desktop app (.NET 10) that prints QR code labels on thermal printers (4BARCODE QR-112D) for Charlotte Wood Carvers carving showcase entries
- **Stack:** C# / .NET 10 / WPF / Win32 P/Invoke / TSPL printer language / GitHub Actions CI/CD / NuGet (ESCPOS_NET, Microsoft.Extensions.Configuration)
- **Created:** 2026-04-01

## Learnings

- **GitHub renamed master → main**: The repository's default branch is now called `main`, but the `master` branch still exists locally and on origin. When pushing to `master`, GitHub accepts it but notes the rename. No action needed; the old `master` branch is kept for backward compatibility.

- **Authenticode code signing implemented (Issue #20)**: The build pipeline now conditionally signs the executable using `signtool.exe` after `dotnet publish` and before artifact packaging. Requires two GitHub secrets: `CODESIGN_PFX_BASE64` (base64-encoded PFX certificate) and `CODESIGN_PFX_PASSWORD`. Signing is optional — if secrets are absent, the build succeeds with an unsigned executable. Uses SHA-256 digest algorithm and DigiCert timestamp server. See `README-CODESIGNING.md` for setup instructions.

<!-- Append new learnings below. Each entry is something lasting about the project. -->
