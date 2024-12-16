using BarcodeScannerContracts.BindingModels;
using BarcodeScannerContracts.SearchModels;
using BarcodeScannerContracts.StorageContracts;
using BarcodeScannerContracts.ViewModels;
using BarcodeScannerDatabaseImplement.Models;
using Microsoft.EntityFrameworkCore;

namespace BarcodeScannerDatabaseImplement.Implements
{
    public class QrStuffStorage : IQrStuffStorage
    {
        public List<QrStuffViewModel> GetAll()
        {
            using var context = new BarcodeScannerDatabase();
            return context.QrStuffs.Select(x => x.GetViewModel).ToList();
        }

        public QrStuffViewModel? GetElement(QrStuffSearchModel model)
        {
            using var context = new BarcodeScannerDatabase();

            if (model.Id.HasValue)
                return context.QrStuffs.FirstOrDefault(x => x.Id == model.Id)?.GetViewModel;
            else if (model.Gtin.HasValue)
                return context.QrStuffs.FirstOrDefault(x => x.Gtin == model.Gtin)?.GetViewModel;

            return null;
        }

        public List<QrStuffViewModel> GetFilteredAll(QrStuffSearchModel model)
        {
            using var context = new BarcodeScannerDatabase();

            var query = context.QrStuffs.AsQueryable();

            if (model.Gtin != null)
            {
                query = query.Where(x => x.Gtin == model.Gtin);
            }

            if (model.DateFrom.HasValue)
            {
                query = query.Where(x => x.Timestamp >= model.DateFrom);
            }

            if (model.DateTo.HasValue)
            {
                query = query.Where(x => x.Timestamp < model.DateTo);
            }

            return query.OrderByDescending(x => x.Timestamp).Select(x => x.GetViewModel).ToList();
        }


        public QrStuffViewModel? Insert(QrStuffBindingModel model)
        {
            var newQr = QrStuff.Create(model);
            if (newQr == null)
            {
                return null;
            }
            using var context = new BarcodeScannerDatabase();
            context.QrStuffs.Add(newQr);
            context.SaveChanges();
            return newQr.GetViewModel;
        }
    }
}
