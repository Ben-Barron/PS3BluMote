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
using System.IO;

namespace PS3BluMote
{
    public static class DebugLog
    {
        public static bool isLogging = false;
        private static List<string> _log = new List<string>();

        public static void write(string line)
        {
            _log.Add("[" + DateTime.Now.ToString() + "] :  " + line);
        }

        public static void outputToFile(string filename)
        {
            StreamWriter file = null;

            try
            {
                file = new StreamWriter(filename);

                foreach (string line in _log)
                {
                    file.WriteLine(line);
                }    
            }
            catch
            { }
            finally
            {
                if (file != null) file.Close();
            }
        }
    }
}
