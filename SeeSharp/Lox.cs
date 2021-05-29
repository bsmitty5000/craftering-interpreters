using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
    class Lox
    {
        static bool hadError = false;

        public static void RunFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            run(System.Text.Encoding.Default.GetString(bytes));

            if(hadError)
            {
                Environment.Exit(65);
            }
        }

        public static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                run(line);

                hadError = false;
            }
        }
        private static void run(string s)
        {
            Scanner scanner = new Scanner(s);
            var tokens = scanner.ScanTokens();

            foreach (Token t in tokens)
            {
                Console.WriteLine(t);
            }
        }

        public static void Error(int line, string message)
        {
            report(line, "", message);
        }

        private static void report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            hadError = true;
        }
    }
}
