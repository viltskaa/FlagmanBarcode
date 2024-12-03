using BarcodeScannerBusinessLogic.BusinessLogic;
using BarcodeScannerContracts.BusinessLogicContracts;
using QRCoder;
using System.Drawing;
using System.Drawing.Printing;
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

            SetFocusOnInput();
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
            var printServer = new LocalPrintServer();
            var printers = printServer.GetPrintQueues().Select(pq => pq.Name).ToList();

            PrinterList.ItemsSource = printers;

            if (printers.Count > 0)
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

                var product = _barcodeProductLogic.ReadElement(new BarcodeScannerContracts.SearchModels.BarcodeProductSearchModel
                {
                    Gtin = long.Parse(inputBarcode)
                });

                if (product == null)
                {
                    MessageBox.Show("Товар с данным штрих-кодом не найден.");
                    BarcodeInput.Clear();
                    SetFocusOnInput();
                    return;
                }

                long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                _qrStuffLogic.Create(new BarcodeScannerContracts.BindingModels.QrStuffBindingModel
                {
                    Gtin = long.Parse(inputBarcode),
                    Timestamp = time
                });

                PrintBarcodeAndQRCode(product.Filename, product.Gtin, time);

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

        private void PrintBarcodeAndQRCode(string barcodeFilename, long gtin, long timestamp)
        {
            if (string.IsNullOrEmpty(barcodeFilename) || !System.IO.File.Exists(barcodeFilename))
            {
                MessageBox.Show("Файл изображения штрих-кода не найден.");
                return;
            }

            if (PrinterList.SelectedItem == null)
            {
                MessageBox.Show("Выберите принтер.");
                return;
            }

            string selectedPrinter = PrinterList.SelectedItem.ToString();
            var barcodeImage = System.Drawing.Image.FromFile(barcodeFilename);
            var qrImage = GenerateQrCode(gtin, timestamp);

            PrintDocument printDocument = new PrintDocument
            {
                PrinterSettings = new PrinterSettings
                {
                    PrinterName = selectedPrinter
                }
            };

            int pageNumber = 1;

            printDocument.PrintPage += (s, ev) =>
            {
                if (pageNumber == 1)
                {
                    float maxWidth = ev.MarginBounds.Width;
                    float maxHeight = ev.MarginBounds.Height;

                    float imageWidth = maxWidth / 3;
                    float imageHeight = maxHeight / 6;

                    var scaledImage = new Bitmap(barcodeImage, new System.Drawing.Size((int)imageWidth, (int)imageHeight));

                    float x = (ev.MarginBounds.Width - imageWidth) / 2 + ev.MarginBounds.Left;
                    float y = (ev.MarginBounds.Height - imageHeight) / 2 + ev.MarginBounds.Top;

                    ev.Graphics.DrawImage(scaledImage, new System.Drawing.RectangleF(x, y, imageWidth, imageHeight));

                    pageNumber++; 
                    ev.HasMorePages = true;
                }
                else if (pageNumber == 2) 
                {
                    float maxWidth = ev.MarginBounds.Width;
                    float maxHeight = ev.MarginBounds.Height;

                    float imageWidth = maxWidth / 3;
                    float imageHeight = maxHeight / 5;

                    var scaledImage = new Bitmap(qrImage, new System.Drawing.Size((int)imageWidth, (int)imageHeight));

                    float x = (ev.MarginBounds.Width - imageWidth) / 2 + ev.MarginBounds.Left;
                    float y = (ev.MarginBounds.Height - imageHeight) / 2 + ev.MarginBounds.Top;

                    ev.Graphics.DrawImage(scaledImage, new System.Drawing.RectangleF(x, y, imageWidth, imageHeight));

                    ev.HasMorePages = false;
                }
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
