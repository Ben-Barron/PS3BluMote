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
using System.Runtime.InteropServices;

namespace WindowsAPI
{
    public class SendInputAPI
    {
        #region SendInput API

        [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
        static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
        static extern IntPtr GetMessageExtraInfo();

        private enum KeyEvent
        {
            KeyUp = 0x0002,
            KeyDown = 0x0000,
            ExtendedKey = 0x0001
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput mi;

            [FieldOffset(0)]
            public KeyboardInput ki;

            [FieldOffset(0)]
            public HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Input
        {
            public uint type;
            public InputUnion iu;
        }

        #endregion

        public class Keyboard
        {
            public bool isSmsEnabled;
            public List<KeyCode> lastKeysDown;

            private int lastNumberPress, smsArrayIndex;
            private DateTime lastPressTime;

            #region "SMS Input"
            static KeyCode[][] numpadLetters =
            {
                new KeyCode[] { KeyCode.Spacebar, KeyCode.NUMPAD_0 },
                new KeyCode[] { KeyCode.NUMPAD_1, KeyCode.Math_Subtract, KeyCode.Period },
                new KeyCode[] { KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.NUMPAD_2 },
                new KeyCode[] { KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.NUMPAD_3 },
                new KeyCode[] { KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.NUMPAD_4 },
                new KeyCode[] { KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.NUMPAD_5 },
                new KeyCode[] { KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.NUMPAD_6 },
                new KeyCode[] { KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.NUMPAD_7 },
                new KeyCode[] { KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.NUMPAD_8 },
                new KeyCode[] { KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z, KeyCode.NUMPAD_9 }
            };
            #endregion

            public Keyboard(bool smsEnabled)
            {
                isSmsEnabled = smsEnabled;
            }

            public void sendKey(KeyCode Key)
            {
                Input[] InputList = new Input[2];

                Input keyInput = new Input();
                keyInput.type = 1;

                keyInput.iu.ki.wScan = 0;
                keyInput.iu.ki.time = 0;
                keyInput.iu.ki.dwFlags = (int)KeyEvent.KeyDown;
                keyInput.iu.ki.dwExtraInfo = GetMessageExtraInfo();
                keyInput.iu.ki.wVk = (ushort)Key;

                InputList[0] = keyInput;

                keyInput.iu.ki.dwFlags = (int)KeyEvent.KeyUp;

                InputList[1] = keyInput;

                SendInput((uint)2, InputList, Marshal.SizeOf(InputList[0]));

                lastKeysDown = null;
            }

            public void releaseLastKeys()
            {
                if (lastKeysDown != null) sendKeysUp(lastKeysDown);
            }

            public void sendKeysDown(List<KeyCode> KeysDown)
            {
                if (KeysDown.Count > 0)
                {
                    if (isSmsEnabled && (KeysDown.Count == 1))
                    {
                        ushort numPressed = (ushort)KeysDown[0];
                        numPressed = (numPressed > 95) ? (numPressed -= 96) : (numPressed -= 48);

                        if ((numPressed >= 0) && (numPressed <= 9))
                        {
                            KeyCode[] letters = numpadLetters[numPressed];

                            if ((numPressed == lastNumberPress) && ((DateTime.Now - lastPressTime).TotalMilliseconds < 1500))
                            {
                                smsArrayIndex = ((letters.Length - 1) == smsArrayIndex) ? 0 : (smsArrayIndex + 1);

                                sendKey(KeyCode.Backspace); //Send backspace                    
                            }
                            else
                            {
                                smsArrayIndex = 0;
                            }

                            sendKey(letters[smsArrayIndex]);

                            lastNumberPress = numPressed;
                            lastPressTime = DateTime.Now;

                            return;
                        }
                        else
                        {
                            lastPressTime = DateTime.Now;
                            lastNumberPress = 10; //set above 0-9 range.
                        }
                    }

                    lastKeysDown = KeysDown;
                    Input[] InputList = new Input[KeysDown.Count];

                    for (int i = 0; i < KeysDown.Count; i++)
                    {
                        Input keyInput = new Input();
                        keyInput.type = 1;

                        keyInput.iu.ki.wScan = 0;
                        keyInput.iu.ki.time = 0;
                        keyInput.iu.ki.dwFlags = (int)KeyEvent.KeyDown;
                        keyInput.iu.ki.dwExtraInfo = GetMessageExtraInfo();
                        keyInput.iu.ki.wVk = (ushort)KeysDown[i];

                        InputList[i] = keyInput;
                    }

                    SendInput((uint)KeysDown.Count, InputList, Marshal.SizeOf(InputList[0]));
                }
            }

            public void sendKeysUp(List<KeyCode> KeysUp)
            {
                if (KeysUp.Count > 0)
                {
                    Input[] InputList = new Input[KeysUp.Count];

                    for (int i = 0; i < KeysUp.Count; i++)
                    {
                        Input keyInput = new Input();
                        keyInput.type = 1;

                        keyInput.iu.ki.wScan = 0;
                        keyInput.iu.ki.time = 0;
                        keyInput.iu.ki.dwFlags = (int)KeyEvent.KeyUp;
                        keyInput.iu.ki.dwExtraInfo = GetMessageExtraInfo();
                        keyInput.iu.ki.wVk = (ushort)KeysUp[i];

                        InputList[i] = keyInput;
                    }

                    SendInput((uint)KeysUp.Count, InputList, Marshal.SizeOf(InputList[0]));
                }
            }

            public enum KeyCode
            {
                Backspace = 8,
                Tab = 9,
                Clear = 12,
                Enter = 13,
                Shift = 16,
                Ctrl = 17,
                Alt = 18,
                Pause = 19,
                Caps_Lock = 20,
                Esc = 27,
                Spacebar = 32,
                Page_Up = 33,
                Page_Down = 34,
                End = 35,
                Home = 36,
                Arrow_Left = 37,
                Arrow_Up = 38,
                Arrow_Right = 39,
                Arrow_Down = 40,
                Select = 41,
                Print = 42,
                Execute = 43,
                Print_Screen = 44,
                Insert = 45,
                Delete = 46,
                Help = 47,
                KEY_0 = 48,
                KEY_1 = 49,
                KEY_2 = 50,
                KEY_3 = 51,
                KEY_4 = 52,
                KEY_5 = 53,
                KEY_6 = 54,
                KEY_7 = 55,
                KEY_8 = 56,
                KEY_9 = 57,
                A = 65,
                B = 66,
                C = 67,
                D = 68,
                E = 69,
                F = 70,
                G = 71,
                H = 72,
                I = 73,
                J = 74,
                K = 75,
                L = 76,
                M = 77,
                N = 78,
                O = 79,
                P = 80,
                Q = 81,
                R = 82,
                S = 83,
                T = 84,
                U = 85,
                V = 86,
                W = 87,
                X = 88,
                Y = 89,
                Z = 90,
                Left_Windows = 91,
                Right_Windows = 92,
                Applications = 93,
                Computer_Sleep = 95,
                NUMPAD_0 = 96,
                NUMPAD_1 = 97,
                NUMPAD_2 = 98,
                NUMPAD_3 = 99,
                NUMPAD_4 = 100,
                NUMPAD_5 = 101,
                NUMPAD_6 = 102,
                NUMPAD_7 = 103,
                NUMPAD_8 = 104,
                NUMPAD_9 = 105,
                Math_Multiply = 106,
                Math_Add = 107,
                Math_Subtract = 109,
                Math_Decimal = 110,
                Math_Divide = 111,
                F1 = 112,
                F2 = 113,
                F3 = 114,
                F4 = 115,
                F5 = 116,
                F6 = 117,
                F7 = 118,
                F8 = 119,
                F9 = 120,
                F10 = 121,
                F11 = 122,
                F12 = 123,
                F13 = 124,
                F14 = 125,
                F15 = 126,
                F16 = 127,
                F17 = 128,
                F18 = 129,
                F19 = 130,
                F20 = 131,
                F21 = 132,
                F22 = 133,
                F23 = 134,
                F24 = 135,
                Num_Lock = 144,
                Scroll_Lock = 145,
                Left_Shift = 160,
                Right_Shift = 161,
                Left_Control = 162,
                Right_Control = 163,
                Left_Menu = 164,
                Right_Menu = 165,
                Browser_Back = 166,
                Browser_Forward = 167,
                Browser_Refresh = 168,
                Browser_Stop = 169,
                Browser_Search = 170,
                Browser_Favorites = 171,
                Browser_Start_and_Home = 172,
                Volume_Mute = 173,
                Volume_Down = 174,
                Volume_Up = 175,
                Media_Next_Track = 176,
                Media_Previous_Track = 177,
                Media_Stop = 178,
                Media_Play_Pause = 179,
                Start_Mail = 180,
                Select_Media = 181,
                Start_Application_1 = 182,
                Start_Application_2 = 183,
                Semicolon = 186,
                Equals_Sign = 187,
                Comma = 188,
                Hyphen = 189,
                Period = 190,
                Forward_Slash = 191,
                Tilde = 192,
                Open_Square_Bracket = 219,
                Back_Slash = 220,
                Close_Square_Bracket = 221,
                Single_Quote = 222,
                Play = 250,
                Zoom = 251
            }
        }
    }
}
