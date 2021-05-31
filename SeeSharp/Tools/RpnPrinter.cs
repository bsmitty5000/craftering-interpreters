using SeeSharp.AstDefinitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeSharp.Tools
{
  public class RpnPrinter : IAstVisitor<string>
  {
    public string Print(Expr expr)
    {
      return expr.accept<string>(this);
    }

    public string visitBinary(Binary expr)
    {
      return parenthesize(expr.oper.Lexeme, new List<Expr> { expr.left, expr.right });
    }

    public string visitExpression(Expression expr)
    {
      throw new NotImplementedException();
    }

    public string visitGrouping(Grouping expr)
    {
      return parenthesize("group", new List<Expr> { expr.expression });
    }

    public string visitLiteral(Literal expr)
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

    public string visitPrint(Print expr)
    {
      throw new NotImplementedException();
    }

    public string visitTernary(Ternary expr)
    {
      return $"{expr.ifExpr.accept(this)} ? {expr.thenExpr.accept(this)} : {expr.elseExpr.accept(this)}";
    }

    public string visitUnary(Unary expr)
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
