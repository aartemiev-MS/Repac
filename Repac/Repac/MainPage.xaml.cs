﻿using Impinj.OctaneSdk;
using Microsoft.AspNetCore.SignalR.Client;
using Repac.Data.Models;
using Repac.Data.Models.DTOs;
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

    // #8dc73f green
    // #24a9e1 blue
    // #CC0000 red

    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        Slides CurrentSlide { get; set; } = Slides.First;
        int ProductsCredit { get; set; } = 0;
        int ExtraCreditsToBuy { get; set; } = 0;
        int ResultCretits { get; set; } = 0;

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

            ReportScannedTags();

            FirstSlideActivate();
        }

        #region "Events"

        private void FirstScreen_Tapped(object sender, EventArgs e)
        {
            // SecondSlideActivate();
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

            UserInfo.IsVisible = false;
            UserIcon.IsVisible = false;
            UserLine.IsVisible = false;
            Header.IsVisible = false;
            Footer.IsVisible = false;

            FirstScreen.IsVisible = true;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
            AdminScreen.IsVisible = false;
        }
        private async void SecondSlideActivate()
        {
            CurrentSlide = Slides.Second;


            UserInfo.IsVisible = false;
            UserIcon.IsVisible = false;
            UserLine.IsVisible = false;
            Header.IsVisible = true;
            Footer.IsVisible = false;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = false;
            AdminScreen.IsVisible = false;

            //await Appear(SecondScreenCounter, 300);
        }
        private void ThirdSlideActivate()
        {
            CurrentSlide = Slides.Third;

            ThirdSlideKey.TranslateTo(100, 100);

            UserInfo.IsVisible = true;
            UserIcon.IsVisible = true;
            UserLine.IsVisible = true;
            Header.IsVisible = true;
            Footer.IsVisible = false;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = true;
            FourthScreen.IsVisible = false;
            AdminScreen.IsVisible = false;

            ProfileReport1.IsVisible = true;
            ProfileReport2.IsVisible = true;
            ProfileReport3.IsVisible = true;

            ThirdSlideSetText();

            AddCreditsButton.IsVisible = true;
            ResetCreditsButton.IsVisible = false;
            AddCreditsOptionsButtons.IsVisible = false;
        }
        private void FourthSlideActivate()
        {
            CurrentSlide = Slides.Fourth;

            UserInfo.IsVisible = true;
            UserIcon.IsVisible = true;
            UserLine.IsVisible = true;

            FirstScreen.IsVisible = false;
            SecondScreen.IsVisible = false;
            ThirdScreen.IsVisible = false;
            FourthScreen.IsVisible = true;
            AdminScreen.IsVisible = false;

            ProfileReport1.IsVisible = true;
            ProfileReport2.IsVisible = false;
            ProfileReport3.IsVisible = true;

            ResultCretits = 0;

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
        }

        private void AdminSlideReport()
        {
            //AdminItemsCounter.Text = $"Scans count:{SessionScans.Count}";
        }

        private void NullifyCycle()
        {
            SessionScans.Clear();
            ScanSession = null;
            ProductsCredit = 0;

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

        private async void ScannedItemAnimation(string tagMessage = null)
        {
            // await Fade(SecondScreenCounter, 200);
            // SecondScreenCounter.Text = SessionScans.Count.ToString();
            // AdminCounterLabel.Text = SessionScans.Count.ToString();
            // await Appear(SecondScreenCounter, 200);
        }
        #endregion

        #region Test stuff

        #region Buttons
        private void ScanRfidButton2_Clicked(object sender, EventArgs e)
        {
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
                    if (CurrentUser.AvailibleCredits >= SessionScans.Count)
                    {
                        FinishSession();
                    }
                    else
                    {
                        ReportArea1Label.Text += $"Session Finish Denied. Not enough Credits: {CurrentUser.AvailibleCredits} (need {SessionScans.Count})  \r\n";
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

        private bool TagWasAlreadyScanned(Guid scanId) => SessionScans.Where(scan => scan.ScanId == scanId || scan.ContainerTagId == scanId).Any();
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
            if (SessionScans.Count > 0)
            {
                CurrentUser = user;

                if (CurrentSlide == Slides.First || CurrentSlide == Slides.Second || CurrentSlide == Slides.Fourth)
                {
                    ReportArea1Label.Text += $"User {user.FirstName} {user.LastName} was authorized. Availible Credits:{CurrentUser.AvailibleCredits} \r\n";
                    SmallConsoleMessage($"User {user.FirstName} {user.LastName} was authorized. Av Cr:{CurrentUser.AvailibleCredits} \r\n");
                    ThirdSlideActivate();
                }
                else
                {
                    if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                    {
                        ReportArea1Label.Text = $"Session Finished.  \r\n";
                        SmallConsoleMessage($"Session Finished.  \r\n");
                        hubConnection.InvokeAsync("FinishSession", ScanSession);

                        FourthSlideSetText();
                        NullifySession();
                        FourthSlideActivate();
                    }
                }

            }
            else
            {
                SmallConsoleMessage($"User {user.FirstName} {user.LastName} wasn't authorized. Because no scans happend yet \r\n");

            }

        }
        private void NotAuthorized(string reason)
        {
            ReportArea1Label.Text += $"User hasn't been authorized: {reason} \r\n";
        }
        private void ScanVerified(ScanDTO verifiedScan)
        {
            if (CurrentSlide == Slides.First || CurrentSlide == Slides.Fourth)
            {
                SecondSlideActivate();
            }

            SessionScans.Add(verifiedScan);
            ReportScannedTags();
            ScannedItemAnimation();

            SecondScreenCounter.Text = SessionScans.Count.ToString();
            AdminScreenCounter.Text = SessionScans.Count.ToString();
            ThirdScreenItemsCounter.Text = SessionScans.Count.ToString();

            ThirdSlideSetText();

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
           // CurrentUser.RemainingCredits = currentCredits;

            ReportArea1Label.Text += $"Credits were bought. Availible Credits:{CurrentUser.AvailibleCredits} \r\n";
            SmallConsoleMessage($"Credits were bought. Availible Credits:{CurrentUser.AvailibleCredits} \r\n");

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
                    if (SessionScans.Count == 0)
                    {
                        NullifyCycle();
                    }
                    else
                    {
                        SecondSlideActivate();
                    }
                }
                else if (CurrentSlide == Slides.Second && SessionScans.Count > 0)
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
                    SecondScreenCounter.Text = SessionScans.Count.ToString();
                    AdminScreenCounter.Text = SessionScans.Count.ToString();
                    // ScannedItemAnimation();

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
            else if (TagWasAlreadyScanned(tagId))
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

            // if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            //{
            ReportArea1Label.Text = $"Session Finished.  \r\n";
            SmallConsoleMessage($"Session Finished.  \r\n");
            hubConnection.InvokeAsync("FinishSession", ScanSession.ScanSessionId, CurrentUser.UserId);

            FourthSlideSetText();
            NullifySession();
            FourthSlideActivate();
            // }
        }

        private void NullifySession()
        {
            ResultCretits = SessionScans.Count - CurrentUser.AvailibleCredits + ExtraCreditsToBuy;

            ScanSession = null;
            CurrentUser = null;
            ExtraCreditsToBuy = 0;
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

        private void SubmitFinishOperation()
        {
            int creditsAvailible = CurrentUser.AvailibleCredits;
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

            FourthSlideSetText();
            NullifySession();
            FourthSlideActivate();
        }

        private void ButtonT1_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(tag1Guid);
            //TemporaryTestScan(tag1Guid);
        }

        private void ButtonT2_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(tag2Guid);
            //TemporaryTestScan(tag2Guid);
        }

        private void ButtonT_Clicked(object sender, EventArgs e)
        {
            TemporaryTestScan(Guid.NewGuid());
        }

        private void ButtonAT_Clicked(object sender, EventArgs e)
        {
            //ThirdSlideActivate();
            NewScanOrAuthenticationHappend(tag3Guid);
        }

        async private void ButtonSasha1_Clicked(object sender, EventArgs e)
        {
            Color aaa = SashaButton.BackgroundColor;
            SashaButton.BackgroundColor = Color.FromHex("#CC0000");

            ThirdSlideKey.TranslateTo(30, 5, 750, Easing.Linear);
            await ThirdSlideKey.ScaleTo(10, 750);
            await Task.Delay(200);

            await ThirdSlideKey.RotateTo(20, 100);
            await ThirdSlideKey.RotateTo(-20, 200);
            await ThirdSlideKey.RotateTo(20, 200);
            await ThirdSlideKey.RotateTo(-20, 200);
            await ThirdSlideKey.RotateTo(0, 100);

            await Task.Delay(1000);

            await ThirdSlideKey.FadeTo(0, 200);
            ThirdSlideKey.TranslateTo(100, 100, 0);
            ThirdSlideKey.ScaleTo(1, 0);
            await ThirdSlideKey.FadeTo(1, 0);
            SashaButton.BackgroundColor = aaa;
        }

        async private void ButtonSasha2_Clicked(object sender, EventArgs e)
        {
          //  CrossMediaManager.Current.PlayFromResource("Tinng_Conveyor_animation_v2.mp4");
        }

            private void ButtonKC_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(keyChainSashaGuid);

            //if (CurrentSlide == Slides.Third)
            //{
            //    FinishSession();
            //}
            //else
            //{
            //    Authorized(new UserDTO() { FirstName = "Denis", LastName = "Lopatin", RemainingCredits = 2 });
            //}
        }

            private void AddCreditsButton_Clicked(object sender, EventArgs e)
        {
            AddCreditsButton.IsVisible = false;
            ResetCreditsButton.IsVisible = false;
            AddCreditsOptionsButtons.IsVisible = true;
        }

        private void ResetCreditsButton_Clicked(object sender, EventArgs e)
        {
            AddCreditsButton.IsVisible = false;
            ResetCreditsButton.IsVisible = false;
            AddCreditsOptionsButtons.IsVisible = true;

            ExtraCreditsToBuy = 0;
            ThirdSlideSetText();
        }

        private void ButtonPlus1_Clicked(object sender, EventArgs e)
        {
            AddCreditsButton.IsVisible = false;
            ResetCreditsButton.IsVisible = true;
            AddCreditsOptionsButtons.IsVisible = false;

            ExtraCreditsToBuy = 1;
            ThirdSlideSetText();
        }

        private void ButtonPlus3_Clicked(object sender, EventArgs e)
        {
            AddCreditsButton.IsVisible = false;
            ResetCreditsButton.IsVisible = true;
            AddCreditsOptionsButtons.IsVisible = false;

            ExtraCreditsToBuy = 3;
            ThirdSlideSetText();
        }

        private void ButtonImpinj_Clicked(object sender, EventArgs e)
        {
            ImpinjStart();
        }

        private void TemporaryTestScan(Guid id)
        {
            NewScanOrAuthenticationHappend(id);
            ScanDTO newScan = new ScanDTO()
            {
                ScanId = Guid.NewGuid(),
                ContainerTagId = id,
            };

            if (CurrentSlide != Slides.Admin && TagWasAlreadyScanned(id) == false)
            {
                ScanVerified(newScan);
            }
            else if (CurrentSlide == Slides.Admin)
            {
                ScanDTO cancelingScan = SessionScans.Where(scan => scan.ContainerTagId == id).FirstOrDefault();

                SessionScans.Remove(cancelingScan);
                SecondScreenCounter.Text = SessionScans.Count.ToString();
                ThirdScreenItemsCounter.Text = SessionScans.Count.ToString();
                AdminScreenCounter.Text = SessionScans.Count.ToString();
            }
        }

        private void ThirdSlideSetText()
        {
            if (CurrentUser != null)
            {
                ThirdScreenPaymentCounter.Text = ScansMoreThanCredits() ? $"{(SessionScans.Count - CurrentUser.AvailibleCredits + ExtraCreditsToBuy) * 5}.00$" : "0.00$";
                CreditsToBuyLabel.Text = ScansMoreThanCredits() ? $"{SessionScans.Count - CurrentUser.AvailibleCredits + ExtraCreditsToBuy}" : "0";

                ProfileReport1.Text = $"Bonjour {CurrentUser.FirstName} {CurrentUser.LastName},";
                ProfileReport2.Text = ScansMoreThanCredits() ? $"{SessionScans.Count - CurrentUser.AvailibleCredits} crédits manquants" : $"{ CurrentUser.AvailibleCredits - SessionScans.Count} crédits disponible";
                ProfileReport3.Text = $"Crédits utilisés: {CurrentUser.UsedCredits}";
            }
        }

        private void FourthSlideSetText()
        {
            int creditsBefore = CurrentUser.AvailibleCredits;
            int creditsUsedInSession = SessionScans.Count;
            int creditsBought = SessionScans.Count - CurrentUser.AvailibleCredits + ExtraCreditsToBuy;
            int creditsAfter = creditsBefore - creditsUsedInSession + creditsBought;

            bool paymentRequired = ScansMoreThanCredits();
            PaymentSuccessLabel.IsVisible = paymentRequired ? true : false;

            FourthScreenCounterAvailible.Text = $"{creditsAfter}";
            FourthScreenCounterUsed.Text = $"{CurrentUser.UsedCredits + creditsUsedInSession}";
            ProfileReport3.Text = $"Crédits au compte: { CurrentUser.UsedCredits + creditsUsedInSession}";

        }

        private bool ScansMoreThanCredits() => SessionScans.Count - CurrentUser.AvailibleCredits >= 0;

        #region "Impinj Reader"
        const string READER_HOSTNAME = "192.168.1.21";  // NEED to set to your speedway!
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        static void ConnectToReader()
        {
            try
            {
                Console.WriteLine("Attempting to connect to {0} ({1}).",
                    reader.Name, READER_HOSTNAME);

                // The maximum number of connection attempts
                // before throwing an exception.
                //reader.MaxConnectionAttempts = 15;
                // Number of milliseconds before a 
                // connection attempt times out.
                reader.ConnectTimeout = 6000;
                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(READER_HOSTNAME);
                Console.WriteLine("Successfully connected.");

                // Tell the reader to send us any tag reports and 
                // events we missed while we were disconnected.
                reader.ResumeEventsAndReports();
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Failed to connect.");
                throw e;
            }
        }

        static void ImpinjStart()
        {
            try
            {
                // Assign a name to the reader. 
                // This will be used in tag reports. 
                reader.Name = "My Reader #1";

                // Connect to the reader.
                ConnectToReader();

                // Get the default settings.
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                Settings settings = reader.QueryDefaultSettings();

                // Start the reader as soon as it's configured.
                // This will allow it to run without a client connected.
                settings.AutoStart.Mode = AutoStartMode.Immediate;
                settings.AutoStop.Mode = AutoStopMode.None;

                // Use Advanced GPO to set GPO #1 
                // when an client (LLRP) connection is present.
                settings.Gpos.GetGpo(1).Mode = GpoMode.LLRPConnectionStatus;

                // Tell the reader to include the timestamp in all tag reports.
                settings.Report.IncludeFirstSeenTime = true;
                settings.Report.IncludeLastSeenTime = true;
                settings.Report.IncludeSeenCount = true;

                // If this application disconnects from the 
                // reader, hold all tag reports and events.
                settings.HoldReportsOnDisconnect = true;

                // Enable keepalives.
                settings.Keepalives.Enabled = true;
                settings.Keepalives.PeriodInMs = 5000;

                // Enable link monitor mode.
                // If our application fails to reply to
                // five consecutive keepalive messages,
                // the reader will close the network connection.
                settings.Keepalives.EnableLinkMonitorMode = true;
                settings.Keepalives.LinkDownThreshold = 5;

                // Assign an event handler that will be called
                // when keepalive messages are received.
                reader.KeepaliveReceived += OnKeepaliveReceived;

                // Assign an event handler that will be called
                // if the reader stops sending keepalives.
                reader.ConnectionLost += OnConnectionLost;

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Save the settings to the reader's 
                // non-volatile memory. This will
                // allow the settings to persist
                // through a power cycle.
                reader.SaveSettings();

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += OnTagsReported;

                // Wait for the user to press enter.
                //Console.WriteLine("Press enter to exit.");
                //Console.ReadLine();

                //// Stop reading.
                //reader.Stop();

                //// Disconnect from the reader.
                //reader.Disconnect();
            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }

        static void OnConnectionLost(ImpinjReader reader)
        {
            // This event handler is called if the reader  
            // stops sending keepalive messages (connection lost).
            Console.WriteLine("Connection lost : {0} ({1})", reader.Name, reader.Address);

            // Cleanup
            reader.Disconnect();

            // Try reconnecting to the reader
            ConnectToReader();
        }

        static void OnKeepaliveReceived(ImpinjReader reader)
        {
            // This event handler is called when a keepalive 
            // message is received from the reader.
            Console.WriteLine("Keepalive received from {0} ({1})", reader.Name, reader.Address);
        }

        static void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                Console.WriteLine("EPC : {0} Timestamp : {1}", tag.Epc, tag.LastSeenTime);
            }
        }

        #endregion
    }
}
