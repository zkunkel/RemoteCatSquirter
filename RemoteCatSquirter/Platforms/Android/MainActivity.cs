using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Plugin.BLE;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace RemoteCatSquirter
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            

            //if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.BluetoothScan }, 0);
            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.BluetoothConnect }, 1);

            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.Bluetooth }, 102);
            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.BluetoothAdmin }, 102);

            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.AccessCoarseLocation }, 2);
            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.AccessFineLocation }, 3);

            CheckPermissions();
        }

        private async void CheckPermissions()
        {
            var status = await Permissions.RequestAsync<BluetoothPermissions>();
            if (status != PermissionStatus.Granted)
            {
                if (Permissions.ShouldShowRationale<BluetoothPermissions>())
                {
                    bool needPermission = await Shell.Current.DisplayAlert("needPermission ", "Requires permission to scan and connect to Bluetooth", "go", "cancel");
                }

                status = await Permissions.RequestAsync<BluetoothPermissions>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("needPermission", "The function is temporarily unavailable because the permission is not obtained", "ok");
                }
            }
        }
    }

    public class BluetoothPermissions : BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string permission, bool isRuntime)>
        {
            //(Android.Manifest.Permission.Bluetooth,true),
            //(Android.Manifest.Permission.BluetoothAdmin,true),
            //(Android.Manifest.Permission.BluetoothAdvertise,true),
            //(Android.Manifest.Permission.BluetoothPrivileged,true),
            (Android.Manifest.Permission.BluetoothConnect,true),
            (Android.Manifest.Permission.BluetoothScan,true),
            (Manifest.Permission.AccessCoarseLocation, true),
            (Manifest.Permission.AccessFineLocation, true)
        }.ToArray();
    }
}
