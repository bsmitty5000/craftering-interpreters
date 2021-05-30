using SeeSharp.Expressions;
using SeeSharp.Tools;
using System;
using System.IO;

namespace SeeSharp
{
    class Program
    {
        static void Main(string[] args)
        {
      if (args.Length > 1)
      {
        Console.WriteLine("Usage: SeeSharp [script]");
        Environment.Exit(64);
      }
      else if (args.Length == 1)
      {
        Lox.RunFile(args[0]);
      }
      else
      {
        Lox.RunPrompt();
      }
      //Expr expression = new Binary(
      //  new Binary(new Literal(1), new Token(TokenType.PLUS, "+", null, 1), new Literal(2)),
      //  new Token(TokenType.STAR, "*", null, 1),
      //  new Binary(new Literal(4), new Token(TokenType.MINUS, "-", null, 1), new Literal(3)));

      //Console.WriteLine(new RpnPrinter().Print(expression));
    }
    }
}
