using BarcodeScannerDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerContracts.BindingModels
{
    public class BarcodeProductBindingModel : IBarcodeProductModel
    {
        public long Gtin { get; set; }

        public string Filename { get; set; } = string.Empty;
    }
}
