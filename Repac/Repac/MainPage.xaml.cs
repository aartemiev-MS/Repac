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
        Slides CurrentSlide { get; set; }
        enum Slides
        {
            First,
            Second,
            Third
        }

        public MainPage()
        {
            InitializeComponent();
            CurrentSlide = Slides.First;
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
        }
        private void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;

            Footer.IsVisible = true;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
            ThirdScreen.IsVisible = false;
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            Footer.IsVisible = false;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = true;
        }
    }
}
