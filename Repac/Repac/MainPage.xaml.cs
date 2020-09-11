using Microsoft.AspNetCore.SignalR.Client;
using Repac.Data.Models;
using Repac.Data.Models.DTOs;
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
        int ProductsCredit { get; set; } = 0;
        bool EditingItemsQuantityMode { get; set; } = false;

        ScanSession ScanSession { get; set; }
        List<ScanDTO> SessionScans { get; set; } = new List<ScanDTO>();
        UserDTO CurrentUser { get; set; }

        private string dbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "RepacCashRegister3.db");
        private readonly Guid userSashaGuid = Guid.Parse("666b55ac-96e7-47b0-96d1-38622d0b176e");
        private readonly Guid keyChainSashaGuid = Guid.Parse("b1056d6c-7f02-4f46-bdc6-e9feaff54a20");
        private readonly Guid scannerId = Guid.Parse("c75cc42d-1295-4cea-ba9e-c0b88bddfa49");


        private Guid tag1Guid = Guid.Parse("51a29bbf-d4d3-43c9-b290-d2445611b0d3");
        private Guid tag2Guid = Guid.Parse("b8a43e30-d24d-43f9-a697-7d6614d6c786");
        private Guid tag3Guid = Guid.Parse("c55cf3d1-9fd5-435d-b7cc-ea36a1c1bf3c");
        private Guid wrongTagGuid = Guid.Parse("8945418f-d9bb-43a1-b26a-ccaec30c2eec");

        private HubConnection hubConnection;

        private List<string> smallConsoleLog = new List<string>();

        enum Slides
        {
            First,
            Second,
            Third,
            Fourth,
            Admin
        }

        public MainPage()
        {
            InitializeComponent();
            FirstSlideActivate();

            List<ScanDTO> itemSource;

            //DisplayReport();

            MessagingCenter.Subscribe<string>(this, "NewTagDataReceived", (tag) => { NewTagDataReceived(tag); });

            hubConnection = new HubConnectionBuilder()
                 .WithUrl("https://repaccore.conveyor.cloud/signalrhub")
                 // .WithAutomaticReconnect()
                 .Build();

            hubConnection.StartAsync();

            hubConnection.On<UserDTO>("Authorized", (user) => { Authorized(user); });
            hubConnection.On<string>("NotAuthorized", (reason) => { NotAuthorized(reason); });

            hubConnection.On<string>("ReceiveMessage", (message) => { ReceiveMessage(message); });
            hubConnection.On<string, string>("TestEvent1", (arg1, arg2) => { TestEvent1(arg1, arg2); });

            hubConnection.On<ScanDTO>("ScanVerified", (scan) => { ScanVerified(scan); });
            hubConnection.On<Guid, string>("ScanNotVerified", (scanId, reason) => { ScanNotVerified(scanId, reason); });
            hubConnection.On<Guid>("SessionFinished", (scanSessionId) => { SessionFinished(scanSessionId); });

            hubConnection.On<int>("CreditsWereBought", (currentCredits) => { CreditsWereBought(currentCredits); });
            hubConnection.On<string>("CreditsWereNotBought", (reason) => { CreditsWereNotBought(reason); });

            hubConnection.InvokeAsync("TestEvent", "nothing");

            hubConnection.Reconnecting += async (error) =>
            {
                ReportArea1Label.Text += $"Hub Connection is Reconnecting \r\n";
                await Task.Delay(1000);
                await hubConnection.StartAsync();
            };
            hubConnection.Closed += async (error) =>
            {
                ReportArea1Label.Text += $"Hub Connection was Closed \r\n";
                await Task.Delay(1000);
                await hubConnection.StartAsync();
            };
            hubConnection.Reconnected += async (error) =>
            {
                ReportArea1Label.Text += $"Hub Connection was Reconnected \r\n";
                await Task.Delay(1000);
                await hubConnection.StartAsync();
            };

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

            ReportScannedTags();

        }

        #region "Events"
        private async void AddProductCredit(object sender, EventArgs e)
        {
            await Fade(CounterGridRight, 200);
            ProductsCredit += 1;
            CounterLabelRight.Text = this.ProductsCredit.ToString();
            await Appear(CounterGridRight, 200);

            if (SessionScans.Count <= ProductsCredit)
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
                    //ThirdSlideActivate();
                    break;
                case Slides.Third:
                    // FourthSlideActivate();
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
            if (false)//CurrentSlide == Slides.Second
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
            //AddScannedItem();
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
            AdminScreen.IsVisible = false;
        }
        private async void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;

            CounterLabel.Text = this.SessionScans.Count.ToString();

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
            AdminScreen.IsVisible = false;

            await Appear(ItemsCounterBackground, 300);
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            CounterLabelLeft.Text = this.SessionScans.Count.ToString();
            CounterLabelRight.Text = this.ProductsCredit.ToString();

            Footer.IsVisible = false;

            InviteIcon.IsVisible = false;
            UserInfo.IsVisible = true;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = true;
            FourthScreen.IsVisible = false;
            AdminScreen.IsVisible = false;

            FooterTextLabel.Text = "TAPER CARTE/TEL. POUR COMPLÉTER LA TRANSACTION.";
            FooterTextLabel.TextColor = BuyCreditsButton.BackgroundColor;
            Footer.IsVisible = SessionScans.Count <= ProductsCredit ? true : false;

            ThirdSlideUpdateLabels();
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
            AdminScreen.IsVisible = false;

            NullifyTimerSet();
        }

        private void AdminSlideActivate()
        {
            CurrentSlide = Slides.Admin;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
            AdminScreen.IsVisible = true;

            AdminSlideReport();
        }

        private void AdminSlideReport()
        {
            AdminItemsCounter.Text = $"Scans count:{SessionScans.Count}";
        }

        private void NullifyCycle()
        {
            SessionScans.Clear();
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

        private async void AddScannedItem(string tagMessage = null)
        {
            if (EditingItemsQuantityMode)
            {
                await ImageAdd.FadeTo(0, 100);
                await ImageAdd.FadeTo(1, 100);
            }
            await Fade(ItemsCounter, 200);
            // SessionScans.Count += 1;

            CounterLabel.Text = SessionScans.Count.ToString();
            CounterLabelLeft.Text = SessionScans.Count.ToString();
            await Appear(ItemsCounter, 200);

            if (SessionScans.Count > ProductsCredit)
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
            if (SessionScans.Count > 0)
            {
                if (EditingItemsQuantityMode)
                {
                    await ImageSubstract.FadeTo(0, 100);
                    await ImageSubstract.FadeTo(1, 100);
                }
                await Fade(ItemsCounter, 200);
                //  SessionScans.Count -= 1;
                CounterLabel.Text = this.SessionScans.Count.ToString();
                CounterLabelLeft.Text = this.SessionScans.Count.ToString();
                await Appear(ItemsCounter, 200);
                if (SessionScans.Count > ProductsCredit)
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

        #region Buttons
        private void ScanRfidButton2_Clicked(object sender, EventArgs e)
        {
            ExecuteReadTag();
            //ICommand ReadTagCommand = new RelayCommand(this.ExecuteReadTag, () => { return this.isIdle; });
        }
        private void TestButtonButton1_Clicked(object sender, EventArgs e)
        {
            hubConnection.InvokeAsync("TestEvent", Guid.NewGuid().ToString());
        }
        private async void TestButtonButton2_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debugger.Log(1, "test", "Successfully connected");
        }

        private void ScanButton_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(tag1Guid);
        }
        private void CancelScanButton_Clicked(object sender, EventArgs e)
        {
            if (SessionScans.Count > 0)
            {
                ScanDTO lastScan = SessionScans[SessionScans.Count - 1];

                SessionScans.Remove(lastScan);
                ReportScannedTags();

                if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                {
                    ReportArea1Label.Text += $"Last Scan (№{SessionScans.Count}) Deleted. Count:{SessionScans.Count} \r\n";
                    hubConnection.InvokeAsync("CancelScan", lastScan.ScanId);
                }
            }
        }
        private void CancelSessionButton_Clicked(object sender, EventArgs e)
        {

            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                ReportArea1Label.Text = $"Session Deleted. \r\n";
                hubConnection.InvokeAsync("CancelSession", ScanSession.ScanSessionId);
            }

            ScanSession = null;
            CurrentUser = null;
            SessionScans.Clear();
            ReportScannedTags();
        }
        private void BuyCreditButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentUser != null)
            {
                int amount = 1;

                RequestCreditsBuying(amount);
            }
            else
            {
                ReportArea1Label.Text += $"Credits can't be bought unless user is authorized \r\n";
            }
        }
        private void FinishSessionButton_Clicked(object sender, EventArgs e)
        {
            if (SessionScans.Count > 0)
            {
                if (CurrentUser != null)
                {
                    if (CurrentUser.RemainingCredits >= SessionScans.Count)
                    {
                        FinishSession();
                    }
                    else
                    {
                        ReportArea1Label.Text += $"Session Finish Denied. Not enough Credits: {CurrentUser.RemainingCredits} (need {SessionScans.Count})  \r\n";
                    }
                }
                else
                {
                    ReportArea1Label.Text += $"Session can't be finished unless user is authorized \r\n";
                }
            }
            else
            {
                ReportArea1Label.Text += $"Empty session can't be finished \r\n";
            }
        }
        private async void AuthorizeButton_Clicked(object sender, EventArgs e)
        {
            //   await hubConnection.StartAsync();
            NewScanOrAuthenticationHappend(keyChainSashaGuid);
        }
        private async void ReconnectButton_Clicked(object sender, EventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {
                ReportArea1Label.Text += $"Connection is already established \r\n";
            }
            else
            {
                while (true)
                {
                    if (hubConnection.State == HubConnectionState.Connecting)
                    {
                        ReportArea1Label.Text += $"Connecting.. \r\n";
                        await Task.Delay(2000);
                    }

                    try
                    {
                        await hubConnection.StartAsync();
                        System.Diagnostics.Debugger.Log(1, "test", "Successfully connected");
                        ReportArea1Label.Text += $"Successfully connected \r\n";
                        break;
                    }
                    catch
                    {
                        // Failed to connect, trying again in 5000 ms.
                        ReportArea1Label.Text += $"Failed to connect. Next attempt in 2 sec \r\n";
                        await Task.Delay(2000);
                    }
                }
            }

        }
        private void ScanTag1_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(tag2Guid);
        }
        private void ScanTag2_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(tag3Guid);
        }
        private void CancelTag1_Clicked(object sender, EventArgs e)
        {
            ScanDTO tag1Scan = SessionScans.Where(scan => scan.ContainerTagId == tag1Guid).FirstOrDefault();

            if (tag1Scan != null)
            {
                SessionScans.Remove(tag1Scan);
                ReportScannedTags();

                if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                {
                    ReportArea1Label.Text += $"Scan of Tag №1 was canceled. Count:{SessionScans.Count} \r\n";
                    hubConnection.InvokeAsync("CancelScan", tag1Scan.ScanId);
                }
            }
        }
        private void CancelTag2_Clicked(object sender, EventArgs e)
        {
            ScanDTO tag2Scan = SessionScans.Where(scan => scan.ContainerTagId == tag2Guid).FirstOrDefault();

            if (tag2Scan != null)
            {
                SessionScans.Remove(tag2Scan);
                ReportScannedTags();

                if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                {
                    ReportArea1Label.Text += $"Scan of Tag №2 was canceled. Count:{SessionScans.Count} \r\n";
                    hubConnection.InvokeAsync("CancelScan", tag2Scan.ScanId);
                }
            }
        }
        private void WrongTagButton_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(wrongTagGuid);
        }
        #endregion

        private void NewTagDataReceived(string tagMessage)
        {
            NewScanOrAuthenticationHappend(Guid.Parse(tagMessage));
        }

        private void CheckIfSessionExists()
        {
            if (ScanSession == null)
            {
                ScanSession = new ScanSession()
                {
                    ScanSessionId = Guid.NewGuid(),
                    ScanSessionStartTimestamp = DateTime.Now
                };

                ReportArea1Label.Text += $"New Session Started. Count:{SessionScans.Count} \r\n";
                SmallConsoleMessage($"New Session Started. \r\n");
            }
        }

        private void ReportScannedTags()
        {
            ReportArea2Label.Text = $" Scanned Tags Count: {SessionScans.Count}.       Ids: \r\n";

            for (int i = 0; i < SessionScans.Count; i++)
            {
                ScanDTO scan = SessionScans[i];

                if (scan.ContainerTagId == tag1Guid)
                {
                    ReportArea2Label.Text += $" {i}) {scan.ContainerTagId}      (Tag №1) \r\n";
                }
                else if (scan.ContainerTagId == tag2Guid)
                {
                    ReportArea2Label.Text += $" {i}) {scan.ContainerTagId}      (Tag №2) \r\n";
                }
                else
                {
                    ReportArea2Label.Text += $" {i}) {scan.ContainerTagId} \r\n";
                }
            }
        }

        private bool TagWasScanned(Guid scanId) => SessionScans.Where(scan => scan.ScanId == scanId || scan.ContainerTagId == scanId).Any();
        #endregion

        #region SignalR invoked events
        // https://localhost:44367/chatHub

        private void TestEvent1(string arg1, string arg2)
        {
            ReportArea1Label.Text += $"TestEvent1. arg1:{arg1} arg2:{arg2} \r\n";
        }
        private void ReceiveMessage(string message)
        {
            int a = 0;
        }
        private void Authorized(UserDTO user)
        {
            CurrentUser = user;

            if (CurrentSlide == Slides.First || CurrentSlide == Slides.Second || CurrentSlide == Slides.Fourth)
            {
                ReportArea1Label.Text += $"User {user.FirstName} {user.LastName} was authorized. Availible Credits:{CurrentUser.RemainingCredits} \r\n";
                SmallConsoleMessage($"User {user.FirstName} {user.LastName} was authorized. Av Cr:{CurrentUser.RemainingCredits} \r\n");
                ThirdSlideActivate();
            }
            else
            {
                if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                {
                    ReportArea1Label.Text = $"Session Finished.  \r\n";
                    SmallConsoleMessage($"Session Finished.  \r\n");
                    hubConnection.InvokeAsync("FinishSession", ScanSession);

                    NullifySession();
                    FourthSlideActivate();
                }
            }

        }
        private void NotAuthorized(string reason)
        {
            ReportArea1Label.Text += $"User hasn't been authorized: {reason} \r\n";
        }
        private void ScanVerified(ScanDTO verifiedScan)
        {
            if (CurrentSlide == Slides.First)
            {
                SecondSlideActivate();
            }

            SessionScans.Add(verifiedScan);
            ReportScannedTags();
            AddScannedItem();
            ThirdSlideUpdateLabels();

            ReportArea1Label.Text += $"Scan was verified. TagId:{verifiedScan.ContainerTagId}\r\n";
            SmallConsoleMessage($"Scan was verified. TagId:{verifiedScan.ContainerTagId} \r\n");
        }

        private void ScanNotVerified(Guid scanId, string reason)
        {
            ReportArea1Label.Text += $"Scan hasn't been verified: {reason} \r\n";

            //SessionScans.Remove(notVerifiedScan);
        }
        private void CreditsWereBought(int currentCredits)
        {
            CurrentUser.RemainingCredits = currentCredits;

            ReportArea1Label.Text += $"Credits were bought. Availible Credits:{CurrentUser.RemainingCredits} \r\n";
            SmallConsoleMessage($"Credits were bought. Availible Credits:{CurrentUser.RemainingCredits} \r\n");

            FinishSession();
        }
        private void CreditsWereNotBought(string reason)
        {
            ReportArea1Label.Text += $"Credits were not bought: {reason} \r\n";
        }

        private void SashaTest()
        {
            var a = 1;
        }
        #endregion

        private void NewScanOrAuthenticationHappend(Guid tagId)
        {
            if (tagId == tag3Guid)
            {
                //emulating admin card
                if (CurrentSlide == Slides.Admin)
                {
                    if (CurrentUser == null)
                    {
                        SecondSlideActivate();
                    }
                    else
                    {
                        ThirdSlideActivate();
                    }
                }
                else if(CurrentSlide == Slides.Second||CurrentSlide == Slides.Third)
                {
                    AdminSlideActivate();
                }
            }
            else if (CurrentSlide == Slides.Admin)
            {
                ScanDTO cancelingScan = SessionScans.Where(scan => scan.ContainerTagId == tagId).FirstOrDefault();

                if (cancelingScan != null)
                {
                    SessionScans.Remove(cancelingScan);
                    ReportScannedTags();
                    AdminSlideReport();

                    if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                    {
                        ReportArea1Label.Text += $"Scan of Tag №1 was canceled. Count:{SessionScans.Count} \r\n";
                        SmallConsoleMessage($"Scan was canceled. TagId: {tagId}\r\n");
                        hubConnection.InvokeAsync("CancelScan", cancelingScan.ScanId);
                    }
                }

            }
            else if (CurrentUser?.KeyChainId == tagId)
            {
                if (SessionScans.Count > 0)
                {
                    SubmitFinishOperation();
                }
            }
            else if (TagWasScanned(tagId))
            {
                ReportArea1Label.Text += $"Denied:This tag has already been scanned. \r\n";
                SmallConsoleMessage($"Denied:The tag has already been scanned. TagId {tagId}\r\n");
            }
            else
            {
                CheckIfSessionExists();

                TagScanDataDTO tagScan = new TagScanDataDTO()
                {
                    ScanId = Guid.NewGuid(),
                    TagId = tagId,
                    ScanSessionId = ScanSession.ScanSessionId,
                    ScannerId = scannerId,
                    Timestamp = DateTime.Now,
                    UserId = CurrentUser == null ? null : (Guid?)CurrentUser.UserId
                };

                //SessionScans.Add(scan);
                //ReportScannedTags();

                if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                {
                    ReportArea1Label.Text += $"Tag Scan Happened \r\n";
                    hubConnection.InvokeAsync("TagScanHappened", tagScan);
                }
            }
        }

        private void Authorize(Guid userId)
        {
            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                hubConnection.InvokeAsync("Authorization", userId);
            }
        }


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

        private void BuyCreditsButton_Clicked(object sender, EventArgs e)
        {
            hubConnection.InvokeAsync("BuyCredits", CurrentUser.UserId, ProductsCredit);

        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {

        }

        private void FinishSession()
        {
            ScanSession.ScanSessionEndTimestamp = DateTime.Now;
            ScanSession.UserId = CurrentUser.UserId;

            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                ReportArea1Label.Text = $"Session Finished.  \r\n";
                SmallConsoleMessage($"Session Finished.  \r\n");
                hubConnection.InvokeAsync("FinishSession", ScanSession.ScanSessionId, CurrentUser.UserId);

                NullifySession();
                FourthSlideActivate();
            }
        }

        private void NullifySession()
        {
            ScanSession = null;
            CurrentUser = null;
            SessionScans.Clear();
            ReportScannedTags();
        }

        private void SmallConsoleMessage(string text)
        {
            smallConsoleLog.Add(text);
            SmallConsole.Text = "";

            if (smallConsoleLog.Count > 2) SmallConsole.Text += $" {smallConsoleLog.Count - 3}) " + smallConsoleLog[smallConsoleLog.Count - 3];
            if (smallConsoleLog.Count > 1) SmallConsole.Text += $" {smallConsoleLog.Count - 2}) " + smallConsoleLog[smallConsoleLog.Count - 2];
            SmallConsole.Text += $" {smallConsoleLog.Count - 1}) " + smallConsoleLog[smallConsoleLog.Count - 1];
        }

        private void ThirdSlideUpdateLabels()
        {
            if (CurrentUser != null)
            {
                int creditsAvailible = CurrentUser.RemainingCredits;
                int creditsRequired = SessionScans.Count;

                FirstLabel.Text = $"Currently you have {creditsAvailible} availible credits.";

                if (creditsRequired == 0)
                {
                    SecondLabel.Text = $"Scan your containers.";
                    ThirdLabel.Text = $"";
                    FourthLabel.Text = $"";
                }
                else if (creditsAvailible < creditsRequired)
                {
                    SecondLabel.Text = $"To finish session you need {creditsRequired - creditsAvailible} more credits.";
                    ThirdLabel.Text = $"It will cost {creditsRequired - creditsAvailible}*4 = {creditsRequired * 4}$.";
                    FourthLabel.Text = $"Scan you Key Chain to submit the purchase and finish session.";
                }
                else
                {
                    SecondLabel.Text = $"To finish session it will take {creditsRequired} credits.";
                    ThirdLabel.Text = $"Scan you Key Chain to submit and finish session.";
                    FourthLabel.Text = $"";
                }
            }
        }

        private void SubmitFinishOperation()
        {
            int creditsAvailible = CurrentUser.RemainingCredits;
            int creditsRequired = SessionScans.Count;

            if (creditsAvailible < creditsRequired)
            {
                RequestCreditsBuying(creditsRequired - creditsAvailible);
            }
            else
            {
                FinishSession();
            }
        }

        private void RequestCreditsBuying(int amount)
        {
            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                ReportArea1Label.Text += $"{amount} Credits requested to buy.  \r\n";
                hubConnection.InvokeAsync("BuyCreditsAndFinish", CurrentUser.UserId, amount, ScanSession.ScanSessionId);
            }
        }

        private void SessionFinished(Guid scanSessionId)
        {
            ReportArea1Label.Text = $"Session Finished.  \r\n";
            SmallConsoleMessage($"Session Finished.  \r\n");

            NullifySession();
            FourthSlideActivate();
        }
    }
}
