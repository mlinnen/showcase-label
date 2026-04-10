# Code Signing Setup

The build pipeline supports Authenticode code signing to satisfy Windows Smart App Control requirements. This allows the executable to run on Windows 11 machines without security warnings.

## Required GitHub Secrets

Configure these secrets in your repository settings (`Settings` → `Secrets and variables` → `Actions` → `New repository secret`):

### `CODESIGN_PFX_BASE64`
The code signing certificate in PFX format, encoded as base64.

To generate this from a PFX file:
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\to\your\certificate.pfx"))
```

Copy the output and paste it as the secret value.

### `CODESIGN_PFX_PASSWORD`
The password protecting the PFX file.

## Optional Signing

Code signing is **optional**. The build will succeed whether or not the secrets are configured:
- **Secrets present:** The executable is signed with Authenticode during the build.
- **Secrets absent:** The executable is built unsigned. It will still run, but Windows Smart App Control may block it.

## Obtaining a Code Signing Certificate

### For Production Use
Purchase a code signing certificate from a commercial Certificate Authority (CA) trusted by Microsoft:
- [DigiCert](https://www.digicert.com/signing/code-signing-certificates)
- [Sectigo](https://sectigo.com/ssl-certificates-tls/code-signing)
- [GlobalSign](https://www.globalsign.com/en/code-signing-certificate)

These certificates typically require business verification and range from $200–$500/year.

### For Internal/Testing Use
You can generate a self-signed certificate for testing purposes:
```powershell
$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=YourCompany" -CertStoreLocation Cert:\CurrentUser\My
$password = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "codesign.pfx" -Password $password
```

⚠️ **Note:** Self-signed certificates do **not** satisfy Windows Smart App Control requirements. They're useful for verifying the build process works, but won't prevent security warnings on end-user machines.

## Signing Details

The build pipeline signs the executable using:
- **Digest algorithm:** SHA-256
- **Timestamp server:** http://timestamp.digicert.com
- **Tool:** `signtool.exe` (Windows SDK, available on `windows-latest` GitHub runners)

The signing step runs after `dotnet publish` and before artifact packaging, ensuring the signed executable is included in the final release artifacts.
