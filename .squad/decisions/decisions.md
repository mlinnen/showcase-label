# Team Decisions

## Decision: Authenticode Code Signing (Issue #20)

**Date:** 2026-04-01  
**Author:** Mr. Pink  
**Issue:** #20  
**PR:** #21

### Summary

The build pipeline now supports optional Authenticode code signing to satisfy Windows Smart App Control requirements. Signing is conditional on GitHub secrets being present — if absent, the build succeeds with an unsigned executable.

### Changes

#### `.github/workflows/build.yml`
Added a `Sign Executable` step between the `Publish` and `Zip Artifacts` steps:
- Runs conditionally: `if: env.CODESIGN_PFX_BASE64 != ''`
- Decodes base64-encoded PFX from `CODESIGN_PFX_BASE64` secret into a temp file in `$RUNNER_TEMP`
- Locates `signtool.exe` from the Windows SDK (available on `windows-latest` runners)
- Signs `./publish/${{ matrix.runtime }}/ShowcaseLabel.exe` with:
  - SHA-256 file digest (`/fd sha256`)
  - SHA-256 timestamp digest (`/td sha256`)
  - DigiCert timestamp server (`/tr http://timestamp.digicert.com`)
- Cleans up the temp PFX file after signing
- Applies to all three runtime targets: win-x64, win-x86, win-arm64

#### `README-CODESIGNING.md`
Created comprehensive setup documentation covering:
- Required GitHub secrets (`CODESIGN_PFX_BASE64`, `CODESIGN_PFX_PASSWORD`)
- PowerShell one-liner to export a PFX certificate to base64
- Where to obtain code signing certificates:
  - Commercial CAs (DigiCert, Sectigo, GlobalSign) for production use
  - Self-signed certificates for testing (with note that they don't satisfy Smart App Control)
- Clarification that signing is optional — builds succeed without secrets

### Rationale

- Windows Smart App Control (formerly SmartScreen) blocks unsigned executables on Windows 11 machines with the feature enabled, creating a poor user experience.
- Authenticode signing verifies the publisher's identity and ensures the executable hasn't been tampered with since signing.
- Making signing **optional** keeps the build process flexible:
  - Public forks and CI runs succeed without needing access to the certificate
  - The certificate and password remain secrets in the repository owner's GitHub settings
  - Unsigned builds are still valid for testing and development

### Technical Details

- **Tool:** `signtool.exe` from the Windows SDK (pre-installed on `windows-latest` GitHub runners)
- **Discovery:** Recursively search `C:\Program Files (x86)\Windows Kits\10\bin` for the most recent x64 signtool
- **Security:** PFX is written to `$RUNNER_TEMP` (ephemeral runner storage), used once, then immediately deleted
- **Timestamp server:** DigiCert's public RFC 3161 timestamp server ensures signatures remain valid after the certificate expires

### Impact

- **Build time:** Adds ~5–10 seconds per runtime target when signing is enabled
- **Artifacts:** Signed executables are included in the release zips
- **User experience:** Executables signed with a trusted certificate run without security warnings on Windows 11 with Smart App Control enabled
- **Setup burden:** Repository admins must configure two secrets; see `README-CODESIGNING.md` for instructions

### Future Considerations

- If the certificate expires, update the `CODESIGN_PFX_BASE64` and `CODESIGN_PFX_PASSWORD` secrets with a renewed certificate
- Timestamping ensures old builds remain validly signed even after the certificate expires
- Consider using Azure Key Vault or similar HSM-backed signing if stricter certificate security is required
