using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerContracts.StorageContracts
{
    public interface IQrStuffStorage
    {
        List<QrStuffViewModel> GetAll();
        List<QrStuffViewModel> GetFilteredAll(QrStuffSearchModel model);
        QrStuffViewModel? GetElement(QrStuffSearchModel model);
        QrStuffViewModel? Insert(QrStuffBindingModel model);
        void DeleteAll();
    }
}
