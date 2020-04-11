using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Repac
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        Slides CurrentSlide { get; set; } = Slides.First;
        int ScannedProducts { get; set; } = 0;
        int ProductsCredit { get; set; } = 0;
        bool EditingItemsQuantityMode { get; set; } = false;

        enum Slides
        {
            First,
            Second,
            Third
        }

        public MainPage()
        {
            InitializeComponent();
            FirstSlideActivate();
        }

        private void ButtonLeft_Clicked(object sender, EventArgs e)
        {
            switch (CurrentSlide)
            {
                case Slides.First:
                    break;
                case Slides.Second:
                    FirstSlideActivate();
                    break;
                case Slides.Third:
                    SecondSlideActivate();
                    break;
            }
        }
        private void ButtonRight_Clicked(object sender, EventArgs e)
        {
            switch (CurrentSlide)
            {
                case Slides.First:
                    SecondSlideActivate();
                    break;
                case Slides.Second:
                    ThirdSlideActivate();
                    break;
                case Slides.Third:
                    break;
            }
        }

        private void FirstSlideActivate()
        {
            CurrentSlide = Slides.First;

            Footer.IsVisible = false;
            FirstScreen.IsVisible = true;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            ButtonAdd.IsVisible = false;
        }
        private void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;

            Footer.IsVisible = true;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
            ThirdScreen.IsVisible = false;
            ButtonAdd.IsVisible = true;
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            Footer.IsVisible = false;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = true;
            ButtonAdd.IsVisible = true;
        }

        private void AddProductCredit(object sender, EventArgs e)
        {
            ProductsCredit += 1;
            CounterLabelRight.Text = this.ProductsCredit.ToString();

            if (ScannedProducts <= ProductsCredit)
            {
                CounterLabelRight.TextColor = Color.Black;
                CreditsErrorLabel.IsVisible = false;

                Footer.IsVisible = true;
                FooterTextLabel.Text = "TAPER CARTE/TEL. POUR COMPLÉTER LA TRANSACTION.";
                FooterTextLabel.TextColor = AddCreditButton.BackgroundColor;
            }

        }

        private void FirstScreen_Tapped(object sender, EventArgs e)
        {
            SecondSlideActivate();
        }

        private void FooterIcon_Tapped(object sender, EventArgs e)
        {
            switch (CurrentSlide)
            {
                case Slides.Second:
                    ThirdSlideActivate();
                    break;
                case Slides.Third:
                    //for forth slide
                    break;
            }
        }

        private void ButtonAdd_Clicked(object sender, EventArgs e)
        {
            ScannedProducts += 1;
            CounterLabel.Text = this.ScannedProducts.ToString();
            CounterLabelLeft.Text = this.ScannedProducts.ToString();

            if (ScannedProducts > ProductsCredit)
            {
                CounterLabelRight.TextColor = Color.DarkRed;
                CreditsErrorLabel.IsVisible = true;
            }
            else
            {
                CounterLabelRight.TextColor = Color.Black;
                CreditsErrorLabel.IsVisible = false;
            }
        }

        private void ButtonSub_Clicked(object sender, EventArgs e)
        {
            ScannedProducts -= 1;
            CounterLabel.Text = this.ScannedProducts.ToString();
            CounterLabelLeft.Text = this.ScannedProducts.ToString();

            if (ScannedProducts > ProductsCredit)
            {
                CounterLabelRight.TextColor = Color.DarkRed;
                CreditsErrorLabel.IsVisible = true;
            }
            else
            {
                CounterLabelRight.TextColor = Color.Black;
                CreditsErrorLabel.IsVisible = false;
            }
        }

        private void RepacLogo_Tapped(object sender, EventArgs e)
        {
            if (CurrentSlide == Slides.Second)
            {
                EditingItemsQuantityMode = !EditingItemsQuantityMode;
                
                if (EditingItemsQuantityMode)
                {
                    ButtonAdd.Opacity = 100;
                    ItemsAdjustmentStack.Opacity = 100;
                    ItemsScannesGrid.Opacity = 0;
                }
                else
                {
                    ButtonAdd.Opacity = 0;
                    ItemsAdjustmentStack.Opacity = 0;
                    ItemsScannesGrid.Opacity = 100;
                }
            }
        }
    }
}
