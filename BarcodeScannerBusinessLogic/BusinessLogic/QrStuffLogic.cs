using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.BusinessLogicContracts;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.StorageContracts;
using BarcodeScannerContracts.ViewModels;

namespace BarcodeScannerBusinessLogic.BusinessLogic
{
    public class QrStuffLogic : IQrStuffLogic
    {
        private readonly IQrStuffStorage _qrStuffStorage;
        private readonly IBarcodeProductLogic _barcodeProductLogic;
        public QrStuffLogic(IQrStuffStorage qrStuffStorage, IBarcodeProductLogic barcodeProductLogic)
        {
            _qrStuffStorage = qrStuffStorage;
            _barcodeProductLogic = barcodeProductLogic;
        }

        public bool ProcessBarcodeScan(long barcode)
        {
            var product = _barcodeProductLogic.ReadElement(new BarcodeProductSearchModel { Gtin = barcode });
            if (product == null)
            {
                Console.WriteLine("Продукт с данным штрих-кодом не найден.");
                return false;
            }

            var qrStuff = new QrStuffBindingModel
            {
                Gtin = barcode,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return _qrStuffStorage.Insert(qrStuff) != null;
        }

        public bool Create(QrStuffBindingModel model)
        {
            if (_qrStuffStorage.Insert(model) == null)
            {
                return false;
            }
            return true;
        }

        public QrStuffViewModel? ReadElement(QrStuffSearchModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var element = _qrStuffStorage.GetElement(model);
            if (element == null)
            {
                return null;
            }
            return element;
        }

        public List<QrStuffViewModel>? ReadList(QrStuffSearchModel? model)
        {
            var list = model == null ? _qrStuffStorage.GetAll() : _qrStuffStorage.GetFilteredAll(model);
            if (list == null)
            {
                return null;
            }
            return list;
        }
    }
}
