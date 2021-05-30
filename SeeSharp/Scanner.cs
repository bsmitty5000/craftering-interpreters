using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
    class Scanner
    {
        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            {"and",     TokenType.AND },
            { "class",  TokenType.CLASS},
            {"else",    TokenType.ELSE},
            {"false",   TokenType.FALSE},
            {"for",     TokenType.FOR},
            {"fun",     TokenType.FUN},
            {"if",      TokenType.IF},
            {"nil",     TokenType.NIL},
            {"or",      TokenType.OR},
            {"print",   TokenType.PRINT},
            {"return",  TokenType.RETURN},
            {"super",   TokenType.SUPER},
            {"this",    TokenType.THIS},
            {"true",    TokenType.TRUE},
            {"var",     TokenType.VAR},
            {"while",   TokenType.WHILE}
        };

        // input
        public string Source { get; }

        // output
        private List<Token> tokens;

        // points to the first character in the lexeme being scanned
        private int start = 0;

        // points at the character currently being considered
        private int current = 0;

        // tracks what source line current is on so we can produce tokens that know their location
        private int line = 1;

        public Scanner(string source)
        {
            Source = source;
        }

        // Scans source until there is nothing left to scan then appends a final EOF token to the output
        public List<Token> ScanTokens()
        {
            tokens = new List<Token>();

            while(!isAtEnd())
            {
                start = current;
                scanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));

            return tokens;
        }

        private void scanToken()
        {
            char c = advance();

            switch(c)
            {
                // Single character lexemes
                case '(': addToken(TokenType.LEFT_PAREN); break;
                case ')': addToken(TokenType.RIGHT_PAREN); break;
                case '{': addToken(TokenType.LEFT_BRACE); break;
                case '}': addToken(TokenType.RIGHT_BRACE); break;
                case ',': addToken(TokenType.COMMA); break;
                case '.': addToken(TokenType.DOT); break;
                case '-': addToken(TokenType.MINUS); break;
                case '+': addToken(TokenType.PLUS); break;
                case ';': addToken(TokenType.SEMICOLON); break;
                case '*': addToken(TokenType.STAR); break;
                case '?': addToken(TokenType.QUESTION_MARK); break;
                case ':': addToken(TokenType.COLON); break;

        // Two character lexemes
        case '!':
                    addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;

                // Comment and division operator
                case '/':

                    if (match('/'))
                    { // A comment is a lexeme, but not meangingful to the interpretor so we'll ignore it
                        // A comment goes until the end of the line.
                        while (peek() != '\n' && !isAtEnd()) advance();
                    }
                    else
                    {
                        addToken(TokenType.SLASH);
                    }
                    break;

                case '"':
                    parseString();
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                default:
                    if(isDigit(c))
                    {
                        parseNumber();
                    }
                    else if(isAlpha(c))
                    {
                        identifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        // Return the character pointed to by current and increment current
        private char advance()
        {
            return Source[current++];
        }

        // Return true and increment current pointer if Source[current] == expected
        // Otherwise, return false and do not increment current pointer
        private bool match(char expected)
        {
            if (isAtEnd())
            {
                return false;
            }
            if (Source[current] != expected)
            {
                return false;
            }

            current++;
            return true;
        }

        // Return the character pointed to by current and DO NOT increment current
        private char peek()
        {
            if (isAtEnd())
            {
                return '\0';
            }
            return Source[current];
        }

        private char peekNext()
        {
            if (current + 1 >= Source.Length)
            {
                return '\0';
            }
            return Source[current + 1];
        }

        private void parseString()
        {
            while(peek() != '"' && !isAtEnd())
            {
                if (peek() == '\n') line++;
                advance();
            }

            if(isAtEnd())
            {
                Lox.Error(line, "Untermindated string.");
                return;
            }

            // because peek was used above we need to consume the final '"'
            advance();

            // Plus/Minus one for each pointer to trim the quotes
            string value = Source.Substring(start + 1, (current - start) - 1);
            addToken(TokenType.STRING, value);
        }

        private void parseNumber()
        {
            // breeze through all the numbers until we hit something else
            while(isDigit(peek()))
            {
                advance();
            }

            if(peek() == '.' && isDigit(peekNext()))
            {
                advance();

                while(isDigit(peek()))
                {
                    advance();
                }
            }

            addToken(TokenType.NUMBER, Double.Parse(Source.Substring(start, (current - start))));
        }

        private void identifier()
        {
            while (isAlphaNumeric(peek()))
            {
                advance();
            }

            string text = Source.Substring(start, (current - start));
            TokenType type;
            if(!keywords.TryGetValue(text, out type))
            {
                type = TokenType.IDENTIFIER;
            }

            addToken(type);
        }

        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }
        private bool isAlphaNumeric(char c)
        {
            return isDigit(c) || isAlpha(c);
        }

        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, Object literal)
        {
            string text = Source.Substring(start, (current - start));
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool isAtEnd()
        {
            return current >= Source.Length;
        }


    }
}
