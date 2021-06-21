using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
  public class VarEnvironment
  {
    private Dictionary<string, object> values = new Dictionary<string, object>();
    public VarEnvironment enclosing { get; }

    public VarEnvironment()
    {
      enclosing = null;
    }

    public VarEnvironment(VarEnvironment enclosing)
    {
      this.enclosing = enclosing;
    }

    public void define(string name, object value)
    {
      values[name] = value;
    }

    public object get(Token name)
    {
      if(values.ContainsKey(name.Lexeme))
      {
        return values[name.Lexeme];
      }

      if(enclosing != null)
      {
        return enclosing.get(name);
      }

      throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
    }

    public void assign(Token name, object value)
    {
      if(values.ContainsKey(name.Lexeme))
      {
        values[name.Lexeme] = value;
        return;
      }

      if(enclosing != null)
      {
        enclosing.assign(name, value);
        return;
      }

      throw new RuntimeError(name,
        $"Undefined variable {name.Lexeme}.");
    }
  }
}
