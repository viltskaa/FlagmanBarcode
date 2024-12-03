using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.ViewModels;

namespace BarcodeScannerContracts.StorageContracts
{
    public interface IBarcodeProductStorage
    {
        List<BarcodeProductViewModel> GetAll();
        List<BarcodeProductViewModel> GetFilteredAll(BarcodeProductSearchModel model);
        BarcodeProductViewModel? GetElement(BarcodeProductSearchModel model);
        BarcodeProductViewModel? Insert(BarcodeProductBindingModel model);
    }
}
