# showcase-label

A Windows WPF application for printing QR code labels on a **4BARCODE QR-112D** (4B-2074A) thermal label printer using TSPL (TSC Printer Language).

## What it does

For each entry in a carving showcase, the app prints a label containing:
- A QR code (left side) that links to the entry's detail page
- The label ID (right side, e.g. `C123-1`, `C123-2`, …, or with division prefix `N-C123-1` for Novice division)

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
  "BaseUrl": "https://yoursite.com/showcase/"
}
```

The QR code on each label will encode the URL as a query string with the event, carver ID, and entry number (e.g. `https://yoursite.com/showcase/?event=2027&carver_id=123&entry=1`).

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

1. Select the **Event** from the dropdown (e.g. `2027` or `2027T`).
2. Choose a carver mode:
   - **Single carver** (the default): enter one numeric **Carver ID** (e.g. `123`).
   - **Carver range**: enter numeric **Start Carver ID** and **End Carver ID** values. Bounds are inclusive and Start must be less than or equal to End.
3. Select a **Division** (None, Novice, Intermediate, or Open). The division prefix (if any) will appear on the label text.
4. Enter the **From Entry** and **To Entry** numbers to define the inclusive entry range.
   - Both values must be positive integers greater than 0.
   - From Entry must be less than or equal to To Entry.
   - For a single label, set both From Entry and To Entry to the same number.
5. Set **Maximum labels** for the batch (defaults to `15`). A batch exceeding this limit is rejected before the printer is opened.
6. Select the **Label Size** matching the stock loaded in the printer (defaults to 2 5/8 x 1 inch).
7. Select **USB001** from the printer dropdown (auto-selected on startup).
8. Click **Print Labels** to print all labels in the specified range.

In carver-range mode, the batch is the Cartesian product of the inclusive carver and entry ranges. Labels are printed in ascending carver-ID order, with entries ascending within each carver. For example, carvers `10`–`12` with entries `1`–`5` print `C10-1` through `C10-5`, then `C11-1` through `C11-5`, and finally `C12-1` through `C12-5` (15 labels).

## How printer detection works

The app reads `HKLM\SYSTEM\CurrentControlSet\Control\Print\Monitors\USB Monitor\UsbPortList` to discover USB label printers and populates the dropdown with the USB ports found (e.g. `USB001`). The device is opened directly using the Win32 `CreateFile`/`WriteFile` API with `FILE_FLAG_WRITE_THROUGH`.

Only USB ports are shown — COM ports and Windows spooler printers are excluded because they are not label printers.

> **Note:** The QR-112D does not need to be installed as a Windows printer. It is accessed directly via its USB device interface.

## Label format

Labels are generated as raw TSPL commands at 203 DPI. Physical mm dimensions are used in `SIZE` and `GAP` so the printer's gap sensor re-homes between each label (preventing vertical drift across multiple prints).

The label text includes an optional division prefix (N-, I-, O-, or empty) based on the selected division.

**2 5/8 x 1 inch example (Novice division):**
```
SIZE 66.7 mm,25.4 mm
GAP 3 mm,0
DIRECTION 0
CLS
QRCODE <x>,<y>,M,3,A,0,M2,S7,"<url>"
TEXT <x>,<y>,"3",0,1,1,"N-C<carver_id>-<entry>"
PRINT 1,1
```

**4 x 6 inch example (no division prefix):**
```
SIZE 101.6 mm,152.4 mm
GAP 3 mm,0
DIRECTION 0
CLS
QRCODE <x>,<y>,M,8,A,0,M2,S7,"<url>"
TEXT <x>,<y>,"3",0,2,2,"C<carver_id>-<entry>"
PRINT 1,1
```
