using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NvtlApiWrapper;

namespace dotnet_attachdevice
{
    class Program
    {

        private static void Main(string[] args)
        {
            ApiWrapper api = new ApiWrapper();

            try
            {

            
            if (api.Init())
            {
                Console.WriteLine("Api initialized.");
                var devices = api.GetAvailableDevices();
                //api.DeviceDataReceived += ApiOnDeviceDataReceived;
                if (devices != null)
                {
                    Console.WriteLine("Found {0} devices", devices.Length);
                    var firstdevice = devices.FirstOrDefault();
                    
                    if (firstdevice != null)
                    {
                        Console.WriteLine("Device found: {0}", firstdevice.szFriendlyName);

                        var attachresult = api.AttachDevice(firstdevice);
                        Console.WriteLine("attachresult: {0} ", attachresult);
                        
                    }
                }
            }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }

           
                Console.WriteLine("Press enter key...");
                Console.ReadLine();
                //api.DeviceDataReceived -= ApiOnDeviceDataReceived;
                Console.WriteLine("IsOK: " + api.IsDeviceOK());
                Console.WriteLine("SignalStrenght: " + api.getSignalStrenght());
                Console.WriteLine("IsDeviceAttached: " + api.getIsDeviceAttached());
                Console.WriteLine("DeviceError: " + api.getDeviceError());

            try
            {
                api.DetachDevice();
                Console.ReadLine();
                Console.WriteLine("IsOK: " + api.IsDeviceOK());
                Console.WriteLine("SignalStrenght: " + api.getSignalStrenght());
                Console.WriteLine("IsDeviceAttached: " + api.getIsDeviceAttached());
                Console.WriteLine("DeviceError: " + api.getDeviceError());
            }
            finally
            {
                api.ReleaseSession();
            }

        }

        private static void ApiOnDeviceDataReceived(object userData, NvtlEventTypeManaged eventType, uint size, uint value)
        {
            switch (eventType)
            {
                case NvtlEventTypeManaged.NW_EVENT_DEVICE_ATTACHED:
                    Console.WriteLine("Event received: {0} - {1}", "NW_EVENT_DEVICE_ATTACHED", value);
                    break;
                    //if (eventType == NvtlEventTypeManaged.NW_EVENT_DEVICE_ATTACHED) 
                default:
                    Console.WriteLine("Event received: {0} - {1}", eventType.ToString(), value);
                    break;
            }
            
        }
    }
}
