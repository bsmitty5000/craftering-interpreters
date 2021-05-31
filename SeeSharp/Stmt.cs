using SeeSharp.AstDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
  public abstract class Stmt
  {
    public abstract T accept<T>(IStmtVisitor<T> visitor);
  }
}
