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
        private ulong gDeviceState = 0;
        private int gDeviceAvailable = 0;

        static int Main(string[] args)
        {
             SdkWrapper	sdk = new SdkWrapper();
            NvtlEventCallback cb = new NvtlEventCallback();
            DeviceInfoStruct	device_info;
            NetworkInfoStruct	network_info;
            DeviceDetail[] device_list;
            ulong device_list_size = 5;
            int retries = 60;
            ushort rval = 0;

            //Check to see if SDK loaded okay
            if (!sdk.IsLoaded())
            {
                Console.WriteLine("SDK unavailable, aborting\n");
                return 0;
            }

            //create a session with the dll so we can access devices
            Console.WriteLine("Creating an SDK session\n");
            rval = sdk.CreateSession();

            //Setup an event callback handler to receive events
            Console.WriteLine("Registering SDK callback\n");
            cb.user_data = 0;
            cb.event_func = EventHandlerFunc;
            rval = sdk.RegisterEventCallback(cb);

            //See if there are any devices available
            device_list = new DeviceDetail[device_list_size];
            
            rval = sdk.GetDeviceInfo()

            printf("Found %ld devices\n", device_list_size);
            //if there are no devices then we need to wait for one to be inserted
            if (device_list_size == 0)
            {
                printf("Waiting 60 seconds for a device to be added!\n");
                while (gDeviceAvailable == 0 && --retries)
                {
                    my_sleep(1);
                }

                device_list_size = 5;
                memset(device_list, 0, device_list_size * sizeof(DeviceDetail));
                rval = sdk.GetAvailableDevices(device_list, &device_list_size);
            }

            //if still no devices were found then we should exit
            if (device_list_size == 0)
            {
                printf("No devices were found, aborting\n");
                sdk.ReleaseSession();
                return 0;
            }

            //now that we have a device here is the basic info.
            printf("A device is available!\n");
            printf("Port = %s\n", device_list[0].szPort);
            printf("Desc = %s\n", device_list[0].szDescription);
            printf("FriendlyName = %s\n", device_list[0].szFriendlyName);

            //Tell the sdk that we wish to communicate with this device
            rval = sdk.AttachDevice(&device_list[0]);

            if (rval)
            {
                printf("Failed to attach to device, %ld\n", rval);
            }
            else
            {
                //Check the state of the device to see if it is ready for general use
                rval = sdk.GetDeviceInfo(&device_info);
                if (LR_ERROR_SUCCESS == rval)
                {
                    //Pause here until the device is ready.  We are waiting for the 
                    //event callback to notify us of state changes.
                    // We are not considering cases where the device is Locked, Disabled, or unable to find service.
                    retries = 60;
                    while (gDeviceState < NW_DEVICE_STATE_IDLE && --retries)
                    {
                        my_sleep(1);
                    }

                    if (gDeviceState >= NW_DEVICE_STATE_IDLE)
                    {
                        rval = sdk.GetDeviceInfo(&device_info);
                        rval = sdk.GetNetworkInfo(&network_info);

                        printf("Device information\n");

                        printf("Device Model = %s\n", device_info.szDeviceModel);
                        printf("Device Number = %s\n", device_info.szMDN);

                        printf("Device service type = %ld\n", network_info.eService);
                        printf("Device roaming status = %ld\n", network_info.eRoam);
                        printf("Device dormancy status = %ld\n", network_info.bDormant);
                        printf("Device RSSI = %ld\n", network_info.dwRSSI);
                        printf("Device Signal Strength = %ld\n", network_info.dwSigStr);

                        if (device_info.eTech == DEV_UMTS || device_info.eTech == DEV_HSDPA)
                        {
                            printf("Device IMSI = %s\n", device_info.szIMSI);
                            printf("Device IMEI = %s\n", device_info.szIMEI);
                            printf("Device ICCID = %s\n", device_info.szICCID);
                            printf("Device SMSC = %s\n", device_info.szSMSC);
                        }
                        else
                        {
                            printf("Device ESN = %x\n", device_info.dwESN);
                            printf("Device MIN = %s\n", device_info.szMIN);
                        }
                    }
                }


                //Stop using this device
                printf("Detaching from device\n");
                sdk.DetachDevice();
            }
            //Notify the SDK that is is okay to clean up now
            printf("Detaching from SDK\n");
            sdk.ReleaseSession();
            return 0;
        }


        static void EventHandlerFunc(ValueType userData, NvtlEventType type, uint size, StandardEvent ev)
{
            switch (type)
            {
                case NvtlEventType.NW_EVENT_SIG_STR:
                    Console.WriteLine("Signal Strength Received = {0}\n", ev.val);
                    break;

                case NvtlEventType.NW_EVENT_ROAMING:
                    Console.WriteLine("Roaming Status Received = {0}\n", ev.val);
                    break;

                case NvtlEventType.NW_EVENT_SERVER_ERROR:
                    Console.WriteLine("EVENT SERVER ERROR = {0}\n", ev.val);
                    break;

                case NvtlEventType.NW_EVENT_DEVICE_STATE:
                {
                    Console.WriteLine("Device State Recevied ={0}\n", ev.val);
                    gDeviceState = ev.val;
                }
                    break;

                case NvtlEventType.NW_EVENT_NETWORK:
                    Console.WriteLine("EVENT NETWORK = {0}\n", ev.val);
                    break;

                case NvtlEventType.NW_EVENT_DEVICE_ADDED:
                {
                    Console.WriteLine("A new device was detected\n");
                    gDeviceAvailable = 1;
                }
                    break;

                case NvtlEventType.NW_EVENT_DEVICE_REMOVED:
                    Console.WriteLine("A device was removed\n");
                    break;
            }
}

    }
}
