# showcase-label

A Windows WPF application for printing QR code labels on a **4BARCODE QR-112D** (4B-2074A) thermal label printer using TSPL (TSC Printer Language).

## What it does

For each entry in a carving showcase, the app prints a label containing:
- A QR code (left side) that links to the entry's detail page
- The label ID (right side, e.g. `ABC-1`, `ABC-2`, …)

Both elements are printed side-by-side on the same label.

## Requirements

- Windows 10/11
- .NET 10 SDK (to build) or .NET 10 Runtime (to run)
- 4BARCODE QR-112D label printer connected via USB

## Supported label sizes

| Size | Dimensions |
|------|-----------|
| 2 5/8 x 1 inch | 66.7 × 25.4 mm (default) |
| 4 x 6 inch | 101.6 × 152.4 mm |

Select the label size from the **Label Size** dropdown before printing.

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
3. Select the **Label Size** matching the stock loaded in the printer (defaults to 2 5/8 x 1 inch).
4. Select **USB001** from the printer dropdown (auto-selected on startup).
5. Click **Print Labels**.

## How printer detection works

The app reads `HKLM\SYSTEM\CurrentControlSet\Control\Print\Monitors\USB Monitor\UsbPortList` to discover USB label printers and populates the dropdown with the USB ports found (e.g. `USB001`). The device is opened directly using the Win32 `CreateFile`/`WriteFile` API with `FILE_FLAG_WRITE_THROUGH`.

Only USB ports are shown — COM ports and Windows spooler printers are excluded because they are not label printers.

> **Note:** The QR-112D does not need to be installed as a Windows printer. It is accessed directly via its USB device interface.

## Label format

Labels are generated as raw TSPL commands at 203 DPI. Physical mm dimensions are used in `SIZE` and `GAP` so the printer's gap sensor re-homes between each label (preventing vertical drift across multiple prints).

**2 5/8 x 1 inch example:**
```
SIZE 66.7 mm,25.4 mm
GAP 3 mm,0
DIRECTION 0
CLS
QRCODE <x>,<y>,H,3,A,0,M2,S7,"<url>"
TEXT <x>,<y>,"3",0,1,1,"<labelId>"
PRINT 1,1
```

**4 x 6 inch example:**
```
SIZE 101.6 mm,152.4 mm
GAP 3 mm,0
DIRECTION 0
CLS
QRCODE <x>,<y>,H,8,A,0,M2,S7,"<url>"
TEXT <x>,<y>,"3",0,2,2,"<labelId>"
PRINT 1,1
```
