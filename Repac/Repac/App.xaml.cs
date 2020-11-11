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
        public App(bool takeOffMode=false)
        {
            InitializeComponent();

            MainPage = new MainPage(takeOffMode);
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
