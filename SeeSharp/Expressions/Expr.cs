using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp.Expressions
{
  public abstract class Expr
  {
    public abstract T accept<T>(IExprVisitor<T> visitor);
  }
}
