/*
Copyright (c) 2010 Ultraviolet Catastrophe

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished 
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace HidLibrary
{
    public class HidDevices
    {
        private static Guid _hidClassGuid = Guid.Empty;

        public static bool IsConnected(string devicePath)
        {
            return EnumerateHidDevices().Where(x => x == devicePath).Any();
        }

        public static HidDevice GetDevice(string devicePath)
        {
            return Enumerate(devicePath).FirstOrDefault();
        }

        public static IEnumerable<HidDevice> Enumerate()
        {
            return EnumerateHidDevices().Select(x => new HidDevice(x));
        }

        public static IEnumerable<HidDevice> Enumerate(string devicePath)
        {
            return EnumerateHidDevices().Where(x => x == devicePath).Select(x => new HidDevice(x));
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId, params int[] productIds)
        {
            return EnumerateHidDevices().Select(x => new HidDevice(x)).Where(x => x.Attributes.VendorId == vendorId && 
                                                                                  productIds.Contains(x.Attributes.ProductId));
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId)
        {
            return EnumerateHidDevices().Select(x => new HidDevice(x)).Where(x => x.Attributes.VendorId == vendorId);
        }

        private static IEnumerable<string> EnumerateHidDevices()
        {
            var hidClass = HidClassGuid;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidClass, null, 0, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            var devices = new List<string>();

            if (deviceInfoSet != (IntPtr) NativeMethods.INVALID_HANDLE_VALUE)
            {
                var deviceInfoData = CreateDeviceInfoData();
                var deviceIndex = 0;

                while (NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref deviceInfoData))
                {
                    deviceIndex += 1;

                    var deviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                    var deviceInterfaceIndex = 0;

                    while (NativeMethods.SetupDiEnumDeviceInterfaces(deviceInfoSet, 0, ref hidClass, deviceInterfaceIndex, ref deviceInterfaceData))
                    {
                        deviceInterfaceIndex++;
                        var devicePath = GetDevicePath(deviceInfoSet, deviceInterfaceData);
                        if (devices.Any(x => x == devicePath)) continue;
                        devices.Add(devicePath);
                    }
                }
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            return devices;
        }

        private static NativeMethods.SP_DEVINFO_DATA CreateDeviceInfoData()
        {
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();

            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            deviceInfoData.DevInst = 0;
            deviceInfoData.ClassGuid = Guid.Empty;
            deviceInfoData.Reserved = IntPtr.Zero;

            return deviceInfoData;
        }

        private static string GetDevicePath(IntPtr deviceInfoSet, NativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            var bufferSize = 0;
            var interfaceDetail = new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA { Size = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8 };

            NativeMethods.SetupDiGetDeviceInterfaceDetailBuffer(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, IntPtr.Zero);

            return NativeMethods.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ref interfaceDetail, bufferSize, ref bufferSize, IntPtr.Zero) ? 
                interfaceDetail.DevicePath : null;
        }

        private static Guid HidClassGuid
        {
            get
            {
                if (_hidClassGuid.Equals(Guid.Empty)) NativeMethods.HidD_GetHidGuid(ref _hidClassGuid);
                return _hidClassGuid;
            }
        }
    }
}
