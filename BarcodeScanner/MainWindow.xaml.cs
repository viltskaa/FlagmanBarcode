﻿using BarcodeScannerBusinessLogic.BusinessLogic;
using BarcodeScannerContracts.BusinessLogicContracts;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BarcodeScanner
{
    public partial class MainWindow : Window
    {
        private readonly IBarcodeProductLogic _barcodeProductLogic;
        private readonly IQrStuffLogic _qrStuffLogic;

        public MainWindow(IBarcodeProductLogic barcodeProductLogic, IQrStuffLogic qrStuffLogic)
        {
            _barcodeProductLogic = barcodeProductLogic;
            _qrStuffLogic = qrStuffLogic;

            InitializeComponent();
            LoadBarcodes();
            LoadPrinters();
            SaveProducts();

            SetFocusOnInput();

            this.PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) &&
                e.Key == Key.D)
            {
                ConfirmAndDeleteAll();
            }
        }

        private void ConfirmAndDeleteAll()
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить все записи?",
                "Подтверждение удаления",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                _qrStuffLogic.DeleteAll();
                MessageBox.Show("Все записи успешно удалены.");
                LoadBarcodes();
            }
        }

        private void SaveProducts()
        {
            _barcodeProductLogic.DeleteAll();
            _barcodeProductLogic.SaveFilesFromDirectory("C:/Barcodes");
            SetFocusOnInput();
            return;
        }

        private void LoadBarcodesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProducts();
            MessageBox.Show("Данные успешно загружены.");
            SetFocusOnInput();
            return;
        }

        private void LoadBarcodes()
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            var barcodes = _qrStuffLogic.ReadList(new BarcodeScannerContracts.SearchModels.QrStuffSearchModel
            {
                DateFrom = new DateTimeOffset(todayStart).ToLocalTime().ToUnixTimeSeconds(),
                DateTo = new DateTimeOffset(todayEnd).ToLocalTime().ToUnixTimeSeconds()
            });

            if (barcodes == null || !barcodes.Any())
            {
                BarcodeList.ItemsSource = null;
                return;
            }

            var formattedBarcodes = barcodes.Select(b =>$"{DateTimeOffset.FromUnixTimeSeconds(b.Timestamp).ToLocalTime().DateTime:yyyy-MM-dd HH:mm:ss} - {b.Gtin}").ToList();

            BarcodeList.ItemsSource = formattedBarcodes;
        }

        private void LoadPrinters()
        {
            const string preferredModel = "Xprinter XP-370B";
            string preferredPrinterName = null;

            var printerNames = new List<string>();
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

            foreach (ManagementObject printer in searcher.Get())
            {
                string printerName = printer["Name"]?.ToString();
                string printerModel = printer["DriverName"]?.ToString(); 

                if (!string.IsNullOrEmpty(printerName))
                {
                    printerNames.Add(printerName);

                    if (!string.IsNullOrEmpty(printerModel) &&
                        printerModel.Equals(preferredModel, StringComparison.OrdinalIgnoreCase))
                    {
                        preferredPrinterName = printerName;
                    }
                }
            }

            if (preferredPrinterName != null)
            {
                printerNames.Remove(preferredPrinterName);
                printerNames.Insert(0, preferredPrinterName);
            }

            PrinterList.ItemsSource = printerNames;

            if (printerNames.Count > 0)
            {
                PrinterList.SelectedIndex = 0;
            }
        }

        private void BarcodeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string inputBarcode = BarcodeInput.Text.Trim();
                if (string.IsNullOrEmpty(inputBarcode))
                {
                    SetFocusOnInput();
                    return;
                }

                if (!long.TryParse(inputBarcode, out long barcodeGtin))
                {
                    MessageBox.Show("Неверный формат штрих-кода.");
                    BarcodeInput.Clear();
                    SetFocusOnInput();
                    return;
                }

                var product = _barcodeProductLogic.ReadElement(new BarcodeScannerContracts.SearchModels.BarcodeProductSearchModel
                {
                    Gtin = barcodeGtin
                });

                if (product == null)
                {
                    MessageBox.Show("Товар с данным штрих-кодом не найден.");
                    BarcodeInput.Clear();
                    SetFocusOnInput();
                    return;
                }

                int quantity;
                var quantityInputDialog = new QuantityInputDialog(1, 100);
                if (quantityInputDialog.ShowDialog() == true)
                {
                    quantity = quantityInputDialog.Quantity;
                }
                else
                {
                    SetFocusOnInput();
                    return;
                }

                List<System.Drawing.Image> barcodeImages = new List<System.Drawing.Image>();
                List<System.Drawing.Image> qrImages = new List<System.Drawing.Image>();

                for (int i = 0; i < quantity; i++)
                {
                    long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()+(i*10);

                    _qrStuffLogic.Create(new BarcodeScannerContracts.BindingModels.QrStuffBindingModel
                    {
                        Gtin = barcodeGtin,
                        Timestamp = time
                    });

                    var qrImage = GenerateQrCode(barcodeGtin, time);
                    var barcodeImage = System.Drawing.Image.FromFile(product.Filename);

                    barcodeImages.Add(barcodeImage);
                    qrImages.Add(qrImage);
                }

                PrintBarcodesAndQRCodes(barcodeImages, qrImages);

                BarcodeInput.Clear();
                LoadBarcodes();
                SetFocusOnInput();
            }
        }

        public static System.Drawing.Image GenerateQrCode(long gtin, long timestamp)
        {
            string data = $"gtin{gtin},time{timestamp}";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            System.Drawing.Image qrImage = qrCode.GetGraphic(20);
            return qrImage;
        }

        private void PrintBarcodesAndQRCodes(List<System.Drawing.Image> barcodeImages, List<System.Drawing.Image> qrImages)
        {
            if (PrinterList.SelectedItem == null)
            {
                MessageBox.Show("Выберите принтер.");
                return;
            }

            string selectedPrinter = PrinterList.SelectedItem.ToString();

            PrintDocument printDocument = new PrintDocument
            {
                PrinterSettings = new PrinterSettings
                {
                    PrinterName = selectedPrinter
                }
            };

            int currentIndex = 0;
            bool isBarcodePage = true;

            const int rightMargin = 20;

            printDocument.PrintPage += (s, ev) =>
            {
                var imageToPrint = isBarcodePage ? barcodeImages[currentIndex] : qrImages[currentIndex];

                float scaleX = (float)(ev.PageBounds.Width - rightMargin) / imageToPrint.Width;
                float scaleY = (float)ev.PageBounds.Height / imageToPrint.Height;
                float scale = Math.Min(scaleX, scaleY);

                float scaledWidth = imageToPrint.Width * scale;
                float scaledHeight = imageToPrint.Height * scale;

                float offsetX = (ev.PageBounds.Width - scaledWidth - rightMargin) / 2;
                float offsetY = (ev.PageBounds.Height - scaledHeight) / 2;

                using (Graphics g = ev.Graphics)
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                    g.DrawImage(imageToPrint, offsetX, offsetY, scaledWidth, scaledHeight);
                }

                if (!isBarcodePage)
                {
                    currentIndex++;
                }

                isBarcodePage = !isBarcodePage;
                ev.HasMorePages = currentIndex < barcodeImages.Count;
            };

            try
            {
                printDocument.Print();
                MessageBox.Show("Печать успешно завершена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}");
            }
        }

        public class LabelConfig
        {
            public float LabelWidthCm { get; set; }
            public float LabelHeightCm { get; set; }
        }

        private LabelConfig LoadConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Конфигурационный файл не найден.");
            }

            string configContent = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<LabelConfig>(configContent);
        }

        private void BarcodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFocusOnInput();
        }

        private void PrinterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFocusOnInput();
        }

        private void SetFocusOnInput()
        {
            BarcodeInput.Focus();
        }
    }
}
