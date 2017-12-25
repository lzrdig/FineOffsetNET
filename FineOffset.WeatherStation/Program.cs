using System;
using System.Diagnostics;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using FineOffsetLib.WSdataStructs;
using FineOffsetLib.HIDmgr;

namespace FineOffset.WeatherStation
{
    class Program {
        public static UsbDevice MyUsbDevice;

        public static int USB_TYPE_CLASS = (0x01 << 5);
        public static int USB_RECIP_INTERFACE = 0x01;

        public static int HISTORY_MAX = 4080;
        public static int HISTORY_CHUNK_SIZE = 16;
        public static int WEATHER_SETTINGS_CHUNK_SIZE = 256;
        public static int HISTORY_START = WEATHER_SETTINGS_CHUNK_SIZE;
        public static int HISTORY_END =(HISTORY_START + (HISTORY_MAX * HISTORY_CHUNK_SIZE));

        #region SET YOUR USB Vendor and Product ID!
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x1941, 0x8021);
        #endregion

        static void Main(string[] args) {
            ErrorCode ec = ErrorCode.None;

            try {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // Cheeck if the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                FOsettings ws = new FOsettings();

                MyUsbDevMgr myDevMGr = new MyUsbDevMgr(wholeUsbDevice, ws);

                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    ec = myDevMGr.InitDevInterface();

                    if(ec == ErrorCode.None) {
                        Debug.WriteLine("Read the block with the device settings.\n");
                        ws = myDevMGr.WSettings;

                        int items_to_read = 10;

                        Debug.WriteLine("Start reading history blocks\n");
                        myDevMGr.GetWeatherData(items_to_read);

                        myDevMGr.Print_summary(ws, myDevMGr.History[HISTORY_MAX - 1]);

                        Debug.WriteLine("Index\tTimestamp\t\tDelay\n");
                        Console.WriteLine("Index\tTimestamp\t\tDelay\n");
                        for (int j=0;j< items_to_read;j++)
                            myDevMGr.Print_history_item(myDevMGr.History[HISTORY_MAX - 1 - j]);

                        myDevMGr.Print_status(ws);

                        myDevMGr.Print_settings(ws);

                        //myDevMGr.SetIndoorTmrptUnits();
                        myDevMGr.SetTmrptLowLevel();
                    }
                }
                

                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally {
                if (MyUsbDevice != null) {
                    if (MyUsbDevice.IsOpen) {
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null)) {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;                    

                    // Free usb resources
                    UsbDevice.Exit();
                }

                // Wait for user input..
                Console.ReadKey();
            }
        }
    }
}
