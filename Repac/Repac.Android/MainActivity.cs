
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Repac.Rfid_Weird_stuff;
using System;
using System.Text;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;
using TechnologySolutions.Rfid.AsciiProtocol.Platform;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;
using Xamarin.Forms;

namespace Repac.Droid
{
    [Activity(Label = "Repac", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private NfcAdapter _nfcAdapter;

        private IAndroidLifecycle lifecyle;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        private IAndroidLifecycle TslLifecycle
        {
            get
            {
                if (this.lifecyle == null)
                {
                    var aaa = Locator.Default;
                    var bbb = Locator.Default.Locate<IAsciiTransportsManager>();
                    AsciiTransportsManager manager = Locator.Default.Locate<IAsciiTransportsManager>() as AsciiTransportsManager;

                    // AndrdoidLifecycleNone provides a no action IAndroidLifecycle instance to call in OnPause, OnResume so we don't keep
                    // attempting to resolve the AsciiTransportManager as the IAndroidLifecycle if it is not being used in this project
                    this.lifecyle = (IAndroidLifecycle)manager ?? new AndroidLifecycleNone();

                    // If the HostBarcodeHandler has been registered with the locator then it will be the Android type that needs IAndroidLifecycle calls
                    // Register the HostBarcodeHandler lifecycle with the AsciiTransportsManager
                    manager?.RegisterLifecycle(Locator.Default.Locate<IHostBarcodeHandler>() as HostBarcodeHandler);
                }

                return this.lifecyle;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            this.TslLifecycle.OnPause();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();

            this.TslLifecycle.OnResume(this);

            if (_nfcAdapter != null)
            {
                var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                var filters = new[] { tagDetected };
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent.Action == NfcAdapter.ActionTagDiscovered)
            {
                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
                if (tag != null)
                {
                    // First get all the NdefMessage
                    var rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
                    if (rawMessages != null)
                    {
                        var msg = (NdefMessage)rawMessages[0];

                        // Get NdefRecord which contains the actual data
                        var record = msg.GetRecords()[0];
                        if (record != null)
                        {
                            if (record.Tnf == NdefRecord.TnfWellKnown) // The data is defined by the Record Type Definition (RTD) specification available from http://members.nfc-forum.org/specs/spec_list/
                            {
                                // Get the transfered data
                                var data = Encoding.ASCII.GetString(record.GetPayload());
                                var aaa = data.Substring(3);
                                try
                                {

                                MessagingCenter.Send(data.Substring(3), "NewTagDataReceived");
                                }
                                catch(Exception e)
                                {

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}