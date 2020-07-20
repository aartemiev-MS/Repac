using Repac.Data;
using Repac.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        DatabaseContext DBInstance { get; set; }

        string DbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "RepacCashRegister1.db");

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

            List<CashRegisterScan> itemSource;

            DisplayReport();

            MessagingCenter.Subscribe<string>(this, "TagScanned", (tag) => { TagScanned(tag); });
        }

        #region "Events"
        private async void AddProductCredit(object sender, EventArgs e)
        {
            await Fade(CounterGridRight, 200);
            ProductsCredit += 1;
            CounterLabelRight.Text = this.ProductsCredit.ToString();
            await Appear(CounterGridRight, 200);

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

        private async void ButtonAdd_Clicked(object sender, EventArgs e)
        {
            if (EditingItemsQuantityMode)
            {
                AddScannedItem();
            }
        }

        private async void ButtonSub_Clicked(object sender, EventArgs e)
        {
            if (EditingItemsQuantityMode)
            {

                SubScannedItem();
            }
        }

        private async void RepacLogo_Tapped(object sender, EventArgs e)
        {
            if (CurrentSlide == Slides.Second)
            {
                EditingItemsQuantityMode = !EditingItemsQuantityMode;

                if (EditingItemsQuantityMode)
                {
                    ItemsAdjustmentLabel.Opacity = 100;
                    ItemsScannesLabel.Opacity = 0;

                    Appear(ImageSubstract, 300);
                    Appear(ImageAdd, 300);
                    //ImageAdd.Opacity = 100;
                    //ImageSubstract.Opacity = 100;
                }
                else
                {
                    Fade(ImageSubstract, 300);
                    await Fade(ImageAdd, 300);
                    //ImageAdd.Opacity = 0;
                    //ImageSubstract.Opacity = 0;
                    ItemsAdjustmentLabel.Opacity = 0;
                    ItemsScannesLabel.Opacity = 100;
                }
            }
        }

        async Task Appear(View element, UInt32 time)
        {
            await element.FadeTo(1, time);
        }

        async Task Fade(View element, UInt32 time)
        {
            await element.FadeTo(0, time);
        }

        private void ItemsCounter_Tapped(object sender, EventArgs e)
        {
            AddScannedItem();
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
            UserInfo.IsVisible = false;

            FirstScreen.IsVisible = true;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
        }
        private async void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;

            CounterLabel.Text = this.ScannedProducts.ToString();

            ImageAdd.Opacity = 0;
            ImageSubstract.Opacity = 0;
            ItemsAdjustmentLabel.Opacity = 0;
            ItemsScannesLabel.Opacity = 100;

            Footer.IsVisible = true;

            InviteIcon.IsVisible = true;
            UserInfo.IsVisible = false;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
            await Appear(ItemsCounterBackground, 300);
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            CounterLabelLeft.Text = this.ScannedProducts.ToString();
            CounterLabelRight.Text = this.ProductsCredit.ToString();

            Footer.IsVisible = false;

            InviteIcon.IsVisible = false;
            UserInfo.IsVisible = true;

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
            UserInfo.IsVisible = true;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = true;

            NullifyTimerSet();
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

        private void NullifyTimerSet()
        {
            int counter = 0;           // timer will work for 6 seconds (12 * 0.5sec)
            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
             {
                 if (counter < 12)     // we check each 0.5 seconds
                 {
                     if (CurrentSlide == Slides.Fourth)
                     {
                         counter += 1;
                         return true;  // user has't tapped on screen yet. we understand it because the fourth slide is still active
                     }
                     else
                     {
                         return false; // user has tapped on screen and cycle has already been nullified. we dont need this timer anymore
                     }
                 }
                 else
                 {
                     NullifyCycle();   // 6 seconds of timer has passed so we nullify the cycle
                     return false;
                 }
             });
        }

        private async void AddScannedItem()
        {
            if (EditingItemsQuantityMode)
            {
                await ImageAdd.FadeTo(0, 100);
                await ImageAdd.FadeTo(1, 100);
            }
            await Fade(ItemsCounter, 200);
            ScannedProducts += 1;
            CounterLabel.Text = this.ScannedProducts.ToString();
            CounterLabelLeft.Text = this.ScannedProducts.ToString();
            await Appear(ItemsCounter, 200);

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

        private async void SubScannedItem()
        {
            if (ScannedProducts > 0)
            {
                if (EditingItemsQuantityMode)
                {
                    await ImageSubstract.FadeTo(0, 100);
                    await ImageSubstract.FadeTo(1, 100);
                }
                await Fade(ItemsCounter, 200);
                ScannedProducts -= 1;
                CounterLabel.Text = this.ScannedProducts.ToString();
                CounterLabelLeft.Text = this.ScannedProducts.ToString();
                await Appear(ItemsCounter, 200);
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
        #endregion

        #region Test stuff
        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            using (var db = new DatabaseContext(DbPath))
            {
                db.CashRegisterScans.RemoveRange(db.CashRegisterScans.ToList());

                List<User> usersSource = db.Users.ToList();
                for (int i = 0; i < usersSource.Count; i++)
                {
                    usersSource[i].Credits = 0;
                }

                db.SaveChanges();
                DisplayReport();
            }
        }
        private void ScanInButton_Clicked(object sender, EventArgs e) => AddScan(true);

        private void ScanOutButton_Clicked(object sender, EventArgs e) => AddScan(false);

        private void AddScan(bool scanDirection)
        {
            using (var db = new DatabaseContext(DbPath))
            {
                User user = db.Users.Where(u => u.UserId == Guid.Parse("666b55ac-96e7-47b0-96d1-38622d0b176e")).FirstOrDefault();
                
                user.Scan(scanDirection);

                db.Add(new CashRegisterScan()
                {
                    ScanId = Guid.NewGuid(),
                    TagId = Guid.NewGuid(),
                    UserId = user.UserId,
                    ScanDirection = scanDirection,
                    Timestamp = DateTime.Now,
                    ResultCreditValue = user.Credits
                });

                db.SaveChanges();
                DisplayReport();
            }
        }

        private void DisplayReport()
        {
            ReportScansLabel.Text = String.Empty;
            ReportUsersLabel.Text = String.Empty;

            using (var db = new DatabaseContext(DbPath))
            {
                List<CashRegisterScan> scansSource = db.CashRegisterScans.ToList();
                List<User> usersSource = db.Users.ToList();

                for (int i = 0; i < scansSource.Count; i++)
                {
                    CashRegisterScan scan = scansSource[i];

                    ReportScansLabel.Text += $"{i}) {scan.TagId}     -     {scan.Timestamp.ToShortTimeString()}     -     {scan.ScanDirection} \r\n";
                }

                for (int i = 0; i < usersSource.Count; i++)
                {
                    User user = usersSource[i];
                    int scansCount = db.CashRegisterScans.Where(s => s.UserId == user.UserId).ToList().Count();

                    ReportUsersLabel.Text += $"{i}) {user.FirstName} {user.LastName}    -     Credits:{user.Credits}     -     Scans Count:{scansCount} \r\n";
                }
            }
        }

        private void TagScanned(string tagMessage)
        {
            using (var db = new DatabaseContext(DbPath))
            {
                User user = db.Users.Where(u => u.UserId == Guid.Parse("666b55ac-96e7-47b0-96d1-38622d0b176e")).FirstOrDefault();

                user.Scan(true);


                db.Add(new CashRegisterScan()
                {
                    ScanId = Guid.NewGuid(),
                    TagId = Guid.Parse(tagMessage),
                    UserId = user.UserId,
                    ScanDirection = true,
                    Timestamp = DateTime.Now,
                    ResultCreditValue = user.Credits
                });

                db.SaveChanges();
                DisplayReport();
            }
           if(CurrentSlide==Slides.Second) AddScannedItem();
        }
        #endregion
    }
}

//20450257662951 TNN
