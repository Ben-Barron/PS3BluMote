/*
Copyright (c) 2011 Ben Barron

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
using System.Timers;

using HidLibrary;
using WindowsAPI;

namespace PS3BluMote
{
    class PS3Remote
    {
        public event EventHandler<EventArgs> BatteryLifeChanged;
        public event EventHandler<ButtonData> ButtonDown;
        public event EventHandler<ButtonData> ButtonReleased;
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;

        private HidDevice hidRemote = null;
        private Timer timerFindRemote = null;
        private Timer timerHibernation = null;
        private int vendorId = 0x054c;
        private int productId = 0x0306;
        private Button lastButton = Button.Angle;
        private bool _hibernationEnabled;

        private byte _batteryLife = 100;

        #region "Remote button codes"
        static byte[][] buttonCodes = 
        {
	        new byte[] { 0, 0, 0, 22 },
	        new byte[] { 0, 0, 0, 100 },
	        new byte[] { 0, 0, 0, 101 },
	        new byte[] { 0, 0, 0, 99 },
	        new byte[] { 0, 0, 0, 15 },
	        new byte[] { 0, 0, 0, 40 },
	        new byte[] { 0, 0, 0, 0 },
	        new byte[] { 0, 0, 0, 1 },
	        new byte[] { 0, 0, 0, 2 },
	        new byte[] { 0, 0, 0, 3 },
	        new byte[] { 0, 0, 0, 4 },
	        new byte[] { 0, 0, 0, 5 },
	        new byte[] { 0, 0, 0, 6 },
	        new byte[] { 0, 0, 0, 7 },
	        new byte[] { 0, 0, 0, 8 },
	        new byte[] { 0, 0, 0, 9 },
	        new byte[] { 0, 0, 0, 128 },
	        new byte[] { 0, 0, 0, 129 },
	        new byte[] { 0, 0, 0, 130 },
	        new byte[] { 0, 0, 0, 131 },
	        new byte[] { 0, 0, 0, 112 },
	        new byte[] { 0, 0, 0, 26 },
	        new byte[] { 0, 0, 0, 64 },
	        new byte[] { 0, 0, 0, 14 },
	        new byte[] { 0, 16, 0, 92 },
	        new byte[] { 0, 32, 0, 93 },
	        new byte[] { 0, 128, 0, 95 },
	        new byte[] { 0, 64, 0, 94 },
	        new byte[] { 16, 0, 0, 84 },
	        new byte[] { 64, 0, 0, 86 },
	        new byte[] { 128, 0, 0, 87 },
	        new byte[] { 32, 0, 0, 85 },
	        new byte[] { 0, 0, 8, 11 },
	        new byte[] { 0, 4, 0, 90 },
	        new byte[] { 0, 1, 0, 88 },
	        new byte[] { 2, 0, 0, 81 },
	        new byte[] { 0, 8, 0, 91 },
	        new byte[] { 0, 2, 0, 89 },
	        new byte[] { 4, 0, 0, 82 },
	        new byte[] { 0, 0, 1, 67 },
	        new byte[] { 1, 0, 0, 80 },
	        new byte[] { 8, 0, 0, 83 },
	        new byte[] { 0, 0, 0, 50 },
	        new byte[] { 0, 0, 0, 56 },
	        new byte[] { 0, 0, 0, 57 },
	        new byte[] { 0, 0, 0, 51 },
	        new byte[] { 0, 0, 0, 52 },
	        new byte[] { 0, 0, 0, 48 },
	        new byte[] { 0, 0, 0, 49 },
	        new byte[] { 0, 0, 0, 96 },
	        new byte[] { 0, 0, 0, 97 },
        };
        #endregion

        public PS3Remote(int vendor, int product, bool hibernation)
        {
            vendorId = vendor;
            productId = product;
            _hibernationEnabled = hibernation;

            timerHibernation = new Timer();
            timerHibernation.Interval = 60000;
            timerHibernation.Elapsed += new ElapsedEventHandler(timerHibernation_Elapsed);

            timerFindRemote = new Timer();
            timerFindRemote.Interval = 1500;
            timerFindRemote.Elapsed += new ElapsedEventHandler(timerFindRemote_Elapsed);
        }

        public void connect()
        {
            timerFindRemote.Enabled = true;
        }

        public byte getBatteryLife
        {
            get { return _batteryLife; }
        }

        public bool isConnected
        {
            get { return timerFindRemote.Enabled; }
        }

        public bool hibernationEnabled
        {
            get { return _hibernationEnabled; }
            set { _hibernationEnabled = value; }
        }

        private void readButtonData(HidDeviceData InData)
        {
            timerHibernation.Enabled = false;

            if (InData.Status == HidDeviceData.ReadStatus.Success)
            {
                if (DebugLog.isLogging) DebugLog.write("Read button data");

                byte[] bCode = { InData.Data[1], InData.Data[2], InData.Data[3], InData.Data[4] };

                int i, j;

                for (j = 0; j < 51; j++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        if (bCode[i] != buttonCodes[j][i]) break;
                    }

                    if (i == 4) break;
                }

                if (j != 51)
                {
                    lastButton = (Button)j;

                    if (ButtonDown != null) ButtonDown(this, new ButtonData(lastButton));
                }
                else
                {
                    if (ButtonReleased != null) ButtonReleased(this, new ButtonData(lastButton));
                }

                byte batteryReading = (byte)(InData.Data[11] * 20);

                if (batteryReading != _batteryLife) //Check battery life reading.
                {
                    _batteryLife = batteryReading;

                    if (BatteryLifeChanged != null) BatteryLifeChanged(this, new EventArgs());
                }

                if (_hibernationEnabled) timerHibernation.Enabled = true;

                hidRemote.Read(readButtonData); //Read next button pressed.
            }
            else
            {
                if (Disconnected != null) Disconnected(this, new EventArgs());

                hidRemote.Dispose(); //Dispose of current remote.

                hidRemote = null;

                timerFindRemote.Enabled = true; //Try to reconnect.
            }
        }

        private void timerFindRemote_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (hidRemote == null)
            {
                if (DebugLog.isLogging) DebugLog.write("Searching for remote");

                IEnumerator<HidDevice> devices = HidDevices.Enumerate(vendorId, productId).GetEnumerator();
                
                if (devices.MoveNext()) hidRemote = devices.Current;

                if (hidRemote != null)
                {
                    if (DebugLog.isLogging) DebugLog.write("Remote found");

                    hidRemote.OpenDevice();

                    if (Connected != null)
                    {
                        Connected(this, new EventArgs());
                    }

                    hidRemote.Read(readButtonData);

                    timerFindRemote.Enabled = false;
                }
            }
            else
            {
                timerFindRemote.Enabled = false;
            }
        }

        private void timerHibernation_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DebugLog.isLogging) DebugLog.write("Attempting to hibernate remote");

            try
            {
                HardwareAPI.DisableDevice(n => n.ToUpperInvariant().Contains
                    ("_VID&0002054c_PID&0304"), true);

                HardwareAPI.DisableDevice(n => n.ToUpperInvariant().Contains
                    ("_VID&0002054c_PID&0304"), false);
            }
            catch
            {
                if (DebugLog.isLogging) DebugLog.write("Unable to hibernate remote");
            }

            timerFindRemote.Enabled = true;
            timerHibernation.Enabled = false;
        }

        public class ButtonData : EventArgs
        {
            public Button button;

            public ButtonData(Button btn)
            {
                button = btn;
            }
        }

        public enum Button
        {
            Eject,
            Audio,
            Angle,
            Subtitle,
            Clear,
            Time,
            NUM_1,
            NUM_2,
            NUM_3,
            NUM_4,
            NUM_5,
            NUM_6,
            NUM_7,
            NUM_8,
            NUM_9,
            NUM_0,
            Blue,
            Red,
            Green,
            Yellow,
            Display,
            Top_Menu,
            PopUp_Menu,
            Return,
            Triangle,
            Circle,
            Square,
            Cross,
            Arrow_Up,
            Arrow_Down,
            Arrow_Left,
            Arrow_Right,
            Enter,
            L1,
            L2,
            L3,
            R1,
            R2,
            R3,
            Playstation,
            Select,
            Start,
            Play,
            Stop,
            Pause,
            Scan_Back,
            Scan_Forward,
            Prev,
            Next,
            Step_Back,
            Step_Forward
        }
    }
}
