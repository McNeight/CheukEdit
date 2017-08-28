using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CheukEdit
{
    /// <summary>
    /// https://cheukyin699.github.io/tutorial/c++/2015/02/01/ncurses-editor-tutorial-01.html
    /// </summary>
    class Program : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(
               IntPtr consoleOutput,
               bool maximumWindow,
               CONSOLE_FONT_INFO_EX consoleCurrentFontEx);

        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public static Editor ed;

        public static unsafe void Main(string[] args)
        {
            //string fontName = "Lucida Console";
            string fontName = "Anonymous Pro";
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                info.cbSize = (uint)Marshal.SizeOf(info);
                bool tt = false;
                // First determine whether there's already a TrueType font.
                if (GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    tt = (info.FontFamily & TMPF_TRUETYPE) == TMPF_TRUETYPE;
                    if (tt)
                    {
                        Debug.WriteLine("The console already is using a TrueType font.");
                        //return;
                    }
                    // Set console font to Lucida Console.
                    CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    IntPtr ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);
                    // Get some settings from current font.
                    newInfo.dwFontSize = new COORD(info.dwFontSize.X, info.dwFontSize.Y);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, newInfo);
                }
            }

            if (args.Length >= 1)
            {
                ed = new Editor(args[0]);
            }
            else
            {
                ed = new Editor();
            }

            using (Program prg = new Program())
            {
                try
                {
                    prg.Run();
                }
                finally
                {
                    //Curses.EndWin();
                }
            }
        }

        public Program()
        {
            //Curses.InitScr();
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Curses.EndWin();
        }

        #endregion

        private void Run()
        {
            Initialize();

            while (ed.getMode() != 'x')
            {
                ed.updateStatus();
                ed.printStatusLine();
                ed.printBuff();

                // Don't echo keystrokes
                ConsoleKeyInfo input = Console.ReadKey(true);
                ed.handleInput(input);

                //
                //Stdscr.Refresh();
                Console.Clear();
                // Window housekeeping
                if (Console.BufferHeight != Console.WindowHeight)
                {
                    Console.BufferHeight = Console.WindowHeight;
                }
                if (Console.BufferWidth != Console.WindowWidth)
                {
                    Console.BufferWidth = Console.WindowWidth;
                }
            }
        }

        private void Initialize()
        {
            Console.Clear();
            // Set the cursor to 0, 0
            Console.SetCursorPosition(0, 0);
            // Set the console window size to 80x25
            Console.SetWindowSize(80, 25);
            // Set the console buffer size to 80x25
            Console.SetBufferSize(80, 25);
            //
            Console.SetWindowPosition(0, 0);
            // Visible cursor
            Console.CursorVisible = true;
            // Full length cursor
            Console.CursorSize = 100;
            // Set input encoding to UTF16
            Console.InputEncoding = System.Text.Encoding.Unicode;
            // Set output encoding to UTF16
            Console.OutputEncoding = System.Text.Encoding.Unicode;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;

            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }
    }
}
