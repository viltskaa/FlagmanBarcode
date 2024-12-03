using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.ViewModels;
using BarcodeScannerDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerDatabaseImplement.Models
{
    [DataContract]
    public class QrStuff : IQrStuffModel
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [Required]
        [DataMember]
        public long Gtin { get; set; }
        public virtual BarcodeProduct BarcodeProduct { get; set; }
        [Required]
        [DataMember]
        public long Timestamp { get; set; }

        public static QrStuff? Create(QrStuffBindingModel? model)
        {
            if (model == null)
            {
                return null;
            }

            return new QrStuff()
            {
                Id = model.Id,
                Gtin = model.Gtin,
                Timestamp = model.Timestamp
            };
        }

        public void Update(QrStuffBindingModel? model)
        {
            if (model == null)
            {
                return;
            }
            Gtin = model.Gtin;
            Timestamp = model.Timestamp;
        }

        public QrStuffViewModel GetViewModel
        {
            get
            {
                return new QrStuffViewModel
                {
                    Id = Id,
                    Gtin = Gtin,
                    Timestamp = Timestamp
                };
            }
        }
    }
}
