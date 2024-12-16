using BarcodeScannerBusinessLogic.BusinessLogic;
using BarcodeScannerContracts.BusinessLogicContracts;
using ControlzEx.Standard;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Svg;

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
                    long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    _qrStuffLogic.Create(new BarcodeScannerContracts.BindingModels.QrStuffBindingModel
                    {
                        Gtin = barcodeGtin,
                        Timestamp = time
                    });

                    var qrImage = GenerateQrCode(barcodeGtin, time);

                    if (product.Filename.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        var svgDoc = SvgDocument.Open(product.Filename);
                        var bitmap = svgDoc.Draw();
                        barcodeImages.Add(bitmap);
                    }
                    else
                    {
                        using (var barcodeImage = System.Drawing.Image.FromFile(product.Filename))
                        {
                            barcodeImages.Add(barcodeImage.Clone() as System.Drawing.Image);
                        }
                    }

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
                },
                DefaultPageSettings = { Margins = new Margins(0, 0, 0, 0) }
            };

            int currentIndex = 0;
            bool isBarcodePage = true;

            LabelConfig config;
            try
            {
                config = LoadConfig("config.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}");
                return;
            }

            printDocument.PrintPage += (s, ev) =>
            {
                const float cmToPixels = 37.8f;
                float targetWidth = config.LabelWidthCm * cmToPixels;
                float targetHeight = config.LabelHeightCm * cmToPixels;

                using (Bitmap resizedImage = new Bitmap((int)targetWidth, (int)targetHeight))
                {
                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        g.Clear(System.Drawing.Color.White);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        var imageToPrint = isBarcodePage ? barcodeImages[currentIndex] : qrImages[currentIndex];

                        float scaleX = targetWidth / imageToPrint.Width;
                        float scaleY = targetHeight / imageToPrint.Height;
                        float scale = Math.Min(scaleX, scaleY);

                        float scaledWidth = imageToPrint.Width * scale;
                        float scaledHeight = imageToPrint.Height * scale;
                        float offsetX = (targetWidth - scaledWidth) / 2;
                        float offsetY = (targetHeight - scaledHeight) / 2;

                        g.DrawImage(imageToPrint, offsetX, offsetY, scaledWidth, scaledHeight);
                    }

                    ev.Graphics.DrawImage(resizedImage, ev.MarginBounds.Left, ev.MarginBounds.Top);
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
