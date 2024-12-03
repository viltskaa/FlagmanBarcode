using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.StorageContracts;
using BarcodeScannerContracts.ViewModels;
using BarcodeScannerDatabaseImplement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerDatabaseImplement.Implements
{
    public class BarcodeProductStorage : IBarcodeProductStorage
    {
        public List<BarcodeProductViewModel> GetAll()
        {
            using var context = new BarcodeScannerDatabase();
            return context.BarcodeProducts.Select(x => x.GetViewModel).ToList();
        }

        public BarcodeProductViewModel? GetElement(BarcodeProductSearchModel model)
        {
            using var context = new BarcodeScannerDatabase();

            if (model.Gtin.HasValue)
                return context.BarcodeProducts.FirstOrDefault(x => x.Gtin == model.Gtin)?.GetViewModel;

            return null;
        }

        public List<BarcodeProductViewModel> GetFilteredAll(BarcodeProductSearchModel model)
        {
            if (model.Gtin != null || string.IsNullOrEmpty(model.Filename))
            {
                return new();
            }
            using var context = new BarcodeScannerDatabase();
            return context.BarcodeProducts.Where(x => x.Gtin == model.Gtin && x.Filename.Contains(model.Filename)).Select(x => x.GetViewModel).ToList();
        }

        public BarcodeProductViewModel? Insert(BarcodeProductBindingModel model)
        {
            var newBarcode = BarcodeProduct.Create(model);
            if (newBarcode == null)
            {
                return null;
            }
            using var context = new BarcodeScannerDatabase();
            context.BarcodeProducts.Add(newBarcode);
            context.SaveChanges();
            return newBarcode.GetViewModel;
        }
    }
}
