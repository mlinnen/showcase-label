using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO.Ports;
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

        private const int DPI         = 203;
        private const int LabelWidth  = 4 * DPI;
        private const int LabelHeight = 6 * DPI;

        public MainWindow()
        {
            InitializeComponent();
            _baseUrl = LoadConfiguration();
            LoadPrinters();
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

        private void LoadPrinters()
        {
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                    PrinterComboBox.Items.Add(printer);

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

                foreach (string port in SerialPort.GetPortNames().OrderBy(p => p))
                    PrinterComboBox.Items.Add(port);

                if (PrinterComboBox.Items.Count > 0)
                {
                    foreach (var item in PrinterComboBox.Items)
                    {
                        string name = item?.ToString() ?? "";
                        if (name.Contains("QR-112", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("Label",  StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith("USB",  StringComparison.OrdinalIgnoreCase))
                        {
                            PrinterComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    if (PrinterComboBox.SelectedIndex == -1)
                        PrinterComboBox.SelectedIndex = 0;
                }
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
            StatusTextBlock.Text = $"Printing {totalEntries} labels to {printerName}...";
            PrintLabels(printerName, carverId, totalEntries);
        }

        private void PrintLabels(string printerName, string carverId, int totalEntries)
        {
            try
            {
                if (printerName.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_usbDevicePaths.TryGetValue(printerName, out string? devicePath))
                        throw new InvalidOperationException($"No device path found for {printerName}.");
                    PrintToUsb(devicePath, carverId, totalEntries);
                }
                else if (printerName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                {
                    using var port = new SerialPort(printerName, 9600);
                    port.Open();
                    for (int i = 1; i <= totalEntries; i++)
                    {
                        byte[] data = BuildTsplLabel($"{carverId}-{i}");
                        port.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    using var stream = System.IO.File.OpenWrite($@"\\localhost\{printerName}");
                    for (int i = 1; i <= totalEntries; i++)
                    {
                        byte[] data = BuildTsplLabel($"{carverId}-{i}");
                        stream.Write(data, 0, data.Length);
                    }
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

        private void PrintToUsb(string devicePath, string carverId, int totalEntries)
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
                    byte[] data = BuildTsplLabel($"{carverId}-{i}");
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

        // Builds TSPL commands for a 4x6 label (203 DPI) with a centered QR code and label ID text.
        private byte[] BuildTsplLabel(string labelId)
        {
            string qrData = $"{_baseUrl}{labelId}";
            int qrX = LabelWidth / 2 - 100;
            int qrY = 200;
            int textX = LabelWidth / 2;
            int textY = qrY + 450;

            var sb = new StringBuilder();
            sb.AppendLine($"SIZE {LabelWidth} dot,{LabelHeight} dot");
            sb.AppendLine("GAP 0,0");
            sb.AppendLine("DIRECTION 0");
            sb.AppendLine("CLS");
            sb.AppendLine($"QRCODE {qrX},{qrY},H,8,A,0,M2,S7,\"{qrData}\"");
            sb.AppendLine($"TEXT {textX},{textY},\"4\",0,2,2,\"{labelId}\"");
            sb.AppendLine("PRINT 1,1");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}