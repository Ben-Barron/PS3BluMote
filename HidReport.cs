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

namespace HidLibrary
{
    public class HidReport
    {
        private byte _reportId;
        private byte[] _data = new byte[] {};

        private readonly HidDeviceData.ReadStatus _status;

        public HidReport(int reportSize)
        {
            Array.Resize(ref _data, reportSize);
        }

        public HidReport(int reportSize, HidDeviceData deviceData)
        {
            _status = deviceData.Status;

            Array.Resize(ref _data, reportSize - 1);

            if ((deviceData.Data != null))
            {

                if (deviceData.Data.Length > 0)
                {
                    _reportId = deviceData.Data[0];
                    Exists = true;

                    if (deviceData.Data.Length > 1)
                    {
                        var dataLength = reportSize - 1;
                        if (deviceData.Data.Length < reportSize - 1) dataLength = deviceData.Data.Length;
                        Array.Copy(deviceData.Data, 1, _data, 0, dataLength);
                    }
                }
                else Exists = false;
            }
            else Exists = false;
        }

        public bool Exists { get; private set; }
        public HidDeviceData.ReadStatus ReadStatus { get { return _status; } }

        public byte ReportId
        {
            get { return _reportId; }
            set
            {
                _reportId = value;
                Exists = true;
            }
        }

        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;
                Exists = true;
            }
        }

        public byte[] GetBytes()
        {
            byte[] data = null;
            Array.Resize(ref data, _data.Length + 1);
            data[0] = _reportId;
            Array.Copy(_data, 0, data, 1, _data.Length);
            return data;
        }
    }
}
