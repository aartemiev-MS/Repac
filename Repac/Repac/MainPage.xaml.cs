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
            Third,
            Fourth
        }

        public MainPage()
        {
            InitializeComponent();
            FirstSlideActivate();
        }

        #region "Events"
        private void AddProductCredit(object sender, EventArgs e)
        {
            ProductsCredit += 1;
            CounterLabelRight.Text = this.ProductsCredit.ToString();

            if (ScannedProducts <= ProductsCredit)
            {
                CounterLabelRight.TextColor = Color.Black;
                CreditsError.Opacity = 0;

                Footer.IsVisible = true;
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
                    FourthSlideActivate();
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
                CreditsError.Opacity = 100;
            }
            else
            {
                CounterLabelRight.TextColor = Color.Black;
                CreditsError.Opacity = 0;
            }
        }

        private void ButtonSub_Clicked(object sender, EventArgs e)
        {
            if (ScannedProducts > 0)
            {
                ScannedProducts -= 1;
                CounterLabel.Text = this.ScannedProducts.ToString();
                CounterLabelLeft.Text = this.ScannedProducts.ToString();

                if (ScannedProducts > ProductsCredit)
                {
                    CounterLabelRight.TextColor = Color.DarkRed;
                    CreditsError.Opacity = 100;
                }
                else
                {
                    CounterLabelRight.TextColor = Color.Black;
                    CreditsError.Opacity = 0;
                }
            }
        }

        private void RepacLogo_Tapped(object sender, EventArgs e)
        {
            if (CurrentSlide == Slides.Second)
            {
                EditingItemsQuantityMode = !EditingItemsQuantityMode;

                if (EditingItemsQuantityMode)
                {
                    ImageAdd.Opacity = 100;
                    ImageSubstract.Opacity = 100;
                    ItemsAdjustmentLabel.Opacity = 100;
                    ItemsScannesLabel.Opacity = 0;
                }
                else
                {
                    ImageAdd.Opacity = 0;
                    ImageSubstract.Opacity = 0;
                    ItemsAdjustmentLabel.Opacity = 0;
                    ItemsScannesLabel.Opacity = 100;
                }
            }
        }

        #endregion

        #region "Functions"
        private void FirstSlideActivate()
        {
            CurrentSlide = Slides.First;

            FooterTextLabel.Text = "TAPER CARTE/TEL. POUR VOUS IDENTIFIER.";
            FooterTextLabel.TextColor = Color.Black;
            Footer.IsVisible = false;

            InviteIcon.IsVisible = true;
            PhilippeAvatar.IsVisible = false;

            FirstScreen.IsVisible = true;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
        }
        private void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;

            CounterLabel.Text = this.ScannedProducts.ToString();

            ImageAdd.Opacity = 0;
            ImageSubstract.Opacity = 0;
            ItemsAdjustmentLabel.Opacity = 0;
            ItemsScannesLabel.Opacity = 100;

            Footer.IsVisible = true;
            InviteIcon.IsVisible = true;
            PhilippeAvatar.IsVisible = false;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            CounterLabelLeft.Text = this.ScannedProducts.ToString();
            CounterLabelRight.Text = this.ProductsCredit.ToString();

            Footer.IsVisible = false;
            InviteIcon.IsVisible = false;
            PhilippeAvatar.IsVisible = true;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = true;
            FourthScreen.IsVisible = false;

            FooterTextLabel.Text = "TAPER CARTE/TEL. POUR COMPLÉTER LA TRANSACTION.";
            FooterTextLabel.TextColor = AddCreditButton.BackgroundColor;
            Footer.IsVisible = ScannedProducts <= ProductsCredit ? true : false;
        }
        private void FourthSlideActivate()
        {
            CurrentSlide = Slides.Fourth;

            Footer.IsVisible = false;
            InviteIcon.IsVisible = false;
            PhilippeAvatar.IsVisible = true;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = true;
        }

        private void NullifyCycle()
        {
            ScannedProducts = 0;
            ProductsCredit = 0;
            EditingItemsQuantityMode = false;

            FirstSlideActivate();
        }

        private void MerciTapped(object sender, EventArgs e)
        {
            NullifyCycle();
        }

        #endregion
    }
}
