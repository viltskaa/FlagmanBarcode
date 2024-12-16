using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerContracts.BusinessLogicContracts
{
    public interface IBarcodeProductLogic
    {
        List<BarcodeProductViewModel>? ReadList(BarcodeProductSearchModel? model);
        BarcodeProductViewModel? ReadElement(BarcodeProductSearchModel model);
        bool Create(BarcodeProductBindingModel model);
        void SaveFilesFromDirectory(string directory);
        void DeleteAll();
    }
}
