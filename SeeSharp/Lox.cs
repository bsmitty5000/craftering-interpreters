using SeeSharp.AstDefinitions;
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
    private static bool hadError = false;
    private static bool hadRuntimeError = false;
    private static Interpreter interpreter { get; } = new Interpreter();

    public static void RunFile(string path)
    {
      var bytes = File.ReadAllBytes(path);
      run(System.Text.Encoding.Default.GetString(bytes));

      if (hadError)
      {
        Environment.Exit(65);
      }
      if(hadRuntimeError)
      {
        Environment.Exit(70);
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
      var statements = parser.parse();

      if (hadError) return;

      interpreter.interpret(statements);
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

    public static void RuntimeError(RuntimeError error)
    {
      Console.WriteLine($"{error.Message} \n[line {error.token.Line}]");
      hadRuntimeError = true;
    }

    private static void report(int line, string where, string message)
    {
      Console.WriteLine($"[line {line}] Error {where}: {message}");
      hadError = true;
    }
  }
}
