using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerContracts.SearchModels
{
    public class QrStuffSearchModel
    {
        public int? Id { get; set; }

        public int? Gtin { get; set; }

        public long? Timestamp { get; set; }

        public long? DateFrom { get; set; } 
        public long? DateTo { get; set; } 
    }
}
