using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CheukEdit
{
    /// <summary>
    /// https://cheukyin699.github.io/tutorial/c++/2015/02/01/ncurses-editor-tutorial-01.html
    /// </summary>
    internal class Program : IDisposable
    {
        public static Editor ed;

        public static unsafe void Main(string[] args)
        {
            //string fontName = "Lucida Console";
            var fontName = "Anonymous Pro";
            var hnd = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);
            if (hnd != NativeMethods.INVALID_HANDLE_VALUE)
            {
                var info = new NativeMethods.CONSOLE_FONT_INFO_EX();
                info.cbSize = (uint)Marshal.SizeOf(info);
                var tt = false;
                // First determine whether there's already a TrueType font.
                if (NativeMethods.GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    tt = (info.FontFamily & NativeMethods.TMPF_TRUETYPE) == NativeMethods.TMPF_TRUETYPE;
                    if (tt)
                    {
                        Debug.WriteLine("The console already is using a TrueType font.");
                        //return;
                    }
                    // Set console font to Lucida Console.
                    var newInfo = new NativeMethods.CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = NativeMethods.TMPF_TRUETYPE;
                    var ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);
                    // Get some settings from current font.
                    newInfo.dwFontSize = new NativeMethods.COORD(info.dwFontSize.X, info.dwFontSize.Y);
                    newInfo.FontWeight = info.FontWeight;
                    NativeMethods.SetCurrentConsoleFontEx(hnd, false, newInfo);
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

            using (var prg = new Program())
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
                var input = Console.ReadKey(true);
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

    }
}
