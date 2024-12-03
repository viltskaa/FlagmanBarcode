using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerContracts.SearchModels
{
    public class BarcodeProductSearchModel
    {
        public long? Gtin { get; set; }

        public string? Filename { get; set; }
    }
}
