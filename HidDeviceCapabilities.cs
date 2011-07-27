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

namespace HidLibrary
{
    public class HidDeviceCapabilities
    {
        internal HidDeviceCapabilities(NativeMethods.HIDP_CAPS capabilities)
        {
            Usage = capabilities.Usage;
            UsagePage = capabilities.UsagePage;
            InputReportByteLength = capabilities.InputReportByteLength;
            OutputReportByteLength = capabilities.OutputReportByteLength;
            FeatureReportByteLength = capabilities.FeatureReportByteLength;
            Reserved = capabilities.Reserved;
            NumberLinkCollectionNodes = capabilities.NumberLinkCollectionNodes;
            NumberInputButtonCaps = capabilities.NumberInputButtonCaps;
            NumberInputValueCaps = capabilities.NumberInputValueCaps;
            NumberInputDataIndices = capabilities.NumberInputDataIndices;
            NumberOutputButtonCaps = capabilities.NumberOutputButtonCaps;
            NumberOutputValueCaps = capabilities.NumberOutputValueCaps;
            NumberOutputDataIndices = capabilities.NumberOutputDataIndices;
            NumberFeatureButtonCaps = capabilities.NumberFeatureButtonCaps;
            NumberFeatureValueCaps = capabilities.NumberFeatureValueCaps;
            NumberFeatureDataIndices = capabilities.NumberFeatureDataIndices;

        }

        public short Usage { get; private set; }
        public short UsagePage { get; private set; }
        public short InputReportByteLength { get; private set; }
        public short OutputReportByteLength { get; private set; }
        public short FeatureReportByteLength { get; private set; }
        public short[] Reserved { get; private set; }
        public short NumberLinkCollectionNodes { get; private set; }
        public short NumberInputButtonCaps { get; private set; }
        public short NumberInputValueCaps { get; private set; }
        public short NumberInputDataIndices { get; private set; }
        public short NumberOutputButtonCaps { get; private set; }
        public short NumberOutputValueCaps { get; private set; }
        public short NumberOutputDataIndices { get; private set; }
        public short NumberFeatureButtonCaps { get; private set; }
        public short NumberFeatureValueCaps { get; private set; }
        public short NumberFeatureDataIndices { get; private set; }
    }
}
