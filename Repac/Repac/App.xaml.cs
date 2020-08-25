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

            using (var db = new DatabaseContext(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "RepacCashRegister3.db")))
            {
                db.Database.EnsureCreated();
                User user = db.Users.Where(u => u.UserId == Guid.Parse("666b55ac-96e7-47b0-96d1-38622d0b176e")).FirstOrDefault();
                if (user == null)
                {
                    db.Add(new User()
                    {
                        UserId = Guid.Parse("666b55ac-96e7-47b0-96d1-38622d0b176e"),
                        FirstName = "Sasha",
                        LastName = "Artemiev",
                        RegistryDate = DateTime.Now,
                    });
                    db.SaveChanges();
                }
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
