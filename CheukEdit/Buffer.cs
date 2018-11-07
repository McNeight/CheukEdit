using System;
using System.Collections.Generic;

namespace CheukEdit
{
    /// <summary>
    /// 
    /// </summary>
    internal class Buffer
    {
        public List<string> lines;

        public Buffer()
        {
            lines = new List<string>();
        }

        /* Some helper functions */
        public void insertLine(string line, int n)
        {
            line = remTabs(line);  // Conversion (happens every time)
            lines.Insert(n, line);
        }

        public void appendLine(string line)
        {
            line = remTabs(line);
            lines.Add(line);
        }

        public void removeLine(int n)
        {
            lines.RemoveAt(n);
        }

        /// <summary>
        /// Substitutes all tabs in string for 4 spaces, so that the tabs won't screw everything up
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string remTabs(string line)
        {
            var tab = line.IndexOf('\t');
            if (tab == -1)
            {
                return line;
            }
            else
            {
                return remTabs(line.Replace("\t", "    "));
            }
        }

        public int Search(string query)
        {
            //Parallel.For(0, query.Length,
            //    () => 0,
            //    (j, loop, subtotal) =>
            //    {
            //        int y = lines.BinarySearch(query);
            //    },
            //    (s) =>
            //    {
            //        lock (this)
            //        {
            //        }
            //    }
            //    );

            return lines.BinarySearch(query);
        }
    }
}
