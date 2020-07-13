using Repac.Data;
using Repac.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace Repac
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            List<CashRegisterScan> itemSource;

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "RepacCashRegister.db");
            using (var db = new DatabaseContext(dbPath))
            {
                // Ensure database is created
                db.Database.EnsureCreated();
                if (db.CashRegisterScans.Count() == 0)
                {
                    db.Add(new CashRegisterScan() { ScanId = Guid.NewGuid(), TagId = Guid.NewGuid(), ScanDirection = true, Timestamp = DateTime.Now });
                    db.Add(new CashRegisterScan() { ScanId = Guid.NewGuid(), TagId = Guid.NewGuid(), ScanDirection = false, Timestamp = DateTime.Now });
                    db.Add(new CashRegisterScan() { ScanId = Guid.NewGuid(), TagId = Guid.NewGuid(), ScanDirection = true, Timestamp = DateTime.Now });
                    db.SaveChanges();
                  }
                itemSource = db.CashRegisterScans.ToList();
            }

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
