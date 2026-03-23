using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace ShowcaseLabel
{
    public partial class MainWindow : Window
    {
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _usbDevicePaths = new();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(
            IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint GENERIC_WRITE           = 0x40000000;
        private const uint FILE_SHARE_READ         = 0x00000001;
        private const uint FILE_SHARE_WRITE        = 0x00000002;
        private const uint OPEN_EXISTING           = 3;
        private const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        private const int DPI = 203;

        // Supported label sizes.
        // WidthMm/HeightMm/GapMm are physical dimensions used in TSPL SIZE/GAP commands
        // so the gap sensor can re-home between labels and prevent vertical drift.
        // Width/Height in dots are used for element positioning.
        private record LabelSize(
            string DisplayName,
            double WidthMm, double HeightMm, double GapMm,
            int Width, int Height, int QrCellWidth);

        private static readonly LabelSize[] LabelSizes =
        [
            new("4 x 6 inch",     101.6,  152.4, 3.0,  4 * DPI,          6 * DPI,     8),
            new("2 5/8 x 1 inch",  66.7,   25.4, 3.0,  (int)(2.625*DPI), 1 * DPI,     3),
        ];

        public MainWindow()
        {
            InitializeComponent();
            _baseUrl = LoadConfiguration();
            LoadPrinters();
            LoadLabelSizes();
        }

        private string LoadConfiguration()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                return config["BaseUrl"] ?? "";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading config: {ex.Message}";
                return "";
            }
        }

        private void LoadLabelSizes()
        {
            foreach (var size in LabelSizes)
                LabelSizeComboBox.Items.Add(size.DisplayName);
            LabelSizeComboBox.SelectedIndex = 1; // default to 2 5/8 x 1 inch
        }

        private LabelSize SelectedLabelSize =>
            LabelSizes[LabelSizeComboBox.SelectedIndex >= 0 ? LabelSizeComboBox.SelectedIndex : 0];

        private void LoadPrinters()
        {
            try
            {
                using var portsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\Print\Monitors\USB Monitor\UsbPortList");
                if (portsKey != null)
                {
                    foreach (string portName in portsKey.GetValueNames().Where(v => v.StartsWith("USB")))
                    {
                        string devicePath = portsKey.GetValue(portName)?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(devicePath))
                        {
                            _usbDevicePaths[portName] = devicePath;
                            PrinterComboBox.Items.Add(portName);
                        }
                    }
                }

                if (PrinterComboBox.Items.Count > 0)
                    PrinterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading printers: {ex.Message}";
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            string carverId = CarverIdTextBox.Text.Trim();
            if (string.IsNullOrEmpty(carverId))
            {
                MessageBox.Show("Please enter a Carver ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(TotalEntriesTextBox.Text, out int totalEntries) || totalEntries <= 0)
            {
                MessageBox.Show("Please enter a valid number of entries.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PrinterComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a printer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string printerName = PrinterComboBox.SelectedItem?.ToString() ?? "";
            LabelSize labelSize = SelectedLabelSize;
            StatusTextBlock.Text = $"Printing {totalEntries} labels to {printerName}...";
            PrintLabels(printerName, carverId, totalEntries, labelSize);
        }

        private void PrintLabels(string printerName, string carverId, int totalEntries, LabelSize labelSize)
        {
            try
            {
                if (printerName.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_usbDevicePaths.TryGetValue(printerName, out string? devicePath))
                        throw new InvalidOperationException($"No device path found for {printerName}.");
                    PrintToUsb(devicePath, carverId, totalEntries, labelSize);
                }
                StatusTextBlock.Text = "Printing complete.";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Printing failed: {ex.Message}";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Error during printing: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintToUsb(string devicePath, string carverId, int totalEntries, LabelSize labelSize)
        {
            IntPtr handle = CreateFile(devicePath, GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero,
                OPEN_EXISTING, FILE_FLAG_WRITE_THROUGH, IntPtr.Zero);

            if (handle == INVALID_HANDLE_VALUE)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),
                    "Failed to open USB printer device.");
            try
            {
                for (int i = 1; i <= totalEntries; i++)
                {
                    byte[] data = BuildTsplLabel($"{carverId}-{i}", labelSize);
                    if (!WriteFile(handle, data, (uint)data.Length, out _, IntPtr.Zero))
                        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),
                            "Failed to write to USB printer device.");
                }
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        // Builds TSPL commands for a label with the QR code and label ID text side by side.
        // The QR code is on the left; the label ID text is vertically centered to its right.
        private byte[] BuildTsplLabel(string labelId, LabelSize size)
        {
            string qrData = $"{_baseUrl}{labelId}";

            // Estimated QR code size in dots (modules × cellWidth; typical QR v3 = 29 modules)
            int qrSize   = 29 * size.QrCellWidth;
            int margin   = Math.Max(5, (size.Height - qrSize) / 2);
            int qrX      = margin;
            int qrY      = margin;

            // Place text to the right of the QR code, vertically centered
            // TSPL font "3": 16×24 dots/char at 1×1; scale up as space allows
            int textAreaWidth = size.Width - qrSize - margin * 3;
            int xMul = textAreaWidth > 200 ? 2 : 1;
            int yMul = size.Height > 100   ? 2 : 1;
            int charW = 16 * xMul;
            int charH = 24 * yMul;
            int textX = qrX + qrSize + margin;
            int textY = Math.Max(0, (size.Height - charH) / 2);

            var sb = new StringBuilder();
            // Use physical mm dimensions so the gap sensor re-homes between labels,
            // preventing vertical drift across multiple prints.
            sb.AppendLine($"SIZE {size.WidthMm} mm,{size.HeightMm} mm");
            sb.AppendLine($"GAP {size.GapMm} mm,0");
            sb.AppendLine("DIRECTION 0");
            sb.AppendLine("CLS");
            sb.AppendLine($"QRCODE {qrX},{qrY},H,{size.QrCellWidth},A,0,M2,S7,\"{qrData}\"");
            sb.AppendLine($"TEXT {textX},{textY},\"3\",0,{xMul},{yMul},\"{labelId}\"");
            sb.AppendLine("PRINT 1,1");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}