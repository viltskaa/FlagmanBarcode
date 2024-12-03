using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerDataModels.Models
{
    public interface IQrStuffModel
    {
        int Id { get; }
        long Gtin { get; }
        long Timestamp { get; }
    }
}
