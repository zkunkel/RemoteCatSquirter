using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Exceptions;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Maui.Core;
using System.Threading.Tasks;
using System.Threading;

namespace RemoteCatSquirter
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        IBluetoothLE ble = CrossBluetoothLE.Current;
        IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        ObservableCollection<IDevice> deviceList = new ObservableCollection<IDevice>();
        //set up services and characteristics
        IReadOnlyList<IService> services;
        IService service;
        IReadOnlyList<ICharacteristic> characteristics;
        ICharacteristic characteristic;
        ICharacteristic characteristicTX;
        ICharacteristic characteristicRX;

        IDevice device = null;

        public MainPage()
        {
            InitializeComponent();
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}


        // SNACKBAR AND TOAST

        async private void SearchBluetooth(object sender, EventArgs e)
        {
            if (adapter.IsScanning)
                return;

            adapter.ScanMode = ScanMode.Balanced;
            adapter.ScanTimeout = 1000;

            deviceList.Clear();
            adapter.DeviceDisconnected += (s, e) =>
            {
                //btcon.deviceList.Remove(e.Device);
                Debug.WriteLine("REMOVE list: " + e.Device);
            };

            adapter.DeviceDiscovered += (s, e) =>
            {
                //  Debug.WriteLine(e.Device);
                if (e.Device.Name != "")
                    deviceList.Add(e.Device);
                Debug.WriteLine("Device list: " + e.Device);
                //Debug.WriteLine("Device NAME: " + e.Device.Name);
            };

            adapter.DeviceAdvertised += (s, e) =>
            {
                Debug.WriteLine("Device advertised: " + e.Device);
            };

            //add already known devices
            //Debug.WriteLine("number of already connected devices: " + adapter.GetSystemConnectedOrPairedDevices().Count());
            /*foreach (IDevice newDev in adapter.GetSystemConnectedOrPairedDevices())
            {
                Debug.WriteLine("NEWDEVICE " + newDev);
                deviceList.Add(newDev);
            }
            foreach (IDevice newDev in adapter.ConnectedDevices)
            {
                Debug.WriteLine("NEWDEVICE " + newDev);
                deviceList.Add(newDev);
            }*/

            //scan for devices
            if (!adapter.IsScanning)
                await adapter.StartScanningForDevicesAsync();

            foreach (var device in deviceList)
                Debug.WriteLine("Device list: " + device);


            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            IToast toast;

            // connect to device if it exists
            device = deviceList.Where(x => x.Name == "ESP32 hh").FirstOrDefault();
            if (device != null)
            {
                btnConnect.IsEnabled = true;
                toast = Toast.Make("esp32 found, connecting . . . ", ToastDuration.Long, 30);
                await toast.Show(cancellationTokenSource.Token);
                if (await ConnectToDevice(device))
                {
                    btnConnect.IsEnabled = false;
                    btnBTscan.IsEnabled = false;
                    btnSQUIRT.IsEnabled = true;
                    toast = Toast.Make("connected!!!!!", ToastDuration.Long, 30);
                    await toast.Show(cancellationTokenSource.Token);
                }
                else
                {
                    toast = Toast.Make("could not connect to device . . .", ToastDuration.Long, 30);
                    await toast.Show(cancellationTokenSource.Token);
                }
            }
            else
            {
                toast = Toast.Make("esp32 not found . . .", ToastDuration.Long, 30);
                await toast.Show(cancellationTokenSource.Token);
            }
        }

        //connect to device faster
        async void ConnectToESP32(object sender, EventArgs args)
        {
            await ConnectToDevice(device);
        }

        async Task<bool> ConnectToDevice(IDevice device)
        {
            try
            {
                await adapter.StopScanningForDevicesAsync();
                ConnectParameters connectParameters = new ConnectParameters(true, true);

                //await adapter.ConnectToKnownDeviceAsync(new Guid("5a562bcb-418b-46a0-a42c-575f8cb6cb3e"));  // rest in peep hiletgo esp32 00000000-0000-0000-0000-58bf25177936
                var devicee = deviceList.Where(x => x.Name == "ESP32 hh").FirstOrDefault();
                await adapter.ConnectToDeviceAsync(devicee, connectParameters);

                //set up services and characteristics
                services = await adapter.ConnectedDevices[0].GetServicesAsync();
                service = await adapter.ConnectedDevices[0].GetServiceAsync(services[2].Id);
                //IService servi = await btcon.btDev.GetServiceAsync(device.Id);

                //characteristics = await services[0].GetCharacteristicsAsync();
                characteristics = await service.GetCharacteristicsAsync();

                //characteristic = characteristics[0];
                characteristic = await service.GetCharacteristicAsync(characteristics[0].Id); //Guid.Parse("guidd")  btcon.btDev.Id

                characteristicTX = characteristics[1];
                characteristicRX = characteristics[0];
                return true;
            } catch (Exception ex) { return false; }
        }

        void SQUIRTCOMMENCE(object sender, EventArgs args)
        {
            UpdateServo(0, "SQUIRT");
        }



        bool sendTimeOut = false;
        async void UpdateServo(int position, string servo)
        {
            sendTimeOut = true;

            byte[] data = Encoding.ASCII.GetBytes(servo + position.ToString());

            //Encoding.Default.GetBytes
            //byte[] data = BitConverter.GetBytes(position);

            try
            {
                await characteristicTX.WriteAsync(data);
                Debug.WriteLine("Sent: " + position);
            }
            catch (CharacteristicReadException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            await Task.Delay(15);
            sendTimeOut = false;
        }






        //constantly read input
        async void ReadData(object sender, EventArgs args)
        {
            (byte[] test, int resultCode) = await characteristics[0].ReadAsync();
            byte[] prevTest = test;
            //Debug.WriteLine("Received: " + Encoding.Default.GetString(test));

            characteristicRX.ValueUpdated += (s, e) =>
            {
                Debug.WriteLine("Received: " + Encoding.Default.GetString(e.Characteristic.Value));
            };
            await characteristicRX.StartUpdatesAsync();

            /*while (true)
            {
                try
                {
                    test = await btcon.characteristics[0].ReadAsync();
                    if (test != prevTest)
                    {
                        Debug.WriteLine("Received: " + Encoding.Default.GetString(test));
                    }
                    await Task.Delay(1000);
                }
                catch (CharacteristicReadException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }*/
        }

        //sends over test data
        async void SendData(object sender, EventArgs args)
        {
            //byte[] data = { 0x01 };
            byte[] data = Encoding.ASCII.GetBytes("OINK");

            try
            {
                await characteristicTX.WriteAsync(data);
                //await chara.WriteAsync(data);

                Debug.WriteLine("Sent: " + "OINK");
            }
            catch (CharacteristicReadException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            while (true)
            {
                try
                {
                    await characteristicTX.WriteAsync(data);
                    //await chara.WriteAsync(data);

                    Debug.WriteLine("Sent: " + "OINK");
                }
                catch (CharacteristicReadException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                await Task.Delay(4100);
            }

        }
    }
}
