using System;
using System.Collections.Generic;
using System.Text;
using SeeSharp.Expressions;

namespace SeeSharp.Tools
{
  public class RpnPrinter : IExprVisitor<string>
  {
    public string Print(Expr expr)
    {
      return expr.accept<string>(this);
    }

    public string visitBinaryExpr(Binary expr)
    {
      return parenthesize(expr.oper.Lexeme, new List<Expr> { expr.left, expr.right });
    }

    public string visitGroupingExpr(Grouping expr)
    {
      return parenthesize("group", new List<Expr> { expr.expression });
    }

    public string visitLiteralExpr(Literal expr)
    {
      if (expr.objValue == null)
      {
        return "nil";
      }
      else
      {
        return expr.objValue.ToString();
      }
    }

    public string visitTernaryExpr(Ternary expr)
    {
      return $"{expr.ifExpr.accept(this)} ? {expr.thenExpr.accept(this)} : {expr.elseExpr.accept(this)}";
    }

    public string visitUnaryExpr(Unary expr)
    {
      return parenthesize(expr.oper.Lexeme, new List<Expr> { expr.right });
    }

    private String parenthesize(String name, List<Expr> exprs)
    {
      StringBuilder builder = new StringBuilder();

      foreach (var expr in exprs)
      {
        builder.Append(expr.accept(this));
        builder.Append(' ');
      }

      builder.Append(name);

      return builder.ToString();
    }
  }
}
