# showcase-label

A Windows WPF application for printing QR code labels on a **4BARCODE QR-112D** (4B-2074A) thermal label printer using TSPL (TSC Printer Language).

## What it does

For each entry in a carving showcase, the app prints a 4×6 inch label containing:
- A QR code that links to the entry's detail page
- The label ID (e.g. `ABC-1`, `ABC-2`, …)

## Requirements

- Windows 10/11
- .NET 10 SDK (to build) or .NET 10 Runtime (to run)
- 4BARCODE QR-112D label printer connected via USB
- 4×6 inch labels loaded in the printer

## Configuration

Edit `src/ShowcaseLabel/appsettings.json` to set the base URL for QR codes:

```json
{
  "BaseUrl": "https://yoursite.com/carving/"
}
```

The QR code on each label will encode `{BaseUrl}{CarverId}-{N}` (e.g. `https://yoursite.com/carving/ABC-1`).

## Building

```bash
dotnet build src/ShowcaseLabel/ShowcaseLabel.csproj
```

## Running

```bash
dotnet run --project src/ShowcaseLabel/ShowcaseLabel.csproj
```

Or open `showcase-label.sln` in Visual Studio and press **F5**.

## Usage

1. Enter the **Carver ID** (e.g. `ABC`).
2. Enter the **Total Entries** (number of labels to print).
3. Select **USB001** from the printer dropdown (auto-selected on startup).
4. Click **Print Labels**.

## How printer detection works

The app detects printers in this order:
1. **USB ports** — reads `HKLM\SYSTEM\CurrentControlSet\Control\Print\Monitors\USB Monitor\UsbPortList` and opens the device directly using the Win32 `CreateFile`/`WriteFile` API with `FILE_FLAG_WRITE_THROUGH`.
2. **COM ports** — lists available serial ports for serial-connected printers.
3. **Windows spooler printers** — lists printers installed in the Windows print spooler.

> **Note:** The QR-112D does not need to be installed as a Windows printer. It is accessed directly via its USB device interface.

## Label format

Labels are generated as raw TSPL commands at 203 DPI on a 4×6 inch (812×1218 dot) canvas:

```
SIZE 812 dot,1218 dot
GAP 0,0
DIRECTION 0
CLS
QRCODE <x>,<y>,H,8,A,0,M2,S7,"<url>"
TEXT <x>,<y>,"4",0,2,2,"<labelId>"
PRINT 1,1
```