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
    public interface IQrStuffLogic
    {
        List<QrStuffViewModel>? ReadList(QrStuffSearchModel? model);
        QrStuffViewModel? ReadElement(QrStuffSearchModel model);
        bool Create(QrStuffBindingModel model);
        bool ProcessBarcodeScan(long barcode);
    }
}
