using Microsoft.AspNetCore.SignalR.Client;
using Repac.Data;
using Repac.Data.Models;
using Repac.Rfid_Weird_stuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TechnologySolutions.Rfid;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;
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

        private string dbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "RepacCashRegister1.db");

        private HubConnection hubConnection;

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

            hubConnection = new HubConnectionBuilder()
                 .WithUrl("https://repaccore.conveyor.cloud/chatHub")
                 .Build();

            hubConnection.StartAsync();

            hubConnection.On<string>("ReceiveMessage", (message) =>
           {
               ReceiveMessage(message);
           });

            this.ReadTagCommand = new RelayCommand(this.ExecuteReadTag, () => { return this.isIdle; });

            this.HexIdentifier = string.Empty;
            this.SelectedMemoryBank = TechnologySolutions.Rfid.MemoryBank.Epc; // 1;
            this.WordAddress = 2;
            this.WordCount = 2;
            //this.MinimumPower = 10;
            //this.OutputPower = this.MaximumPower = 30;

            this.IsIdle = true;

           // this.addNewEnumerator = this.transportsManager.Enumerators.Where(enumerator => enumerator.CanShowAddNew).FirstOrDefault();
            this.AddNewCommand = new RelayCommand(() => { this.addNewEnumerator?.ShowAddNew(); }, () => { return this.addNewEnumerator != null; });

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
            using (var db = new DatabaseContext(dbPath))
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

        private void ScanRfidButton1_Clicked(object sender, EventArgs e)
        {
            ICommand ReadTagCommand = new RelayCommand(this.ExecuteReadTag, () => { return this.isIdle; });
        }

        private void ScanRfidButton2_Clicked(object sender, EventArgs e)
        {
            ExecuteReadTag();
        }

        private void ConnectRfidButton_Clicked(object sender, EventArgs e)
        {

        }

        private void AddScan(bool scanDirection)
        {
            using (var db = new DatabaseContext(dbPath))
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

            using (var db = new DatabaseContext(dbPath))
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
            using (var db = new DatabaseContext(dbPath))
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
            if (CurrentSlide == Slides.Second) AddScannedItem();
        }

        private void SendMessageButton_Clicked(object sender, EventArgs e)
        {
            CashRegisterScan scan = new CashRegisterScan()
            {
                ScanId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TagId = Guid.NewGuid(),
                ScanDirection = true,
                ResultCreditValue = 1
            };
            ScanHappened(scan);
        }

        #endregion

        #region SignalR
        // https://localhost:44367/chatHub

        async Task ScanHappened(CashRegisterScan scan)
        {
            await hubConnection.InvokeAsync("ScanHappened", scan);
        }

        private void ReceiveMessage(string message)
        {
            int a = 0;
        }
        #endregion

        #region Rfid Read
        #region IsIdle
        /// <summary>
        /// Gets or sets a value indicating whether a long running task is not executing
        /// </summary>
        public bool IsIdle
        {
            get
            {
                return this.isIdle;
            }

            set
            {
                // this.Set(ref this.isIdle, value);
                this.isIdle = value;
            }
        }

        /// <summary>
        /// Backing store for IsBusy property
        /// </summary>
        private bool isIdle;
        #endregion
        #region HexIdentifier
        /// <summary>
        /// Gets or sets the new EPC filter
        /// </summary>
        public string HexIdentifier
        {
            get
            {
                return this.hexIdentifier;
            }

            set
            {
                //this.Set(ref this.hexIdentifier, value);
                hexIdentifier = value;
            }
        }

        /// <summary>
        /// Backing store for the HexIdentifier property
        /// </summary>
        private string hexIdentifier;
        #endregion
        #region MemoryBank
        /// <summary>
        /// Gets or sets the memoryBank
        /// </summary>
        public int MemoryBankIndex
        {
            get
            {
                return this.memoryBankIndex;
            }

            set
            {
                //if (this.Set(ref this.memoryBankIndex, value))
                //{
                //    this.RaisePropertyChanged("SelectedMemoryBank");
                //}
                memoryBankIndex = value;
            }
        }

        /// <summary>
        /// Backing store for the MemoryBank property
        /// </summary>
        private int memoryBankIndex;

        public MemoryBank SelectedMemoryBank
        {
            get
            {
                return (MemoryBank)this.MemoryBankIndex;
            }

            set
            {
                this.MemoryBankIndex = (int)value;
            }
        }

        /// <summary>
        /// Gets the names of the memory banks sorted by enumeration order
        /// </summary>
        public List<string> MemeoryBanks
        {
            get
            {
                return Enum.GetValues(typeof(MemoryBank))
                    .Cast<MemoryBank>()
                    .OrderBy(x => (int)x)
                    .Select(x => x.ToString())
                    .ToList();
            }
        }
        #endregion
        #region WordAddress
        /// <summary>
        /// Gets or sets the wordAddress
        /// </summary>
        public int WordAddress
        {
            get
            {
                return this.wordAddress;
            }

            set
            {
                // this.Set(ref this.wordAddress, (int)value);
                wordAddress = (int)value;
            }
        }

        /// <summary>
        /// Backing store for the WordAddress property
        /// </summary>
        private int wordAddress;
        #endregion
        #region WordCount
        /// <summary>
        /// Gets or sets the wordCount
        /// </summary>
        public int WordCount
        {
            get
            {
                return this.wordCount;
            }

            set
            {
                // this.Set(ref this.wordCount, (int)value);
                wordCount = (int)value;
            }
        }

        /// <summary>
        /// Backing store for the WordCount property
        /// </summary>
        private int wordCount;
        #endregion
        #region OutputPower
        /// <summary>
        /// Gets or sets the outputPower
        /// </summary>
        public int OutputPower
        {
            get
            {
                return this.outputPower;
            }

            set
            {
                // this.Set(ref this.outputPower, value);
                outputPower = value;
            }
        }

        /// <summary>
        /// Backing store for the OutputPower property
        /// </summary>
        private int outputPower;
        #endregion

        /// <summary>
        /// The instance used to read tag
        /// </summary>
        private readonly ITagReader tagReader;


        /// <summary>
        /// Gets the command to read the tag
        /// </summary>
        public ICommand ReadTagCommand { get; private set; }
        public ICommand WriteTagCommand { get; private set; }

        private async void ExecuteReadTag()
        {
            this.IsIdle = false;
            this.ReadTagCommand.RefreshCanExecute();
        //    this.WriteTagCommand.RefreshCanExecute();

            try
            {
                // Execute the potentially long running task off the UI thread
                int count = await this.tagReader.ReadTagsAsync(this.HexIdentifier, this.SelectedMemoryBank, this.WordAddress, this.WordCount, this.OutputPower);
            }
            catch (Exception ex)
            {
            }


            //await Task.Run(() =>
            //{
            //    try
            //    {
            //        if (this.tagReader.ReadTags(this.HexIdentifier.ToLower(), this.SelectedMemoryBank, this.WordAddress, this.WordCount, this.OutputPower))
            //        {
            //            this.AppendMessage("Done.");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        this.AppendMessage(ex.Message);
            //    }
            //});

            this.IsIdle = true;
            this.ReadTagCommand.RefreshCanExecute();
            //this.WriteTagCommand.RefreshCanExecute();
        }
        #endregion

        #region Rfid Connect
        public ICommand AddNewCommand { get; private set; }
        private IAsciiTransportEnumerator addNewEnumerator;

        #endregion
    }
}

//20450257662951 TNN
