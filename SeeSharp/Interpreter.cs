using SeeSharp.AstDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
  public class RuntimeError: Exception
  {
    public Token token { get; }

    public RuntimeError(Token token, string message) : base(message)
    {
      this.token = token;
    }

  }
  public class Interpreter : IExprVisitor<Object>, IStmtVisitor<int>
  {

    public void interpret(List<Stmt> statements)
    {
      try
      {
        foreach(var stmt in statements)
        {
          execute(stmt);
        }
      }
      catch(RuntimeError e)
      {
        Lox.RuntimeError(e);
      }
    }

    private void execute(Stmt statement)
    {
      statement.accept<int>(this);
    }

    public object visitBinaryExpr(Binary expr)
    {
      Object left = evaluate(expr.left);
      Object right = evaluate(expr.right);

      switch(expr.oper.Type)
      {
        case TokenType.MINUS:
          checkNumberOperands(expr.oper, left, right);
          return (double)left - (double)right;

        case TokenType.SLASH:
          checkNumberOperands(expr.oper, left, right);
          return (double)left / (double)right;
        
        case TokenType.STAR:
          checkNumberOperands(expr.oper, left, right);
          return (double)left * (double)right;
        
        case TokenType.PLUS:
          if (left is double && right is double)
          {
            return (double)left + (double)right;
          }

          if(left is string && right is string)
          {
            return (string)left + (string)right;
          }
          throw new RuntimeError(expr.oper, "Operands must be two numbers or two strings.");
        
        case TokenType.GREATER:
          checkNumberOperands(expr.oper, left, right);
          return (double)left > (double)right;
        
        case TokenType.GREATER_EQUAL:
          checkNumberOperands(expr.oper, left, right);
          return (double)left >= (double)right;
        
        case TokenType.LESS:
          checkNumberOperands(expr.oper, left, right);
          return (double)left < (double)right;
        
        case TokenType.LESS_EQUAL:
          checkNumberOperands(expr.oper, left, right);
          return (double)left <= (double)right;
        
        case TokenType.BANG_EQUAL:
          return !isEqual(left, right);
        
        case TokenType.EQUAL_EQUAL:
          return isEqual(left, right);
      }

      // Should be unreachable
      return null;
    }

    public object visitGroupingExpr(Grouping expr)
    {
      return evaluate(expr.expression);
    }

    public object visitLiteralExpr(Literal expr)
    {
      return expr.objValue;
    }

    public object visitTernaryExpr(Ternary expr)
    {
      if(isTruthy(evaluate(expr.ifExpr)))
      {
        return evaluate(expr.thenExpr);
      }
      else
      {
        return evaluate(expr.elseExpr);
      }
    }

    public object visitUnaryExpr(Unary expr)
    {
      Object right = evaluate(expr.right);

      switch(expr.oper.Type)
      {
        case TokenType.BANG:
          return !isTruthy(right);
        case TokenType.MINUS:
          checkNumberOperand(expr.oper, right);
          return -(double)right;
      }

      // unreachable
      return null;
    }

    public int visitExpressionStmt(Expression expr)
    {
      evaluate(expr.expression);
      return 0;
    }

    public int visitPrintStmt(Print expr)
    {
      Object value = evaluate(expr.expression);
      Console.WriteLine(stringify(value));
      return 0;
    }

    #region Utilities
    private object evaluate(Expr expr)
    {
      return expr.accept<object>(this);
    }

    private bool isEqual(Object a, Object b)
    {
      if(a == null && b == null)
      {
        return true;
      }
      if(a == null)
      {
        return false;
      }

      return a.Equals(b);
    }

    private void checkNumberOperand(Token oper, Object o)
    {
      if(o is double)
      {
        return;
      }
      throw new RuntimeError(oper, "Operand must be a number.");
    }

    private void checkNumberOperands(Token o, Object left, Object right)
    {
      if(left is double && right is double)
      {
        return;
      }
      throw new RuntimeError(o, "Operands must be numbers.");
    }

    private bool isTruthy(Object o)
    {
      /*
       * Lox follows Ruby’s simple rule: false and nil are falsey, and everything else is truthy
       */
      if (o == null)
      {
        return false;
      }
      if(o is bool)
      {
        return (bool)o;
      }
      return true;
    }

    private string stringify(Object o)
    {
      if(o == null)
      {
        return "nil";
      }

      if(o is double)
      {
        string text = o.ToString();
        if(text.EndsWith(".0"))
        {
          text = text.Substring(0, text.Length - 2);
        }
        return text;
      }

      return o.ToString();
    }

    #endregion
  }
}
