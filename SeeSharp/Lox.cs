using SeeSharp.Expressions;
using SeeSharp.Tools;
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

      if (hadError)
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
      Parser parser = new Parser(tokens);
      Expr expression = parser.parse();

      if (hadError) return;

      Console.WriteLine(new RpnPrinter().Print(expression));
    }

    public static void Error(int line, string message)
    {
      report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
      if (token.Type == TokenType.EOF)
      {
        report(token.Line, " at end", message);
      }
      else
      {
        report(token.Line, $" at '{token.Lexeme}'", message);
      }
    }

    private static void report(int line, string where, string message)
    {
      Console.WriteLine($"[line {line}] Error {where}: {message}");
      hadError = true;
    }
  }
}
