using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens.
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals.
        IDENTIFIER, STRING, NUMBER,

        // Keywords.
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

        EOF
    }

    public class Token
    {
        // The type of this particular lexeme
        public TokenType Type { get; }

        // A group of characters that represent something
        // The only raw substrings of the source code
        public string Lexeme { get; }

        // If a lexeme is a literal, ie strings & numbers
        // We store the actual value of the runtime object for use by the interpretor later
        public Object Literal { get; }

        // Mainly for error reporting
        // Store more information here if necessary, eg column & length, etc
        public int Line { get; }

        public Token(TokenType type, string lexeme, Object literal, int line)
        {
            this.Type = type;
            this.Lexeme = lexeme;
            this.Literal = literal;
            this.Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }

    }
}
