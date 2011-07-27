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
using System.Threading;

namespace HidLibrary
{
    internal class HidDeviceEventMonitor
    {
        public event InsertedEventHandler Inserted;
        public event RemovedEventHandler Removed;

        public delegate void InsertedEventHandler();
        public delegate void RemovedEventHandler();

        private readonly HidDevice _device;
        private bool _wasConnected;

        public HidDeviceEventMonitor(HidDevice device)
        {
            _device = device;
        }

        public void Init()
        {
            var eventMonitor = new Action(DeviceEventMonitor);
            eventMonitor.BeginInvoke(DisposeDeviceEventMonitor, eventMonitor);
        }

        private void DeviceEventMonitor()
        {
            var isConnected = _device.IsConnected;

            if (isConnected != _wasConnected)
            {
                if (isConnected && Inserted != null) Inserted();
                else if (!isConnected && Removed != null) Removed();
                _wasConnected = isConnected;
            }

            Thread.Sleep(500);

            if (_device.MonitorDeviceEvents) Init();
        }

        private static void DisposeDeviceEventMonitor(IAsyncResult ar)
        {
            ((Action)ar.AsyncState).EndInvoke(ar);
        }
    }
}
