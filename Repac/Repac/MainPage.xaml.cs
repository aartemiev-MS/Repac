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
            Third, 
            Fourth
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
                case Slides.Second:
                    FirstSlideActivate();
                    break;
                case Slides.Third:
                    SecondSlideActivate();
                    break;
                case Slides.Fourth:
                    ThirdSlideActivate();
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
                    FourthSlideActivate();
                    break;
                case Slides.Fourth:
                    FirstSlideActivate();
                    break;
            }
        }

        private void FirstSlideActivate()
        {
            CurrentSlide = Slides.First;
            
            Footer.IsVisible = false;
            InviteIcon.IsVisible = true;
            PhilippeAvatar.IsVisible = false;
         //   UserGreetings.IsVisible = false;
            FirstScreen.IsVisible = true;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
        }
        private void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;

            Footer.IsVisible = true;
            InviteIcon.IsVisible = true;
            PhilippeAvatar.IsVisible = false; 
           // UserGreetings.IsVisible = false;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            Footer.IsVisible = false;
            InviteIcon.IsVisible = false;
          //  UserGreetings.IsVisible = true;
            PhilippeAvatar.IsVisible = true;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = true;
            FourthScreen.IsVisible = false;
        }
        private void FourthSlideActivate()
        {
            CurrentSlide = Slides.Fourth;

            Footer.IsVisible = false;
            InviteIcon.IsVisible = false;
            PhilippeAvatar.IsVisible = true;
           // UserGreetings.IsVisible = false;
            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = true;
        }
    }
}
