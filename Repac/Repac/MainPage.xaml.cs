using Impinj.OctaneSdk;
using Microsoft.AspNetCore.SignalR.Client;
using Repac.Data;
using Repac.Data.Models;
using Repac.Data.Models.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace Repac
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer

    // #8dc73f green logo
    // #D9EAD3 green light
    // #24a9e1 blue
    // #CC0000 red

    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private static MainPage Me;
        private HubConnection hubConnection;
        bool takeOutMode;

        Slides currentSlide;
        ScanSession scanSession;
        List<ScanDTO> sessionScans = new List<ScanDTO>();
        UserDTO currentUser;

        ThirdSlideButtonsMode currentButtonsMode = ThirdSlideButtonsMode.Add;
        int productsCredit = 0;
        int extraCreditsToBuy = 0;

        private string inputPhoneNumber = "";
        private string platsNumber = "";

        private bool numberSuggestion1Selected = false;
        private bool numberSuggestion2Selected = false;
        private bool phoneNumberEntryMode = true;

        private static List<TakeOutOrderDTO> ordersList = new List<TakeOutOrderDTO>();
        private static TakeOutOrderDTO currentOrder;
        private int orderScansCounter;


        private List<string> smallConsoleLog = new List<string>();
        private static Dictionary<string, DateTime> ScansTimingTracker = new Dictionary<string, DateTime>();

        enum Slides
        {
            First,
            SecondMain,
            SecondTakeOut,
            ThirdMain,
            ThirdTakeOut,
            FourthMain,
            FourthTakeOut,
            Admin
        }

        enum ThirdSlideButtonsMode
        {
            Add,
            Reset,
            Options
        }

        public MainPage(bool takeOutMode = false)
        {
            InitializeComponent();

            Me = this;
            this.takeOutMode = takeOutMode;

            MessagingCenter.Subscribe<string>(this, "NewTagDataReceivedNFC", (tag) => { NewTagDataReceived(tag); });

            SignalRHubCOnfiguration();
            ImpinjStart();

            ActivateSlide(1);
            StartKeyAnimation();

            if (Device.RuntimePlatform == Device.UWP)
            {
                WelcomeLogo.Source = ImageSource.FromFile("Assets/logo_tinng.png");
                HeaderLogo.Source = ImageSource.FromFile("Assets/logo_tinng.png");
                UserIcon.Source = ImageSource.FromFile("Assets/PhilippesIcon_transparent.png");
                SuccessLabel.Source = ImageSource.FromFile("Assets/Check_transparent.png");
                KeySecondSlide.Source = ImageSource.FromFile("Assets/key_black.png");
                KeyAdminSlide.Source = ImageSource.FromFile("Assets/key_black.png");
                ThirdSlideKey.Source = ImageSource.FromFile("Assets/key_black.png");
                ClickIcon.Source = ImageSource.FromFile("Assets/clickIcon.png");
                ConveyorTemp.Source = ImageSource.FromFile("Assets/conveyor.png");


                // ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(480, 800);
            }
        }

        private void SignalRHubCOnfiguration()
        {
            hubConnection = new HubConnectionBuilder()
                 .WithUrl("https://repaccore.conveyor.cloud/signalrhub")
                  .WithAutomaticReconnect()
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

            hubConnection.On<List<TakeOutLocationDTO>>("SuggestionClientsPulled", (dtoList) => { SuggestionClientsPulled(dtoList); });
            hubConnection.On("AdminKeyChainScanHappend", () => { AdminKeyChainScanHappend(); });

            hubConnection.InvokeAsync("TestEvent", "nothing");

            hubConnection.Reconnecting += async (error) =>
            {
                await Task.Delay(1000);
                await hubConnection.StartAsync();
            };
            hubConnection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                await hubConnection.StartAsync();
            };
            hubConnection.Reconnected += async (error) =>
            {
                await Task.Delay(1000);
                await hubConnection.StartAsync();
            };
        }

        #region "Events"

        private void FirstScreen_Tapped(object sender, EventArgs e)
        {
            // SecondSlideActivate();
        }

        private void FooterIcon_Tapped(object sender, EventArgs e)
        {
            switch (currentSlide)
            {
                case Slides.SecondMain:
                    //ThirdSlideActivate();
                    break;
                case Slides.ThirdMain:
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

        private void AddCreditsButton_Clicked(object sender, EventArgs e)
        {
            RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode.Options);
        }

        private void ResetCreditsButton_Clicked(object sender, EventArgs e)
        {
            RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode.Add);

            extraCreditsToBuy = 0;
            ThirdSlideMainSetText();
        }

        private void ButtonPlus1_Clicked(object sender, EventArgs e)
        {
            RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode.Reset);

            extraCreditsToBuy = 1;
            ThirdSlideMainSetText();
        }

        private void ButtonPlus3_Clicked(object sender, EventArgs e)
        {
            RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode.Reset);

            extraCreditsToBuy = 3;
            ThirdSlideMainSetText();
        }


        private void ButtonKey1_Clicked(object sender, EventArgs e) => NumberButtonClicked("1");
        private void ButtonKey2_Clicked(object sender, EventArgs e) => NumberButtonClicked("2");
        private void ButtonKey3_Clicked(object sender, EventArgs e) => NumberButtonClicked("3");
        private void ButtonKey4_Clicked(object sender, EventArgs e) => NumberButtonClicked("4");
        private void ButtonKey5_Clicked(object sender, EventArgs e) => NumberButtonClicked("5");
        private void ButtonKey6_Clicked(object sender, EventArgs e) => NumberButtonClicked("6");
        private void ButtonKey7_Clicked(object sender, EventArgs e) => NumberButtonClicked("7");
        private void ButtonKey8_Clicked(object sender, EventArgs e) => NumberButtonClicked("8");
        private void ButtonKey9_Clicked(object sender, EventArgs e) => NumberButtonClicked("9");
        private void ButtonKey0_Clicked(object sender, EventArgs e) => NumberButtonClicked("0");
        private void NumberButtonClicked(string number)
        {
            if (phoneNumberEntryMode)
                PhoneNumberAdd(number);
            else
                PlatsNumberAdd(number);
        }
        private void PhoneNumberAdd(string symbol)
        {
            if (inputPhoneNumber.Length < 10)
            {
                inputPhoneNumber += symbol;
                FilterSuggestedClients();
                PhoneNumberInputSet();
            }
        }
        private void PlatsNumberAdd(string number)
        {
            if (platsNumber.Length < 3)
            {
                platsNumber += number;
                PlatsNumberInputSet();

                SubmitOrderButtonSuccess.IsVisible = true;
            }
        }
        private void RemoveLastButtonClicked()
        {
            if (phoneNumberEntryMode)
            {
                inputPhoneNumber = inputPhoneNumber.Remove(inputPhoneNumber.Length - 1);
                FilterSuggestedClients();
                PhoneNumberInputSet();
            }
            else
            {
                if (platsNumber.Length > 0)
                {
                    platsNumber = platsNumber.Remove(platsNumber.Length - 1);
                    PlatsNumberInputSet();
                    SubmitOrderButtonSuccess.IsVisible = platsNumber.Length > 0 ? true : false;
                }
            }
        }

        private void ButtonKeyRemoveLast_Clicked(object sender, EventArgs e) => RemoveLastButtonClicked();
        private void ButtonKeyRemoveAll_Clicked(object sender, EventArgs e) => PhoneNumberRemoveAll();

        private void NumberSuggestion1_Clicked(object sender, EventArgs e)
        {
            numberSuggestion1Selected = !numberSuggestion1Selected;
            numberSuggestion2Selected = false;

            SetSuggestioButtonsColors();

            PhoneNumberRemoveAll();
            PhoneNumberAdd("514");

            if (!numberSuggestion1Selected)
                PhoneNumberRemoveAll();
        }
        private void NumberSuggestion2_Clicked(object sender, EventArgs e)
        {
            numberSuggestion1Selected = false;
            numberSuggestion2Selected = !numberSuggestion2Selected;

            SetSuggestioButtonsColors();

            PhoneNumberRemoveAll();
            PhoneNumberAdd("438");
            FilterSuggestedClients();

            if (!numberSuggestion2Selected)
                PhoneNumberRemoveAll();
        }

        private void SubmitOrderButtonSuccessClicked(object sender, EventArgs e)
        {
            var correspondintLocation = Constants.takeOutLocationsCache.Where(tol => tol.PhoneNumber == inputPhoneNumber).FirstOrDefault();

            TakeOutOrderDTO newOrder = new TakeOutOrderDTO()
            {
                Id = Guid.NewGuid(),
                ClerkId = currentUser.UserId,
                ScanSessionId = scanSession.ScanSessionId,
                Location = correspondintLocation,
                CreationDate = DateTime.Now,
                ContainersEstimated = Int32.Parse(platsNumber)
            };
            ordersList.Add(newOrder);

            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                hubConnection.InvokeAsync("SubmitOrder", newOrder);
            }
            ActivateSlide(2);
            NewOrderAddedScrollingAnimation();
        }
        private void FourthScreenTakeOutButton_Clicked(object sender, EventArgs e)
        {
            if (sessionScans.Count > 0)
            {
                orderScansCounter = 0;
                FourthScreenTakeOutCounter.Text = orderScansCounter.ToString();
                FourthScreenTakeOutButton.Text = "Continuer";
                ordersList.Remove(currentOrder);
                FinishSession();
                sessionScans.Clear();
                currentOrder = null;

                FourthSlideTakeOutButtonSet(true);
            }
        }

        private void SmallKeyboardClicked(object sender, EventArgs e)
        {
            EmptyOrdersList.IsVisible = false;

            ActivateSlide(3);
        }
        private void ButtonClient_Clicked(object sender, EventArgs e, string phoneNumber)
        {
            inputPhoneNumber = phoneNumber;
            PlatsEntryModeActivate();
        }
        #endregion

        #region "Functions"

        private void ActivateSlide(int slideIndex)
        {
            switch (slideIndex)
            {
                case 1:
                    FirstSlideActivate();
                    break;

                case 2:
                    if (takeOutMode)
                    {
                        SecondSlideTakeOutActivate();
                    }
                    else
                    {
                        SecondSlideMainActivate();
                    }
                    break;

                case 3:
                    if (takeOutMode)
                    {
                        ThirdSlideTakeOutActivate();
                    }
                    else
                    {
                        ThirdSlideMainActivate();
                    }
                    break;

                case 4:
                    if (takeOutMode)
                    {
                        FourthSlideTakeOutActivate();
                    }
                    else
                    {
                        FourthSlideMainActivate();
                    }
                    break;
                case 5:
                    AdminSlideActivate();
                    break;
            }
        }
        private void FirstSlideActivate()
        {
            currentSlide = Slides.First;
            RightScreenDisplay();
            RightHeaderDisplay();

            StartMessageLabel.IsVisible = takeOutMode ? true : false;
        }
        private void SecondSlideMainActivate()
        {
            currentSlide = Slides.SecondMain;
            RightScreenDisplay();
            RightHeaderDisplay();

            //await Appear(SecondScreenCounter, 300);
        }
        private void SecondSlideTakeOutActivate()
        {
            currentSlide = Slides.SecondTakeOut;
            RightScreenDisplay();
            RightHeaderDisplay();


            EmptyOrdersList.IsVisible = ordersList.Count > 0 ? false : true;
            OrdersListLayout.IsVisible = ordersList.Count > 0 ? true : false;

            SuggestedClientsList.Children.Clear();
            PhoneEntryModeActivate();
            GenerateOrdersList();

            TakeOutHeaderLabel.Text = $"Bonjour {currentUser.FirstName} {currentUser.LastName},\r\nVoici votre liste de commandes actives";
        }
        private void ThirdSlideMainActivate()
        {
            currentSlide = Slides.ThirdMain;
            RightScreenDisplay();
            RightHeaderDisplay();
            RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode.Add);

            ThirdSlideKey.TranslateTo(100, 100);
            ThirdSlideKey.ScaleTo(0.1);

            ThirdSlideMainSetText();
        }
        private void ThirdSlideTakeOutActivate()
        {
            currentSlide = Slides.ThirdTakeOut;
            RightScreenDisplay();
            RightHeaderDisplay();
            RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode.Add);

            PhoneNumberInput.Text = "";
            ContainersNumberInput.IsVisible = false;
            PlatsRequestLabel.IsVisible = false;
            PlatsNumberEntryModeLayout.IsVisible = false;
            SubmitOrderButtonSuccess.IsVisible = false;
            SuggestionButtonsArea.IsVisible = true;

            FilterSuggestedClients();
        }

        private void FourthSlideMainActivate()
        {
            currentSlide = Slides.FourthMain;
            RightScreenDisplay();
            RightHeaderDisplay();

            NullifyTimerSet();

        }
        private void FourthSlideTakeOutActivate()
        {
            currentSlide = Slides.FourthTakeOut;
            RightScreenDisplay();
            RightHeaderDisplay();

            TakeOutHeaderLabel.Text = $"Bonjour {currentUser.FirstName} {currentUser.LastName},\r\nVous traitez la commande de {currentOrder.Location.LocationName} | {currentOrder.ContainersEstimated}";
        }

        private void AdminSlideActivate()
        {
            currentSlide = Slides.Admin;
            RightScreenDisplay();
            RightHeaderDisplay();
        }

        private void RightScreenDisplay()
        {
            FirstScreen.IsVisible = currentSlide == Slides.First ? true : false;
            SecondScreenMain.IsVisible = currentSlide == Slides.SecondMain ? true : false;
            SecondScreenTakeOut.IsVisible = currentSlide == Slides.SecondTakeOut ? true : false;
            ThirdScreenMain.IsVisible = currentSlide == Slides.ThirdMain ? true : false;
            ThirdScreenTakeOut.IsVisible = currentSlide == Slides.ThirdTakeOut ? true : false;
            FourthScreenMain.IsVisible = currentSlide == Slides.FourthMain ? true : false;
            FourthScreenTakeOut.IsVisible = currentSlide == Slides.FourthTakeOut ? true : false;
            AdminScreen.IsVisible = currentSlide == Slides.Admin ? true : false;
        }
        private void RightHeaderDisplay()
        {
            if (currentSlide == Slides.First || currentSlide == Slides.ThirdTakeOut)
            {
                Header.IsVisible = false;
            }
            else
            {
                Header.IsVisible = true;

                if (currentSlide == Slides.SecondMain || currentSlide == Slides.SecondTakeOut || currentSlide == Slides.Admin || currentSlide == Slides.FourthTakeOut)
                {
                    UserInfo.IsVisible = false;
                    UserIcon.IsVisible = false;
                    UserLine.IsVisible = false;

                    TakeOutHeaderLabel.IsVisible = currentSlide == Slides.SecondTakeOut || currentSlide == Slides.FourthTakeOut ? true : false;
                }
                else
                {
                    UserInfo.IsVisible = true;
                    UserIcon.IsVisible = true;
                    UserLine.IsVisible = true;
                }
            }
        }
        private void RightThirdSlideButtonsDisplay(ThirdSlideButtonsMode mode)
        {
            currentButtonsMode = mode;

            AddCreditsButton.IsVisible = mode == ThirdSlideButtonsMode.Add ? true : false;
            ResetCreditsButton.IsVisible = mode == ThirdSlideButtonsMode.Reset ? true : false;
            AddCreditsOptionsButtons.IsVisible = mode == ThirdSlideButtonsMode.Options ? true : false;
        }

        private void AdminSlideReport()
        {
            //AdminItemsCounter.Text = $"Scans count:{SessionScans.Count}";
        }

        private void NullifyCycle()
        {
            sessionScans.Clear();

            scanSession = null;
            currentOrder = null;
            currentUser = takeOutMode ? currentUser : null;

            productsCredit = 0;

            inputPhoneNumber = "";
            platsNumber = "";

            ActivateSlide(takeOutMode ? 2 : 1);
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
                    if (currentSlide == Slides.FourthMain)
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

        async private void ButtonSasha1_Clicked(object sender, EventArgs e)
        {
            //if (SashaButton.BackgroundColor != Color.FromHex("#CC0000"))
            //{
            //    Color tempColor = SashaButton.BackgroundColor;
            //    SashaButton.BackgroundColor = Color.FromHex("#CC0000");

            //    ThirdSlideKey.TranslateTo(30, 5, 750, Easing.Linear);
            //    await ThirdSlideKey.ScaleTo(1, 750);
            //    await Task.Delay(200);

            //    await ThirdSlideKey.RotateTo(20, 100);
            //    await ThirdSlideKey.RotateTo(-20, 200);
            //    await ThirdSlideKey.RotateTo(20, 200);
            //    await ThirdSlideKey.RotateTo(-20, 200);
            //    await ThirdSlideKey.RotateTo(0, 100);

            //    await Task.Delay(1000);

            //    await ThirdSlideKey.FadeTo(0, 200);
            //    ThirdSlideKey.TranslateTo(100, 100, 0);
            //    ThirdSlideKey.ScaleTo(0.1, 0);
            //    await ThirdSlideKey.FadeTo(1, 0);

            //    //-----

            //    await ThirdSlideKey.TranslateTo(-50, -50, 500, Easing.Linear);
            //    await ClickIcon.ScaleTo(0.8, 500);
            //    await ClickIcon.ScaleTo(1, 500);
            //    await ThirdSlideKey.FadeTo(0, 200);
            //    await ThirdSlideKey.TranslateTo(0, 0, 0);

            //    SashaButton.BackgroundColor = tempColor;
            //}
        }
        #endregion

        #region Test stuff
        private void NewTagDataReceived(string tagMessage)
        {
            Guid TagGuid;
            if (!Guid.TryParse(tagMessage, out TagGuid)) TagGuid = Constants.containerTagsEPCLookup[tagMessage];
            bool anAdminTag = Constants.adminKeychains.Contains(TagGuid);

            if (Constants.useBackend)
            {
                if (anAdminTag && currentSlide == Slides.ThirdMain) Me.FinishSession();
                else if (anAdminTag && currentSlide == Slides.FourthTakeOut) ActivateSlide(5);
                else NewScanOrAuthenticationHappend(TagGuid);
            }
            else
            {
                if (Constants.adminKeychains.Contains(TagGuid) || Constants.containerTags.ContainsKey(TagGuid)) Me.TemporaryTestScan(TagGuid);

                if (Constants.userKeychains.ContainsKey(TagGuid))
                {
                    if (Me.currentSlide == Slides.ThirdMain) Me.FinishSession();
                    else Me.Authorized(Constants.userKeychains[TagGuid]);
                }

            }



        }

        private static void NewTagDataReceivedPhil(string tagMessage)
        {
            Guid TagGuid;
            if (!Guid.TryParse(tagMessage, out TagGuid)) TagGuid = Constants.containerTagsEPCLookup[tagMessage];

            if (Constants.adminKeychains.Contains(TagGuid) || Constants.containerTags.ContainsKey(TagGuid)) Me.TemporaryTestScan(TagGuid);

            if (Constants.userKeychains.ContainsKey(TagGuid))
            {
                if (Me.currentSlide == Slides.ThirdMain) Me.FinishSession();
                else Me.Authorized(Constants.userKeychains[TagGuid]);
            }

        }


        private static void TestAction()
        {
            // details.Text = OStateUserID.name + "\n" + OStateUserID.population;
        }


        private void CheckIfSessionExists()
        {
            if (scanSession == null)
            {
                scanSession = new ScanSession()
                {
                    ScanSessionId = Guid.NewGuid(),
                    ScanSessionStartTimestamp = DateTime.Now
                };

                SmallConsoleMessage($"New Session Started. \r\n");
            }
        }

        private bool TagWasAlreadyScanned(Guid scanId) => sessionScans.Where(scan => scan.ScanId == scanId || scan.ContainerTagId == scanId).Any();
        #endregion

        #region SignalR invoked events
        // https://localhost:44367/chatHub

        private void TestEvent1(string arg1, string arg2)
        {
        }
        private void ReceiveMessage(string message)
        {
            int a = 0;
        }
        private void Authorized(UserDTO user)
        {
            if (takeOutMode)
            {
                currentUser = user;
                ActivateSlide(2);
            }
            else
            {
                if (sessionScans.Count > 0)
                {
                    currentUser = user;

                    if (currentSlide == Slides.First || currentSlide == Slides.SecondMain || currentSlide == Slides.FourthMain)
                    {
                        SmallConsoleMessage($"User {user.FirstName} {user.LastName} was authorized. Av Cr:{currentUser.AvailibleCredits} \r\n");
                        ActivateSlide(3);
                    }
                    else
                    {
                        if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                        {
                            SmallConsoleMessage($"Session Finished.  \r\n");
                            hubConnection.InvokeAsync("FinishSession", scanSession);

                            FourthSlideSetText();
                            NullifySession();
                            ActivateSlide(4);
                        }
                    }

                }

            }
        }
        private void NotAuthorized(string reason)
        {
        }
        private void ScanVerified(ScanDTO verifiedScan)
        {
            if (takeOutMode)
            {
                CheckIfSessionExists();
                sessionScans.Add(verifiedScan);
                SetScanCountersText();
                FourthSlideTakeOutButtonSet();
            }
            else
            {
                if (currentSlide == Slides.First || currentSlide == Slides.FourthMain)
                {
                    ActivateSlide(2);
                }

                sessionScans.Add(verifiedScan);
                ScannedItemAnimation();

                SetScanCountersText();
                ThirdSlideMainSetText();

                SmallConsoleMessage($"Scan was verified. TagId:{verifiedScan.ContainerTagId} \r\n");
            }
        }

        private void SetScanCountersText()
        {

            SecondScreenCounter.Text = sessionScans.Count.ToString();
            AdminScreenCounter.Text = sessionScans.Count.ToString();
            ThirdScreenItemsCounter.Text = sessionScans.Count.ToString();
            FourthScreenTakeOutCounter.Text = sessionScans.Count.ToString();

        }

        private void ScanNotVerified(Guid scanId, string reason)
        {

            //SessionScans.Remove(notVerifiedScan);
        }
        private void CreditsWereBought(int currentCredits)
        {
            // CurrentUser.RemainingCredits = currentCredits;

            SmallConsoleMessage($"Credits were bought. Availible Credits:{currentUser.AvailibleCredits} \r\n");

            FinishSession();
        }
        private void CreditsWereNotBought(string reason)
        {
        }

        private void SashaTest()
        {
            var a = 1;
        }

        private void SuggestionClientsPulled(List<TakeOutLocationDTO> dtoList)
        {
            Constants.takeOutLocationsCache = dtoList;
            CreateClientButtonsList();
        }
        #endregion

        private void NewScanOrAuthenticationHappend(Guid tagId)
        {
            if (!Constants.useBackend && Constants.adminKeychains.Contains(tagId))
            {
                AdminKeyChainScanHappend();//emulating admin card
            }
            else if (currentSlide == Slides.Admin)
            {
                CancelScan(tagId);

            }
            else if (currentUser?.KeyChainId == tagId)
            {
                if (takeOutMode)
                {
                    //this is equal an admin keychain scan logic
                    AdminKeyChainScanHappend();
                }
                else
                {
                    if (sessionScans.Count > 0)
                    {
                        SubmitFinishOperation();
                    }

                }
            }
            else if (TagWasAlreadyScanned(tagId))
            {
                SmallConsoleMessage($"Denied:The tag has already been scanned. TagId {tagId}\r\n");
            }
            else
            {
                CheckIfSessionExists();
                SendTagScanToBackEnd(tagId);
            }
        }

        private void CancelScan(Guid tagId)
        {
            ScanDTO cancelingScan = sessionScans.Where(scan => scan.ContainerTagId == tagId).FirstOrDefault();

            if (takeOutMode && currentUser?.KeyChainId == tagId) AdminKeyChainScanHappend();

            if (cancelingScan != null)
            {
                sessionScans.Remove(cancelingScan);
                SetScanCountersText();

                if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
                {
                    hubConnection.InvokeAsync("CancelScan", cancelingScan.ScanId);
                }
            }
        }

        private void AdminKeyChainScanHappend()
        {
            if (currentSlide == Slides.Admin)
            {
                FourthSlideTakeOutButtonSet();
                if (sessionScans.Count == 0)
                {
                    NullifyCycle();
                }
                else
                {
                    ActivateSlide(takeOutMode ? 4 : 2);
                }
            }
            else if (currentSlide == Slides.SecondMain && sessionScans.Count > 0)
            {
                ActivateSlide(5);
            }
        }

        private void SendTagScanToBackEnd(Guid tagId)
        {
            TagScanDataDTO tagScan = new TagScanDataDTO()
            {
                ScanId = Guid.NewGuid(),
                TagId = tagId,
                ScanSessionId = scanSession.ScanSessionId,
                ScannerId = Constants.scannerId,
                Timestamp = DateTime.Now,
                UserId = currentUser == null ? null : (Guid?)currentUser.UserId
            };

            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                hubConnection.InvokeAsync("TagScanHappened", tagScan);
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
            hubConnection.InvokeAsync("BuyCredits", currentUser.UserId, productsCredit);

        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {

        }

        private void FinishSession()
        {
            scanSession.ScanSessionEndTimestamp = DateTime.Now;
            scanSession.UserId = currentUser.UserId;

            if (hubConnection.State == HubConnectionState.Connected || hubConnection.State == HubConnectionState.Connecting)
            {
                Guid? attachedOrderId = currentOrder == null ? null : (Guid?)currentOrder.Id;
                hubConnection.InvokeAsync("FinishSession", scanSession.ScanSessionId, currentUser.UserId, attachedOrderId);

                if (takeOutMode)
                {
                    ActivateSlide(2);
                }
                else
                {
                    FourthSlideSetText();
                    NullifySession();
                    ActivateSlide(4);
                }
            }
        }

        private void NullifySession()
        {

            scanSession = null;
            currentUser = null;
            extraCreditsToBuy = 0;
            sessionScans.Clear();
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
            int creditsAvailible = currentUser.AvailibleCredits;
            int creditsRequired = sessionScans.Count;

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
                hubConnection.InvokeAsync("BuyCreditsAndFinish", currentUser.UserId, amount, scanSession.ScanSessionId);
            }
        }

        private void SessionFinished(Guid scanSessionId)
        {
            SmallConsoleMessage($"Session Finished.  \r\n");

            FourthSlideSetText();
            NullifySession();
            ActivateSlide(4);
        }

        private void TemporaryTestScan(Guid id)
        {
            NewScanOrAuthenticationHappend(id);
            ScanDTO newScan = new ScanDTO()
            {
                ScanId = Guid.NewGuid(),
                ContainerTagId = id,
            };

            if (currentSlide != Slides.Admin && !TagWasAlreadyScanned(id) && !Constants.adminKeychains.Contains(id))
            {
                ScanVerified(newScan);
            }
            else if (currentSlide == Slides.Admin)
            {
                ScanDTO cancelingScan = sessionScans.Where(scan => scan.ContainerTagId == id).FirstOrDefault();

                sessionScans.Remove(cancelingScan);
                SetScanCountersText();
            }
        }

        private void ThirdSlideMainSetText()
        {
            if (currentUser != null)
            {
                ThirdScreenPaymentCounter.Text = ScansMoreThanCredits() ? $"{(sessionScans.Count - currentUser.AvailibleCredits + extraCreditsToBuy) * 5}" : "0";
                CreditsToBuyLabel.Text = ScansMoreThanCredits() ? $"{sessionScans.Count - currentUser.AvailibleCredits + extraCreditsToBuy}" : "0";

                ProfileReport1.Text = $"Bonjour {currentUser.FirstName} {currentUser.LastName},";
                ProfileReport2.Text = ScansMoreThanCredits() ? $"{sessionScans.Count - currentUser.AvailibleCredits} crédits manquants" : $"{ currentUser.AvailibleCredits - sessionScans.Count} crédits disponible";
                ProfileReport3.Text = $"Crédits utilisés: {currentUser.UsedCredits}";
            }
        }

        private void FourthSlideSetText()
        {
            int creditsBefore = currentUser.AvailibleCredits;
            int creditsUsedInSession = sessionScans.Count;
            int creditsBought = sessionScans.Count - currentUser.AvailibleCredits + extraCreditsToBuy;
            int creditsAfter = creditsBefore - creditsUsedInSession + creditsBought;

            bool paymentRequired = ScansMoreThanCredits();
            PaymentSuccessLabel.IsVisible = paymentRequired ? true : false;

            FourthScreenCounterAvailible.Text = $"{creditsAfter}";
            FourthScreenCounterUsed.Text = $"{currentUser.UsedCredits + creditsUsedInSession}";
            ProfileReport3.Text = $"Crédits au compte: { currentUser.UsedCredits + creditsUsedInSession}";

        }

        private bool ScansMoreThanCredits() => sessionScans.Count - currentUser.AvailibleCredits >= 0;

        #region "Impinj Reader"
        const string READER_HOSTNAME = "10.28.74.112";  // NEED to set to your speedway!
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();
        static AntennaConfig antennaConfig;
        static Impinj.OctaneSdk.Settings readerSettings;

        static void ConnectToReader()
        {
            try
            {
                Console.WriteLine("Attempting to connect to {0} ({1}).",
                    reader.Name, READER_HOSTNAME);

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

                // The maximum number of connection attempts
                // before throwing an exception.
                //reader.MaxConnectionAttempts = 15;
                // Number of milliseconds before a
                // connection attempt times out.
                reader.ConnectTimeout = 6000;
                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs
                // to the IP address or hostname of your reader.

                // Put Read power Tx to 20 Dbm.
                foreach (AntennaConfig antenna in settings.Antennas) { antenna.TxPowerInDbm = 20; }

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
        static async void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                Console.WriteLine("EPC : {0} Timestamp : {1}", tag.Epc, tag.LastSeenTime);
                ReportTags(tag);
            }


        }

        static async void ReportTags(Tag tag)
        {
            string tagId = tag.Epc.ToString();

            //way 1
            if (TagWasRecentlyScanned(tagId)) { return; }
            Device.BeginInvokeOnMainThread(delegate () { Me.NewTagDataReceived(tagId); });

            string test = tag.LastSeenTime.ToString();

            //way 2
            //DateTime lastScanTime = DateTime.Parse(tag.LastSeenTime.ToString());
            //if (DateTime.Now - lastScanTime > scanDelay)
            //{
            //    ManageTagReportStatus(tagId);
            //    Device.BeginInvokeOnMainThread(delegate () { NewTagDataReceivedPhil(tagId); });
            //}

        }

        private static bool TagWasRecentlyScanned(string tagId)
        {
            if (ThisTagWasAlreadyScanned(tagId))
            {
                return DateTime.Now - CurrentTagLastScanDate(tagId) < Constants.scanDelay ? true : false;
            }
            else
            {
                return false;
            }
        }

        private static void ManageScansTimingTracker(string tagId)
        {
            if (ThisTagWasAlreadyScanned(tagId))
            {
                ScansTimingTracker.Remove(tagId);
            }

            ScansTimingTracker.Add(tagId, DateTime.Now);
        }
        private static bool ThisTagWasAlreadyScanned(string tagId) => ScansTimingTracker.Where(scan => scan.Key == tagId).Any();
        private static DateTime CurrentTagLastScanDate(string tagId) => ScansTimingTracker.Where(scan => scan.Key == tagId).FirstOrDefault().Value;


        private static string lastScanTagId;
        private static void NewTagDataReceivedPhilBridge()
        {
            NewTagDataReceivedPhil(lastScanTagId);
            lastScanTagId = null;
        }

        private void ButtonSlide2_Clicked(object sender, EventArgs e)
        {
            NewScanOrAuthenticationHappend(Constants.adminKeychains.FirstOrDefault());
        }


        #endregion

        private void ButtonJennifer_Clicked(object sender, EventArgs e)
        {
            OrdersListLayout.IsVisible = false;
            SmallKeyboard.IsVisible = false;
        }

        private void ButtonArt_Clicked(object sender, EventArgs e)
        {
            EmptyOrdersList.IsVisible = false;
            OrdersListLayout.IsVisible = true;
        }

        #region "Keyboard"

        private TakeOutLocationDTO GetSelectedLocation() => Constants.takeOutLocationsCache.Where(tol => tol.PhoneNumber == inputPhoneNumber).FirstOrDefault();

        private void PlatsEntryModeActivate()
        {
            SuggestedClientsList.Children.Clear();

            ContainersNumberInput.Text = "Plats: ...";
            UserCreditsCounterFourthSlide.Text = GetSelectedLocation().AvailibleCredits.ToString();
            // UserCreditsCounterFourthSlide.IsVisible = false; //sasha test
            ContainersNumberInput.BackgroundColor = Constants.lightGreen;

            ContainersNumberInput.IsVisible = true;
            ContainersNumberInputFrame.IsVisible = true;
            ContainersNumberIcon.IsVisible = true;
            PlatsNumberEntryModeLayout.IsVisible = true;
            PlatsRequestLabel.IsVisible = true;
            SuggestionButtonsArea.IsVisible = false;
            phoneNumberEntryMode = false;

            PhoneNumberInput.BackgroundColor = Color.White;

            PhoneNumberInputSet();

            Button suggestionButton = CreateClientButtonElement(GetSelectedLocation().LocationName, inputPhoneNumber, Constants.lightGreen);
            suggestionButton.Clicked += (s, e) =>
            {
                PhoneEntryModeActivate();
            };

            SuggestedClientsList.Children.Add(suggestionButton);
        }
        private void PhoneEntryModeActivate()
        {
            phoneNumberEntryMode = true;
            numberSuggestion1Selected = false;
            numberSuggestion2Selected = false;
            PhoneNumberInputSet();
            platsNumber = "";
            inputPhoneNumber = "";

            PhoneNumberInput.BackgroundColor = Constants.lightGreen;
            ContainersNumberInput.BackgroundColor = Color.White;
            NumberSuggestion1Button.BackgroundColor = Color.White;
            NumberSuggestion2Button.BackgroundColor = Color.White;

            ContainersNumberInputFrame.IsVisible = false;
            ContainersNumberIcon.IsVisible = false;
            PlatsNumberEntryModeLayout.IsVisible = false;
            PlatsRequestLabel.IsVisible = false;
            SuggestionButtonsArea.IsVisible = true;
            SubmitOrderButtonSuccess.IsVisible = false;

            PhoneNumberInputSet();
            FilterSuggestedClients();
        }




        private void PhoneNumberRemoveAll()
        {
            inputPhoneNumber = "";
            FilterSuggestedClients();
            PhoneNumberInputSet();
        }

        private void PhoneNumberInputSet()
        {
            string formattedNumber = GetFormattedPhoneNumber(inputPhoneNumber);

            if (inputPhoneNumber.Length < 10) formattedNumber += " ...";

            PhoneNumberInput.Text = formattedNumber;
        }
        private void PlatsNumberInputSet()
        {
            ContainersNumberInput.Text = "Plats: " + (platsNumber.Length > 0 ? platsNumber : "...");
        }

        private string GetFormattedPhoneNumber(string rawNumber)
        {
            string formattedNumber = rawNumber;

            if (rawNumber.Length > 3) formattedNumber = formattedNumber.Insert(3, " ");
            if (rawNumber.Length > 6) formattedNumber = formattedNumber.Insert(7, " ");

            return formattedNumber;
        }

        private void FilterSuggestedClients()
        {
            if (Constants.useBackend)
            {
                PullSuggestionClients(inputPhoneNumber);
            }
            else
            {
                CreateClientButtonsList();
            }
        }

        private Button CreateClientButtonElement(string clientName, string clientNumber, Color color)
        {
            return new Button()
            {
                BackgroundColor = color,
                BorderWidth = 1,
                BorderColor = Color.Black,
                WidthRequest = 450,
                HeightRequest = 100,
                CornerRadius = 20,
                Text = $"{clientName}\r\n{GetFormattedPhoneNumber(clientNumber)}",
                FontSize = 20,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center
            };
        }

        private void CreateClientButtonsList()
        {
            SuggestedClientsList.Children.Clear();
            int displayedSuggestionsCounter = 0;

            foreach (TakeOutLocationDTO location in Constants.takeOutLocationsCache)
            {
                if (location.PhoneNumber.StartsWith(inputPhoneNumber) && displayedSuggestionsCounter < 5)
                {
                    Button suggestionButton = CreateClientButtonElement(location.ResponsiblePersonName, location.PhoneNumber, Color.White);
                    suggestionButton.Clicked += (s, e) =>
                    {
                        ButtonClient_Clicked(s, e, location.PhoneNumber);
                    };

                    SuggestedClientsList.Children.Add(suggestionButton);

                    displayedSuggestionsCounter += 1;
                }
            }

        }

        private void SetSuggestioButtonsColors()
        {
            NumberSuggestion1Button.BackgroundColor = numberSuggestion1Selected ? Constants.lightGreen : Color.White;
            NumberSuggestion2Button.BackgroundColor = numberSuggestion2Selected ? Constants.lightGreen : Color.White;
        }
        #endregion



        private void GenerateOrdersList()
        {
            OrdersListLayout.Children.Clear();

            foreach (TakeOutOrderDTO order in ordersList)
            {
                Button anOrder = new Button()
                {
                    BackgroundColor = Color.White,
                    BorderWidth = 1,
                    BorderColor = Color.Black,
                    WidthRequest = 700,
                    HeightRequest = 100,
                    CornerRadius = 20,
                    Text = order.Location.ResponsiblePersonName + " " + order.CreationDate.ToShortTimeString(),
                    FontSize = 40,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Center
                };
                anOrder.Clicked += (s, e) =>
                {
                    currentOrder = order;
                    FourthSlideTakeOutActivate();
                };

                OrdersListLayout.Children.Add(anOrder);
            }
        }


        private void PullSuggestionClients(string phoneNumber)
        {
            hubConnection.InvokeAsync("PullSuggestionClients", phoneNumber);
        }

        private void FourthSlideTakeOutButtonSet(bool forceDefault = false)
        {
            FourthScreenTakeOutButton.BackgroundColor = forceDefault || sessionScans?.Count < currentOrder?.ContainersEstimated ? Color.Orange : Constants.lightGreen;
            FourthScreenTakeOutButton.Text = forceDefault || sessionScans?.Count < currentOrder?.ContainersEstimated ? "Continuer" : "Valider";
        }

        private async void ButtonScroll_Clicked(object sender, EventArgs e)
        {
            NewOrderAddedScrollingAnimation();
        }

        private async Task NewOrderAddedScrollingAnimation()
        {
            var lastChild = OrdersListLayout.Children.LastOrDefault();
            var firstChild = OrdersListLayout.Children.FirstOrDefault();

            if (lastChild != null && firstChild != null)
            {
                await OrdersScroll.ScrollToAsync(lastChild, ScrollToPosition.MakeVisible, true);
                lastChild.BackgroundColor = Constants.lightGreen;
                await Task.Delay(1000);

                lastChild.BackgroundColor = Color.White;
                lastChild.WidthRequest = 700;
                await OrdersScroll.ScrollToAsync(firstChild, ScrollToPosition.MakeVisible, true);
            }
        }

        private async void StartKeyAnimation()
        {
            while (true)
            {
                await KeyFirstSlide.RotateTo(20, 100);
                await KeyFirstSlide.RotateTo(-20, 200);
                await KeyFirstSlide.RotateTo(20, 200);
                await KeyFirstSlide.RotateTo(-20, 200);
                await KeyFirstSlide.RotateTo(0, 100);

                await Task.Delay(3500);
            }
        }

        private void CancelAllScansButtonClicked(object sender, EventArgs e)
        {
            for (int i = sessionScans.Count - 1; i >= 0; i--) CancelScan(sessionScans[i].ContainerTagId);
        }
    }
}
