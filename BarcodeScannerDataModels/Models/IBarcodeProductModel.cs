using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerDataModels.Models
{
    public interface IBarcodeProductModel
    {
        long Gtin {  get; }
        string Filename { get; }
    }
}
