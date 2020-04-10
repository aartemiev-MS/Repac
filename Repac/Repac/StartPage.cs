using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace Repac
{
    class StartPage : ContentPage
    {
        public StartPage()
        {
            Label header = new Label() { Text = "Sasha says Hello" };
            this.Content = header;
        }
    }
}
