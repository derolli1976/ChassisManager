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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// WCS CLI VT100 Escape Sequence Baseclass
    /// </summary>
    abstract class Vt100Base
    {
        /// <summary>
        /// cursor position tracker
        /// </summary>
        private int _posTop = 0;

        /// <summary>
        /// cursor position tracker
        /// </summary>
        private int _posLeft = 0;

        /// <summary>
        /// cache locker, lock object for incrementing 
        /// position integers
        /// </summary>
        private object _locker = new object();

        /// <summary>
        /// VT100 escape character byte
        /// </summary>
        private const byte esc = 0x1B;

        /// <summary>
        /// VT100 bracket charactor byte
        /// </summary>
        private const byte bracket = 0x5B;

        /// <summary>
        /// VT100 value separator byte
        /// </summary>
        private const byte separator = 0x3B;

        /// <summary>
        /// Default background color
        /// </summary>
        private ConsoleColor previousBackground = ConsoleColor.Black;

        /// <summary>
        /// Default background color
        /// </summary>
        private ConsoleColor previousForground = ConsoleColor.Gray;

        /// <summary>
        /// unreconciled bytes from previous payload.
        /// </summary>
        private byte[] previous = new byte[0];

        /// <summary>
        /// Counter for processed data in the 
        /// SplitAnsiEscape method.
        /// </summary>
        private int _watcher = 0;

        /// <summary>
        /// Signals Intensity was set
        /// </summary>
        private bool intensity = false;

        /// <summary>
        /// Word wrap enable
        /// </summary>
        private bool wordwrap = false;

        /// <summary>
        /// Signals colors have been reversed.
        /// </summary>
        private bool reversed = false;

        /// <summary>
        /// Counter for processed data in the 
        /// FilterAnsiEscape method.
        /// </summary>
        private int Watcher
        {
            get { return this._watcher; }
            set { this._watcher = value; }
        }

        /// <summary>
        /// Cursor Top Position Tracker
        /// </summary>
        protected int PositionTop
        {
            get { lock (_locker) { return this._posTop; } }
            set { lock (_locker) { this._posTop = SetTopPosition(value); } }
        }

        /// <summary>
        /// Cursor Left Position Tracker
        /// </summary>
        protected int PositionLeft
        {
            get { lock (_locker) { return this._posLeft; } }
            set { lock (_locker) { this._posLeft = SetLeftPosition(value); } }
        }

        /// <summary>
        /// Sets Position Left Tracker and ensures tracker is within
        /// the Buffer ranges
        /// </summary>
        private int SetLeftPosition(int position)
        {
            if (position == Console.WindowWidth)
                position = (Console.WindowWidth - 1);

            if (position < 0)
                return 0;
            else
                return position;
        }

        /// <summary>
        /// Sets Position Top Tracker and ensures tracker is within
        /// the Buffer ranges
        /// </summary>
        private int SetTopPosition(int position)
        {
            if (position == Console.WindowHeight)
                position = (Console.WindowHeight - 1);

            if (position < 0)
                return 0;
            else
                return position;
        }

        /// <summary>
        /// default encoding
        /// </summary>
        internal Encoding encode = Encoding.ASCII;

        /// <summary>
        /// Initialize Class
        /// </summary>
        internal Vt100Base()
        {
            encode = Encoding.GetEncoding(437);
        }

        /// <summary>
        /// Extracts VT100/ANSI escape sequences from the raw byte array 
        /// </summary>
        /// <param name="data"></param>
        internal void SplitAnsiEscape(byte[] data)
        {
            // ensure data is not passed as null object.
            if (data != null)
            {
                int prevLen = previous.Length;

                // if there were previous bytes, append 
                // new data.
                if (prevLen > 0)
                {
                    Array.Resize<byte>(ref previous, (prevLen + data.Length));

                    Buffer.BlockCopy(data, 0, previous, prevLen, data.Length);

                    // processing data should be the resized previous array.
                    data = previous;
                }

                previous = new byte[0] { };

                List<byte> screenTxt = new List<byte>();

                // indicates escape sequence end
                // was found within the byte array.
                bool foundEscEnd = false;

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == esc)
                    {
                        // signal end of escape sequence
                        // has not been found.
                        foundEscEnd = false;

                        // if cannot read the next byte due to lenght, previous will catch on the next pass.
                        if (i + 1 < data.Length)
                        {
                            // if the next symbol is a bracket, it's not a VT100 escape sequence we care about.
                            if (data[i + 1] == bracket)
                            {
                                for (int x = i; x < data.Length; x++)
                                {
                                    if ((data[x] >= 0x41 && data[x] <= 0x5A)
                                        || (data[x] >= 0x61 && data[x] <= 0x7A)) // search for A-Z && a - z ir 00 TODO:
                                    {
                                        List<byte> payload = new List<byte>();

                                        for (; i <= x; i++)
                                        {
                                            // drop none ANSI charactors
                                            if (data[i] >= 0x1B && data[i] <= 0x7E)
                                            {
                                                payload.Add(data[i]);
                                            }
                                        }

                                        if (payload.Count > 0)
                                        {
                                            // Post the current string data in position
                                            OutPutString(screenTxt.ToArray());
                                            // clear the string list, to prevent duplicaiton
                                            screenTxt.Clear();

                                            // execute the Vt100 escape sequence
                                            EscapeSeqAction(payload.ToArray());
                                        }

                                        i = x;

                                        foundEscEnd = true;

                                        // break loop.
                                        x = data.Length;
                                    }
                                }
                            }
                            else // treat byte as screen text
                            {
                                screenTxt.Add(data[i]);
                                foundEscEnd = true;
                            }
                        }

                        if (!foundEscEnd)
                        {
                            List<byte> unPocessed = new List<byte>();

                            for (int z = i; z < data.Length; z++)
                            {
                                // drop none ANSI charactors
                                if (data[z] >= 0x1B && data[z] <= 0x7E)
                                {
                                    unPocessed.Add(data[z]);
                                }
                            }

                            previous = new byte[unPocessed.Count];

                            Buffer.BlockCopy(data, i, previous, 0, unPocessed.Count);

                            i = data.Length;
                        }
                    }
                    else
                    {
                        screenTxt.Add(data[i]);
                    }

                    OutPutString(screenTxt.ToArray());
                    screenTxt.Clear();
                }
            }
        }

        /// <summary>
        /// Writes Console Output
        /// </summary>
        private void OutPutString(byte[] screenTxt)
        {
            if (screenTxt.Count() > 0)
            {
                try
                {
                    Console.Write(encode.GetString(screenTxt));
                }
                catch (System.IO.IOException ex)
                {
                    Debug.Write("Unable to Write to Console. IO Exception: " 
                        + ex.Message 
                        + " when trying to write: " 
                        + SharedFunc.ByteArrayToHexString(screenTxt));
                }
            }
        }

        /// <summary>
        /// Executes the action defined by the VT100/ANSI escape sequence
        /// </summary>
        private void EscapeSeqAction(byte[] payload)
        {
            //Debug.WriteLine(string.Format("Payload: {0} Data: {1}",
            //    SharedFunc.ByteArrayToHexString(payload),
            //    Encoding.ASCII.GetString(payload)));

            int lenght = payload.Length;

            string data = string.Empty;

            int num = 0, index = 2;

            // determine whether to lookup two digits or 1
            if (lenght == 4)
                index = 1;

            //Debug.Assert(payload[0] == esc && payload[1] == bracket);

            if (lenght >= 3)
            {
                // Extract the data portion of the payload
                data = encode.GetString(payload, 2, (lenght - 3));

                // split the data into strings
                string[] vars = data.Split(';');

                // function code byte
                byte func = payload[lenght - 1];
                #region Text Attributes <Esc> m
                if (func == 0x6D) // func: m
                {
                    if (lenght == 3)
                    {
                        // Esc[m  Turn off character attributes 
                        RestAttributes();
                    }
                    else
                    {
                        foreach (string var in vars)
                        {
                            if (int.TryParse(var, out num))
                            {
                                if (num < 10)
                                {
                                    switch (num)
                                    {
                                        case 0:     // Esc[0m	Turn off character attributes 
                                            RestAttributes();
                                            break;
                                        case 1:     // Esc[1m 	Turn bold mode on 
                                            NativeMethods.AddIntensity();
                                            intensity = true;
                                            break;
                                        case 2:     // Esc[2m 	Turn low intensity mode on 
                                            NativeMethods.RemoveIntensity();
                                            intensity = false;
                                            break;
                                        case 3:     // Esc[3m 	Standout.
                                            NativeMethods.AddIntensity();
                                            intensity = true;
                                            break;
                                        case 4:     // Esc[4m 	Turn underline mode on 
                                            Debug.WriteLine(string.Format("Turn underline mode on"));
                                            break;
                                        case 5:     // Esc[5m 	Turn blinking mode on
                                            Debug.WriteLine(string.Format("Turn blinking mode on"));
                                            break;
                                        case 7:     // Esc[7m 	Turn reverse video on 
                                            RollColors();
                                            reversed = true;
                                            break;
                                        case 8:     // Esc[8m 	Turn invisible text mode on 
                                            InvisibleText();
                                            break;
                                        default:
                                            Debug.WriteLine(string.Format("Unknown m number: {0}", num));
                                            break;
                                    }
                                }
                                else if (num >= 30 && num <= 49)
                                {
                                    switch (num)
                                    {
                                        case 30: // black foreground
                                            SetForegroundColor(ConsoleColor.Black);
                                            break;
                                        case 31: // red foreground
                                            SetForegroundColor(ConsoleColor.Red);
                                            break;
                                        case 32: // green foreground
                                            SetForegroundColor(ConsoleColor.Green);
                                            break;
                                        case 33: // yellow foreground
                                            SetForegroundColor(ConsoleColor.Yellow);
                                            break;
                                        case 34: // blue foreground
                                            SetForegroundColor(ConsoleColor.Blue);
                                            break;
                                        case 35: // magenta foreground
                                            SetForegroundColor(ConsoleColor.Magenta);
                                            break;
                                        case 36: // cyan foreground
                                            SetForegroundColor(ConsoleColor.Cyan);
                                            break;
                                        case 37: // white foreground
                                            SetForegroundColor(ConsoleColor.White);
                                            break;
                                        case 39: // default foreground
                                            SetForegroundColor(ConsoleColor.Gray);
                                            break;
                                        case 40: // black background
                                            SetBackgroundColor(ConsoleColor.Black);
                                            break;
                                        case 41: // red background
                                            SetBackgroundColor(ConsoleColor.Red);
                                            break;
                                        case 42: // green background
                                            SetBackgroundColor(ConsoleColor.Green);
                                            break;
                                        case 43: // yellow background
                                            SetBackgroundColor(ConsoleColor.Yellow);
                                            break;
                                        case 44: // blue background
                                            SetBackgroundColor(ConsoleColor.Blue);
                                            break;
                                        case 45: // magenta background
                                            SetBackgroundColor(ConsoleColor.Magenta);
                                            break;
                                        case 46: // cyan background
                                            SetBackgroundColor(ConsoleColor.Cyan);
                                            break;
                                        case 47: // white background
                                            SetBackgroundColor(ConsoleColor.Gray);
                                            break;
                                        case 49: // default background
                                            SetBackgroundColor(ConsoleColor.Black);
                                            break;
                                        default:
                                            Debug.WriteLine(string.Format("Unknown Color change: {0} Data: {1}",
                                                SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine(string.Format("Uable to Parse Number in: {0} Data: {1}",
                                        SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                }
                            }
                            else
                            {
                                Debug.WriteLine(string.Format("Unable to parse: {0} Data: {1}",
                                    SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                            }
                        }
                    }

                }
                #endregion
                #region Cursor Position  <Esc> A - G Or <Esc> f
                else if ((func >= 0x41 && func <= 0x48)
                    || (func == 0x66))
                {

                    int left = PositionLeft;
                    int top = PositionTop;

                    if (lenght == 3)
                    {
                        switch (func)
                        {
                            case 0x66:
                            case 0x48:
                                SetCursorPosition(0, 0);
                                break;
                            default:
                                Debug.WriteLine(string.Format("Error converting to cursor move: {0} Data {1}: ",
                                    SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                break;
                        }
                    }
                    else if (lenght > 3 && (func == 0x48 || func == 0x66))
                    {
                        if (vars.Count() > 1)
                        {
                            if (int.TryParse(vars[0], out top) && int.TryParse(vars[1], out left))
                            {
                                // remove 1 for zero based offset
                                SetCursorPosition((top - 1), (left - 1));
                            }
                            else
                            {
                                Debug.WriteLine(string.Format("Error converting to cursor move: {0} Data {1}: ",
                                    SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                            }
                        }
                        else if (int.TryParse(vars[0], out top))
                        {
                            // remove 1 for zero based offset
                            left = Console.CursorLeft;
                            SetCursorPosition((top - 1), left);
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("Error converting to cursor move: {0} Data {1}: ",
                                SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                        }
                    }
                    else if (lenght > 3 && (func >= 0x41 && func <= 0x44)) // Move cursor up n lines 
                    {
                        Direction dir = Direction.Invalid;

                        List<int> dirs = new List<int>();

                        foreach (string var in vars)
                        {
                            if (int.TryParse(var, out num))
                            {
                                dirs.Add(num);
                            }
                        }

                        switch (func)
                        {
                            case 0x41:
                                dir = Direction.Up;
                                break;
                            case 0x42:
                                dir = Direction.Down;
                                break;
                            case 0x43:
                                dir = Direction.Right;
                                break;
                            case 0x44:
                                dir = Direction.Left;
                                break;
                            default:
                                Debug.WriteLine(string.Format("Unknown Direction: {0} Data {1}: ",
                                    SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                break;
                        }

                        if (dirs.Count > 0 && dir != Direction.Invalid)
                            DirectionalMove(dirs.ToArray(), dir);
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("Move/Tab unknown: {0}  Bytes: {1} Data {2}: ", lenght,
                            SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                    }

                }
                #endregion
                #region Set Console Mode <Esc>=h
                else if (func == 0x68)
                {
                    // set mode
                    if (payload[2] == 0x3D)
                    {
                        foreach (string var in vars)
                        {
                            if (int.TryParse(var.Substring(1), out num))
                            {

                                switch (num)
                                {
                                    case 0:     // 0	40 x 25 monochrome (text) 
                                        SetConsoleSize(40, 25);
                                        SetConsoleBufferSize(40, 25);
                                        break;
                                    case 1:     // 1	40 x 25 color (text) 
                                        SetConsoleSize(40, 25);
                                        SetConsoleBufferSize(40, 25);
                                        break;
                                    case 2:     // 2	80 x 25 monochrome (text) 
                                        SetConsoleSize(80, 25);
                                        SetConsoleBufferSize(80, 25);
                                        break;
                                    case 3:     // 3	80 x 25 color (text) 
                                        SetConsoleSize(80, 25);
                                        SetConsoleBufferSize(80, 25);
                                        break;
                                    case 7:     // 7	Enables line wrapping 
                                        NativeMethods.EnableWordWrap();
                                        wordwrap = true;
                                        break;
                                    case 4:     // 4	320 x 200 4-color (graphics)
                                    case 5:     // 5	320 x 200 monochrome (graphics)
                                    case 6:     // 6	640 x 200 monochrome (graphics)
                                    case 13:    // 13	320 x 200 color (graphics) 
                                    case 14:    // 14	640 x 200 color (16-color graphics) 
                                    case 15:    // 15	640 x 350 monochrome (2-color graphics) 
                                    case 16:    // 16	640 x 350 color (16-color graphics) 
                                    case 17:    // 17	640 x 480 monochrome (2-color graphics) 
                                    case 18:    // 18	640 x 480 color (16-color graphics) 
                                    case 19:    // 19	320 x 200 color (256-color graphics) 
                                        Debug.WriteLine(string.Format("Set Mode Graphics Mode Un-supported: {0}  Number: {1} Data {2}: ", num,
                                        SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                        break;
                                    default:
                                        Debug.WriteLine(string.Format("Set Mode <ESC>[= num.  Unknown num: {0}  Number: {1} Data {2}: ", num,
                                        SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                        break;
                                }
                            }
                            else
                            {
                                Debug.WriteLine(string.Format("Set Mode <ESC>[= num.  Unable to convert num: {0}  Lenght: {1} Data {2}: ", lenght,
                                SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("Set Mode <ESC>[= num.  3rd char not = symbol: {0}  Lenght: {1} Data {2}: ", lenght,
                        SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                    }
                }
                #endregion
                #region Clear Line <Esc> J & <Esc> K
                else if (func == 0x4A) // J
                {
                    //Erase Down		<ESC>[J
                    //Erases the screen from the current position down to the bottom of the screen.
                    if (lenght == 3)
                    {
                        ClearToDirection(Direction.Down);
                    }
                    else
                    {
                        foreach (string var in vars)
                        {
                            if (int.TryParse(var, out num))
                            {
                                switch (num)
                                {
                                    case 0:
                                        //Erase Up		<ESC>[0J
                                        //Erases the screen from the current position up to the end the screen. 
                                        ClearToDirection(Direction.Down);
                                        break;
                                    case 1:
                                        //Erase Up		<ESC>[1J
                                        //Erases the screen from the current line up to the top of the screen. 
                                        ClearToDirection(Direction.Up);
                                        break;
                                    case 2:
                                        //Erase Screen		<ESC>[2J
                                        //Erases the screen with the background color and moves the cursor to home. 
                                        Clear();

                                        // reset the cursor position
                                        SetCursorPosition(0, 0);

                                        break;
                                    default:
                                        Debug.WriteLine(string.Format("Erase Screen <ESC>[num J unknown num: {0}  Num: {1} Data: {2} ", num,
                                        SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                        break;
                                }
                            }
                            else
                            {
                                Debug.WriteLine(string.Format("Erase Line <ESC> J unable to convert number: {0}  Bytes: {1} Data: {2}, {3} ", lenght,
                                SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload), index));
                            }
                        }
                    }
                }
                else if (func == 0x4B) // K
                {

                    //Erases from the current cursor position to the end of the current line. 
                    if (lenght == 3)
                    {
                        //Erase End of Line	<ESC>[K
                        ClearToDirection(Direction.Right);
                    }
                    else
                    {
                        foreach (string var in vars)
                        {
                            if (int.TryParse(var, out num))
                            {
                                switch (num)
                                {
                                    case 0:
                                        ClearToDirection(Direction.Right);
                                        break;
                                    case 1:
                                        ClearToDirection(Direction.Left);
                                        break;
                                    case 2:
                                        ClearLine();
                                        break;
                                    default:
                                        Debug.WriteLine(string.Format("Erase Screen <ESC>[num K unknown num: {0}  Num: {1} Data {2}: ", num,
                                        SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                                        break;
                                }
                            }
                            else
                            {
                                Debug.WriteLine(string.Format("Erase Line <ESC>[ K unable to convert number: {0}  Bytes: {1} Data {2}: ", lenght,
                                SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format("Erase Line <ESC>[ ?? unable to convert function: {0}  Bytes: {1} Data {2}: ", lenght,
                    SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
                }
                #endregion
            }
            else
            {
                Debug.WriteLine(string.Format("Malformed function lenght: {0}  Bytes: {1} Data {2}: ", lenght,
                SharedFunc.ByteArrayToHexString(payload), Encoding.ASCII.GetString(payload)));
            }


        }

        /// <summary>
        /// Resets all text attributes
        /// </summary>
        private void RestAttributes()
        {
            // turn off intensity.

            if (intensity)
            {
                NativeMethods.RemoveIntensity();

                intensity = false;
            }

            // Disable word-wrap.
            if (wordwrap)
            {
                NativeMethods.DisableWordWrap();
                wordwrap = false;
            }

            // if reversed, roll colors back
            if (reversed)
            {
                RollColors();
                reversed = false;
            }

            // Reset console colors to their defaults.
            Console.ResetColor();

        }

        /// <summary>
        /// Reverses forground and background colors.
        /// </summary>
        private void RollColors()
        {
            ConsoleColor forground = Console.ForegroundColor;
            ConsoleColor background = Console.BackgroundColor;

            // set background to foreground
            Console.BackgroundColor = forground;
            // set forground to background.
            Console.ForegroundColor = background;

        }

        /// <summary>
        /// Sets Forground the same as background
        /// </summary>
        private void InvisibleText()
        {
            SetForegroundColor(Console.BackgroundColor);
        }

        #region Console Commands

        #region Console/Buffer Size

        /// <summary>
        /// Get the Current Console Window Size
        /// </summary>
        protected void GetConsoleSize(out int width, out int height)
        {
            width = Console.WindowWidth;
            height = Console.WindowHeight;
        }

        /// <summary>
        /// Set the Console Window Size
        /// </summary>
        protected void SetConsoleSize(int width, int height)
        {
            Console.SetWindowSize(width, height);
        }

        /// <summary>
        /// Get the Console Buffer Size
        /// </summary>
        protected void GetConsoleBufferSize(out int width, out int height)
        {
            width = Console.BufferWidth;
            height = Console.BufferHeight;
        }

        /// <summary>
        /// Get the Console Buffer Size
        /// </summary>
        protected void SetConsoleBufferSize(int width, int height)
        {
            Console.SetBufferSize(width, height);
        }

        #endregion

        #region Cursor Position

        /// <summary>
        /// Get the Current Console Cursor position
        /// </summary>
        protected void GetCursorPosition(out int left, out int top)
        {
            left = Console.CursorLeft;
            top = Console.CursorTop;
        }

        /// <summary>
        /// Moves the console cursor in the direction specified, by the 
        /// numbers specified
        /// </summary>
        protected void DirectionalMove(int[] nums, Direction dir)
        {

            foreach (int num in nums)
            {
                switch (dir)
                {
                    case Direction.Up:
                        SetCursorUp(num);
                        break;
                    case Direction.Down:
                        SetCursorDown(num);
                        break;
                    case Direction.Left:
                        SetCursorBack(num);
                        break;
                    case Direction.Right:
                        SetCursorForward(num);
                        break;
                    case Direction.Invalid:
                    default:
                        Debug.WriteLine(string.Format("Unknown Cursor Movement: {0} Direction: {1}", "DirectionalMove", dir.ToString()));
                        break;
                }

            }

        }

        /// <summary>
        /// Cursor Directions
        /// </summary>
        protected enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            Invalid
        }

        /// <summary>
        /// Sets the position of the cursor.
        /// Parameters:
        ///   left:
        ///     The column position of the cursor.
        ///
        ///   top:
        ///     The row position of the cursor.
        /// </summary>
        protected virtual void SetCursorPosition(int top, int left)
        {
            PositionTop = top;
            PositionLeft = left;

            //Console.CursorLeft = PositionLeft;
            //Console.CursorTop = PositionTop;

            Console.SetCursorPosition(PositionLeft, PositionTop);
        }

        /// <summary>
        /// Sets the position of the cursor.
        /// Parameters:
        ///   top:
        ///     The row position of the cursor.
        /// </summary>
        protected virtual void SetCursorUp(int top)
        {
            PositionTop = (Console.CursorTop - top);
            SetCursorPosition(Console.CursorLeft, PositionTop);
        }

        /// <summary>
        /// Sets the position of the cursor.
        /// Parameters:
        ///   top:
        ///     The row position of the cursor.
        /// </summary>
        protected virtual void SetCursorDown(int top)
        {
            PositionTop = (Console.CursorTop + top);

            Console.SetCursorPosition(Console.CursorLeft, PositionTop);
        }

        /// <summary>
        /// Sets the position of the cursor.
        /// Parameters:
        ///   left:
        ///     The column position of the cursor.
        /// </summary>
        protected virtual void SetCursorForward(int left)
        {
            PositionLeft = (Console.CursorLeft + left);
            Console.SetCursorPosition(PositionLeft, Console.CursorTop);
        }

        /// <summary>
        /// Sets the position of the cursor.
        /// Parameters:
        ///   left:
        ///     The column position of the cursor.
        /// </summary>
        protected virtual void SetCursorBack(int left)
        {
            PositionLeft = (Console.CursorLeft - left);
            Console.SetCursorPosition(PositionLeft, Console.CursorTop);
        }

        #endregion

        #region Color

        /// <summary>
        /// Sets the console forground color
        /// </summary>
        protected virtual void SetForegroundColor(ConsoleColor color)
        {
            previousForground = color;
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// Sets the console background color
        /// </summary>
        protected virtual void SetBackgroundColor(ConsoleColor color)
        {
            previousBackground = color;
            Console.BackgroundColor = color;
        }

        #endregion

        #region Clear

        /// <summary>
        /// Clear the entire console
        /// </summary>
        protected virtual void Clear()
        {
            Console.Clear();
        }

        /// <summary>
        /// Clear Console to the beginging of the line to the end of line
        /// </summary>
        protected virtual void ClearLine()
        {
            // set start of line
            PositionLeft = Console.CursorLeft;

            // set to start of line
            Console.CursorLeft = 0;

            // clear to the right.
            ClearToDirection(Direction.Right);

            // set to start of line
            Console.CursorLeft = PositionLeft;

            PositionTop = Console.CursorTop;
            PositionLeft = Console.CursorLeft;
        }

        /// <summary>
        /// Clear Console to the current position to the end of line
        /// </summary>
        protected virtual void ClearToDirection(Direction dir)
        {
            PositionTop = Console.CursorTop;
            PositionLeft = Console.CursorLeft;

            // capture current cursor positions.
            int originalLeft = PositionLeft;
            int originalTop = PositionTop;

            switch (dir)
            {
                case Direction.Up:
                    // move the cursor to the top
                    SetCursorPosition(0, 0);
                    // Erase from the current line position to the top of the screen
                    for (int i = 0; i <= originalTop; i++)
                    {
                        for (int x = 0; x < Console.WindowHeight; x++)
                        {
                            Console.Write(" ");
                        }

                        SetCursorPosition(i, 0);
                    }
                    // set the cursor back to the original row
                    // reset the cursor left to the begining of the line
                    SetCursorPosition(originalTop, originalLeft);
                    break;
                case Direction.Down:
                    // Erase from the current line position to the bottom of the screen
                    for (int i = originalTop; i <= Console.WindowHeight; i++)
                    {
                        Console.CursorLeft = 0;

                        for (int x = 0; x < Console.WindowWidth; x++)
                        {
                            Console.Write(" ");
                        }

                        SetCursorPosition(i, 0);
                    }
                    // set the cursor back to the original row
                    // reset the cursor left to the begining of the line
                    SetCursorPosition(originalTop, originalLeft);
                    break;
                case Direction.Left:
                    // set cursor to begining of line
                    SetCursorPosition(originalTop, 0);
                    // shift cursor to known left position
                    for (int i = 0; i < PositionLeft; i++)
                    {
                        // overwrite with nothing (erase).
                        Console.Write(" ");
                    }
                    SetCursorPosition(originalTop, originalLeft);
                    break;
                case Direction.Right:
                    // from current position to the width of the console,
                    // write nothing
                    for (int i = originalLeft; i <= Console.WindowWidth; i++)
                    {
                        Console.Write(" ");
                    }
                    // set the cursor back to origin.
                    SetCursorPosition(originalTop, originalLeft);
                    break;
                case Direction.Invalid:
                default:
                    Debug.WriteLine(string.Format("Unknow erase comamnd: {0}  ", dir.ToString()));
                    break;
            }
        }

        #endregion

        #endregion
    }   
}
