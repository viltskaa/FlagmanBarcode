using BarcodeScannerDatabaseImplement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScannerDatabaseImplement
{
    public class BarcodeScannerDatabase : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=clinic_queue.db");
            }
            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<BarcodeProduct> BarcodeProducts { get; set; }
        public virtual DbSet<QrStuff> QrStuffs { get; set; }
    }
}
