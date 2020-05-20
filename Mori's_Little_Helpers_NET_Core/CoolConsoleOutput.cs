using System;
using System.Collections.Generic;
using System.Linq;

namespace Schweigm_NETCore_Helpers
{
    public static class CoolConsoleOutput
    {
        public static void Write(string title, ConsoleColor color)
        {
            Console.Write("[");
            Console.ForegroundColor = color;
            Console.Write(title);
            Console.ResetColor();
            Console.Write("]" + Environment.NewLine + Environment.NewLine);
        }

        public static void Write(string title, string body, ConsoleColor color)
        {
            Console.Write("[");
            Console.ForegroundColor = color;
            Console.Write(title);
            Console.ResetColor();
            Console.Write("]\t\t"+ body + Environment.NewLine + Environment.NewLine);
        }

        public static void Write(string title, string body, List<string> bodySubValues, ConsoleColor color)
        {
            Console.Write("[");
            Console.ForegroundColor = color;
            Console.Write(title);
            Console.ResetColor();
            Console.Write("]\t\t" + body + Environment.NewLine);

            var lastString = bodySubValues.Last();
            foreach(var subString in bodySubValues)
            {
                if(subString.Equals(lastString)) Console.WriteLine("\t\t\t\t\t----------> " + subString + Environment.NewLine);
                else Console.WriteLine("\t\t\t\t\t----------> " + subString);
            }
        }
    }
}
