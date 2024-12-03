using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.ViewModels;
using BarcodeScannerDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BarcodeScannerDatabaseImplement.Models
{
    [DataContract]
    public class BarcodeProduct : IBarcodeProductModel
    {
        [Key]
        [DataMember]
        public long Gtin { get; set; }
        [Required]
        [DataMember]
        public string Filename { get; set; } = string.Empty;
        [ForeignKey("Gtin")]
        public virtual List<QrStuff> QrStuffs { get; set; } = new();

        public static BarcodeProduct? Create(BarcodeProductBindingModel? model)
        {
            if (model == null)
            {
                return null;
            }

            return new BarcodeProduct()
            {
                Gtin = model.Gtin,
                Filename = model.Filename
            };
        }

        public void Update(BarcodeProductBindingModel? model)
        {
            if (model == null)
            {
                return;
            }
            Gtin = model.Gtin;
            Filename = model.Filename;
        }

        public BarcodeProductViewModel GetViewModel
        {
            get
            {
                return new BarcodeProductViewModel
                {
                    Gtin = Gtin,
                    Filename = Filename
                };
            }
        }
    }
}
