using BarcodeScannerDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerContracts.BindingModels
{
    public class QrStuffBindingModel : IQrStuffModel
    {
        public int Id { get; set; }

        public long Gtin { get; set; }

        public long Timestamp { get; set; }
    }
}
