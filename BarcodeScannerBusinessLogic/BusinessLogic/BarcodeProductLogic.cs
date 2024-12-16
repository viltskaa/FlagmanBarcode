using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.BusinessLogicContracts;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.StorageContracts;
using BarcodeScannerContracts.ViewModels;

namespace BarcodeScannerBusinessLogic.BusinessLogic
{
    public class BarcodeProductLogic : IBarcodeProductLogic
    {
        private readonly IBarcodeProductStorage _barcodeProductStorage;
        public BarcodeProductLogic(IBarcodeProductStorage barcodeProductStorage)
        {
            _barcodeProductStorage = barcodeProductStorage;
        }
        public bool Create(BarcodeProductBindingModel model)
        {
            if (_barcodeProductStorage.Insert(model) == null)
            {
                return false;
            }
            return true;
        }

        public BarcodeProductViewModel? ReadElement(BarcodeProductSearchModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var element = _barcodeProductStorage.GetElement(model);
            if (element == null)
            {
                return null;
            }
            return element;
        }

        public List<BarcodeProductViewModel>? ReadList(BarcodeProductSearchModel? model)
        {
            var list = model == null ? _barcodeProductStorage.GetAll() : _barcodeProductStorage.GetFilteredAll(model);
            if (list == null)
            {
                return null;
            }
            return list;
        }

        public void DeleteAll()
        {
            _barcodeProductStorage.DeleteAll();
        }

        public void SaveFilesFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Папка {directoryPath} не найдена.");
            }

            var files = Directory.GetFiles(directoryPath);

            foreach (var file in files)
            {
                string gtin = Path.GetFileNameWithoutExtension(file);

                var barcodeProduct = new BarcodeProductBindingModel
                {
                    Gtin = long.TryParse(gtin, out var parsedGtin) ? parsedGtin : 0,
                    Filename = file
                };

                Create(barcodeProduct);
            }
        }
    }
}
