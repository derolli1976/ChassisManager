// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License. 

namespace Microsoft.GFS.WCS.WcsCli
{
    using System;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    static class NativeMethods 
    {
        /// <summary>
        /// ForeGround Color Intensity
        /// </summary>
        private const int FR_INTENSITY = 0x00000008;

        /// <summary>
        /// Background Color Intensity
        /// </summary>
        private const int BK_INTENSITY = 0x00000080;

        /// <summary>
        /// Console output handle
        /// </summary>
        private const int STD_OUTPUT_HANDLE = -11;

        /// <summary>
        /// Console input handle
        /// </summary>
        private const int STD_INPUT_HANDLE = -10;

        /// <summary>
        /// Console Output handle
        /// </summary>
        private static IntPtr hConsoleOut;

        /// <summary>
        /// Console Input handle
        /// </summary>
        private static IntPtr hConsoleIn;

        /// <summary>
        /// Gets the pointer to the current console window
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", EntryPoint = "WriteConsole", SetLastError = true,
        CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer,
        uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
        IntPtr lpReserved);

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true,
        CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint="GetConsoleScreenBufferInfo",
        SetLastError=true, CharSet=CharSet.Auto, CallingConvention=CallingConvention.StdCall)]
        private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
                         ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll", EntryPoint="SetConsoleTextAttribute",
        SetLastError=true, CharSet=CharSet.Auto, CallingConvention=CallingConvention.StdCall)]
        private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, ushort wAttributes);

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleMode",
        SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleMode",
        SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, ref ushort mode);

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleCP",
        SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint GetConsoleCP();

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleCP",
        SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetConsoleCP(uint codePage);

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleOutputCP",
        SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint GetConsoleOutputCP();

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleOutputCP",
        SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetConsoleOutputCP(uint codePage); 
 
        [StructLayout(LayoutKind.Sequential)] 
        private struct COORD
         {
            short X;
            short Y;
         }
			
        [StructLayout(LayoutKind.Sequential)] 
        private struct SMALL_RECT
         {
            short Left;
            short Top;
            short Right;
            short Bottom;
         }

        [StructLayout(LayoutKind.Sequential)] 
        private struct CONSOLE_SCREEN_BUFFER_INFO
         {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
         }

        // Constructor.
        static NativeMethods()
        {
           // Get Console Handles.
           hConsoleOut = GetStdHandle(STD_OUTPUT_HANDLE);
           hConsoleIn = GetStdHandle(STD_INPUT_HANDLE);
        }

        /// <summary>
        /// Adds forground color intensity
        /// </summary>
        internal static void AddIntensity()
        {
            CONSOLE_SCREEN_BUFFER_INFO ConsoleInfo = new CONSOLE_SCREEN_BUFFER_INFO();
            GetConsoleScreenBufferInfo(hConsoleOut, ref ConsoleInfo);
            SetConsoleTextAttribute(hConsoleOut, (ushort)(ConsoleInfo.wAttributes | FR_INTENSITY));
        }

        /// <summary>
        /// Decreases forground color intensity
        /// </summary>
        internal static void RemoveIntensity()
        {
            CONSOLE_SCREEN_BUFFER_INFO ConsoleInfo = new CONSOLE_SCREEN_BUFFER_INFO();
            GetConsoleScreenBufferInfo(hConsoleOut, ref ConsoleInfo);
            SetConsoleTextAttribute(hConsoleOut, (ushort)(ConsoleInfo.wAttributes & (~FR_INTENSITY)));
        }

        /// <summary>
        /// Write payload to the Console.
        /// </summary>
        internal static uint WriteConsole(string payload)
        {
            uint written;
            WriteConsole(hConsoleOut, payload, (uint)payload.Length, out written, IntPtr.Zero);
            return written;
        }

        /// <summary>
        /// Enables Console Word Wrap
        /// </summary>
        internal static void EnableWordWrap()
        {
            if (!SetConsoleMode(hConsoleOut, 3))
            {
                Debug.WriteLine("EnableWordWrap Error Attempting: SetConsoleMode");
            }
        }

        /// <summary>
        /// Disables Console Word Wrap
        /// </summary>
        internal static void DisableWordWrap()
        {
            if (!SetConsoleMode(hConsoleOut, 1))
            {
                Debug.WriteLine("DisableWordWrap Error Attempting: SetConsoleMode");
            }
        }

        /// <summary>
        /// Set Console Code Page
        /// </summary>
        internal static void SetCodePage(uint codepage)
        {
            if (!SetConsoleOutputCP(codepage))
            {
                Debug.WriteLine("SetConsoleOutputCP Error Attempting: SetConsoleOutputCP");
            }
        }

        /// <summary>
        ///  Get Current Code Page
        /// </summary>
        internal static uint GetCodePage()
        {
            return GetConsoleOutputCP();
        }



      }
}
