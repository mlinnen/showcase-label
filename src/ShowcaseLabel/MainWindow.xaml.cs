using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Windows;
using ESCPOS_NET;
using ESCPOS_NET.Utilities;
using ESCPOS_NET.Emitters;
using Microsoft.Extensions.Configuration;

namespace ShowcaseLabel
{
    public partial class MainWindow : Window
    {
        private readonly string _baseUrl;

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
                {
                    PrinterComboBox.Items.Add(printer);
                }

                if (PrinterComboBox.Items.Count > 0)
                {
                    foreach (var item in PrinterComboBox.Items)
                    {
                        if (item?.ToString()?.Contains("QR-112", StringComparison.OrdinalIgnoreCase) == true || 
                            item?.ToString()?.Contains("Label", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            PrinterComboBox.SelectedItem = item;
                            break;
                        }
                    }

                    if (PrinterComboBox.SelectedIndex == -1)
                    {
                        PrinterComboBox.SelectedIndex = 0;
                    }
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
                // Note: FilePrinter on Windows often needs the shared printer path like \\localhost\PrinterName
                var printer = new FilePrinter(printerName, false);
                var e = new EPSON();

                for (int i = 1; i <= totalEntries; i++)
                {
                    string labelId = $"{carverId}-{i}";
                    string qrData = $"{_baseUrl}{labelId}";

                    printer.Write(
                        ByteSplicer.Combine(
                            e.Initialize(),
                            e.CenterAlign(),
                            e.PrintQRCode(qrData, TwoDimensionCodeType.QRCODE_MODEL2, Size2DCode.LARGE),
                            e.PrintLine(labelId),
                            e.FeedLines(3),
                            e.FullCut()
                        )
                    );
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
    }
}
