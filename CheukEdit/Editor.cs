using System;
using System.Diagnostics;
using System.IO;

namespace CheukEdit
{
    internal class Editor
    {
        private int x, y;
        private char mode;
        private Buffer buff;
        private string status, filename;

        public Editor()
        {
            x = 0; y = 0; mode = 'n';
            status = "Normal Mode";
            filename = "untitled.txt";

            /* Initializes buffer and appends line to prevent seg. faults */
            buff = new Buffer();
            buff.appendLine("");
        }

        public Editor(string fn)
        {
            x = 0; y = 0; mode = 'n';
            status = "Normal Mode";
            filename = fn;

            buff = new Buffer();

            try
            {
                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    // Open the text file using a stream reader.
                    using (var sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            // Read the stream to a string, and append it to the buffer.
                            buff.appendLine(sr.ReadLine());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The file {0} could not be read:", filename);
                Debug.WriteLine(e.Message);
                buff.appendLine("");
            }
        }

        #region Cursor Movement

        private void moveHome()
        {
            x = 0;
            Console.SetCursorPosition(x, y);
        }
        private void moveLeft()
        {
            if (x - 1 >= 0)
            {
                x--;
                Console.SetCursorPosition(x, y);
            }
        }
        private void moveRight()
        {
            // Throws ArgumentOutOfRangeException if
            // left is greater than or equal to BufferWidth
            // or
            // top is greater than or equal to BufferHeight
            if (x + 1 < Console.WindowWidth && x + 1 <= buff.lines[y].Length)
            {
                x++;
                Console.SetCursorPosition(x, y);
            }
        }
        private void moveEnd()
        {
            x = buff.lines[y].Length;
            Console.SetCursorPosition(x, y);
        }
        private void moveUp()
        {
            if (y - 1 >= 0)
            {
                y--;
            }

            if (x >= buff.lines[y].Length)
            {
                x = buff.lines[y].Length;
            }

            Console.SetCursorPosition(x, y);
        }
        private void moveDown()
        {
            if (y + 1 < Console.WindowHeight - 1 && y + 1 < buff.lines.Count)
            {
                y++;
            }

            if (x >= buff.lines[y].Length)
            {
                x = buff.lines[y].Length;
            }

            Console.SetCursorPosition(x, y);
        }

        #endregion Cursor Movement

        /// <summary>
        /// Deletes current line
        /// </summary>
        private void deleteLine()
        {
            buff.removeLine(y);
        }

        /// <summary>
        /// Deletes line <int>
        /// </summary>
        /// <param name="i"></param>
        private void deleteLine(int i)
        {
            buff.removeLine(i);
        }

        /// <summary>
        /// Saves buffer into the file
        /// </summary>
        private void saveFile()
        {
            if (filename == "")
            {
                // Set filename to untitled
                filename = "untitled.txt";
            }

            try
            {
                using (var fs = new FileStream(filename, FileMode.Create))
                {
                    // Open the text file using a stream reader.
                    using (var sw = new StreamWriter(fs))
                    {
                        for (var i = 0; i < buff.lines.Count; i++)
                        {
                            sw.WriteLine(buff.lines[i]);
                        }
                        status = "Saved to file!";
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The file {0} could not be read:", filename);
                Debug.WriteLine(e.Message);
                status = "Error: Cannot open file for writing!";
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public char getMode()
        {
            return mode;
        }

        /// <summary>
        /// Handles keyboard input
        /// </summary>
        /// <param name="c"></param>
        public void handleInput(ConsoleKeyInfo c)
        {
            switch (c.Key)
            {
                case ConsoleKey.Home:
                    moveHome();
                    return;
                case ConsoleKey.LeftArrow:
                    moveLeft();
                    return;
                case ConsoleKey.RightArrow:
                    moveRight();
                    return;
                case ConsoleKey.End:
                    moveEnd();
                    return;
                case ConsoleKey.UpArrow:
                    moveUp();
                    return;
                case ConsoleKey.DownArrow:
                    moveDown();
                    return;
            }

            switch (mode)
            {
                case 'n':
                    switch (c.Key)
                    {
                        case ConsoleKey.X:
                            // Press 'x' to exit
                            mode = 'x';
                            break;
                        case ConsoleKey.I:
                            // Press 'i' to enter insert mode
                            mode = 'i';
                            break;
                        case ConsoleKey.S:
                            // Press 's' to save the current file
                            saveFile();
                            break;
                    }
                    break;
                case 'i':
                    switch (c.Key)
                    {
                        case ConsoleKey.Escape:
                            // The Escape/Alt key
                            mode = 'n';
                            break;
                        case ConsoleKey.Backspace:
                            // The Backspace key
                            if (x == 0 && y > 0)
                            {
                                x = buff.lines[y - 1].Length;
                                // Bring the line down
                                buff.lines[y - 1] += buff.lines[y];
                                // Delete the current line
                                deleteLine();
                                moveUp();
                            }
                            else
                            {
                                try
                                {
                                    // Removes a character
                                    buff.lines[y] = buff.lines[y].Remove(--x, 1);
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    Debug.WriteLine(e.Message);
                                    // Attempting to delete from beginning of empty line
                                    Console.Beep();
                                    x++;
                                }
                            }
                            break;
                        case ConsoleKey.Delete:
                            // The Delete key
                            if (x == buff.lines[y].Length && y != buff.lines.Count - 1)
                            {
                                // Bring the line down
                                buff.lines[y] += buff.lines[y + 1];
                                // Delete the line
                                deleteLine(y + 1);
                            }
                            else
                            {
                                try
                                {
                                    buff.lines[y] = buff.lines[y].Remove(x, 1);
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    Debug.WriteLine(e.Message);
                                    // Attempting to delete from end of line
                                    Console.Beep();
                                }
                            }
                            break;
                        case ConsoleKey.Enter:
                            // The Enter key
                            // Bring the rest of the line down
                            if (x < buff.lines[y].Length)
                            {
                                // Put the rest of the line on a new line
                                buff.insertLine(buff.lines[y].Substring(x, buff.lines[y].Length - x), y + 1);
                                // Remove that part of the line
                                buff.lines[y] = buff.lines[y].Remove(x, buff.lines[y].Length - x);
                            }
                            else
                            {
                                buff.insertLine("", y + 1);
                            }
                            x = 0;
                            moveDown();
                            break;
                        case ConsoleKey.Tab:
                            // The Tab key
                            buff.lines[y] = buff.lines[y].Insert(x, "    ");
                            x += 4;
                            break;
                        default:
                            // Any other character
                            buff.lines[y] = buff.lines[y].Insert(x, c.KeyChar.ToString());
                            x++;
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void printBuff()
        {
            for (var i = 0; i < Console.WindowHeight - 1; i++)
            {
                if (i >= buff.lines.Count)
                {
                    Console.SetCursorPosition(0, i);
                    //Stdscr.ClearToEol();
                }
                else
                {
                    Console.SetCursorPosition(0, i);
                    Console.Write(buff.lines[i]);
                }
                //Stdscr.ClearToEol();
            }
            Console.SetCursorPosition(x, y);
        }

        /// <summary>
        /// Prints the status line (like vim!!!)
        /// </summary>
        public void printStatusLine()
        {
            var origF = Console.ForegroundColor;
            var origB = Console.BackgroundColor;
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.ForegroundColor = origB;
            Console.BackgroundColor = origF;
            Console.Write(status);
            Console.ForegroundColor = origF;
            Console.BackgroundColor = origB;
            Console.Title = Console.WindowHeight + " - " + Console.BufferHeight;
            Console.SetCursorPosition(x, y);
        }

        /// <summary>
        /// Updates the status line (text, not display)
        /// </summary>
        public void updateStatus()
        {
            switch (mode)
            {
                case 'n':
                    // Normal mode
                    status = "Normal Mode";
                    break;
                case 'i':
                    // Insert mode
                    status = "Insert Mode";
                    break;
                case 'q':
                case 'x':
                    // Exiting
                    status = "Exiting";
                    break;
            }

            status += "\tCOL: " + x.ToString() + "\tROW: " + y.ToString();
        }
    }
}
