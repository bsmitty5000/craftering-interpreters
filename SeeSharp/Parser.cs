using SeeSharp.AstDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeSharp
{
  public class ParseError : Exception { }

  public class Parser
  {
    private List<Token> tokens = new List<Token>();
    private int current = 0;

    public Parser(List<Token> tokens)
    {
      this.tokens = tokens;
    }

    public List<Stmt> parse()
    {
      List<Stmt> statements = new List<Stmt>();
      while(!isAtEnd())
      {
        statements.Add(declaration());
      }

      return statements;
    }

    #region Grammar rules
    /*
     * A recursive descent parser is a literal translation of the 
     * grammar’s rules straight into imperative code. Each rule 
     * becomes a function. The body of the rule translates to code 
     * roughly like:
     * 
     *  Grammar notation 	    |     Code representation
     *  ----------------------|-----------------------------------------------------
              Terminal        |     Code to match and consume a token
     *  ----------------------|-----------------------------------------------------
     *       Nonterminal      |     Call to that rule’s function
     *  ----------------------|-----------------------------------------------------
     *           |            |     if or switch statement
     *  ----------------------|-----------------------------------------------------
     *         * or +         |     while or for loop
     *  ----------------------|-----------------------------------------------------
     *           ?            |     if statement
     *  ----------------------|-----------------------------------------------------
     */

    private delegate Expr ruleDelegate();

    // declaration & varDecl are broken up because we can't have varDecl in some places we have
    // declarations
    // declaration -> varDecl | statement
    private Stmt declaration()
    {
      try
      {
        if (match(new List<TokenType>() { TokenType.VAR }))
        {
          return varDecl();
        }

        return statement();
      }
      catch(ParseError)
      {
        synchronize();
        return null;
      }
    }

    // varDecl 	->	"var" IDENTIFIER ( "=" expression )? ";" ; 
    private Stmt varDecl()
    {
      Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

      Expr initializer = null;
      if(match(new List<TokenType>() { TokenType.EQUAL }))
      {
        initializer = expression();
      }

      consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

      return new Var(name, initializer);
    }

    // statement      → exprStmt
    //            | forStmt
    //            | ifStmt
    //            | printStmt
    //            | whileStmt
    //            | block ;
    private Stmt statement()
    {
      if (match(new List<TokenType>() { TokenType.IF }))
      {
        return ifStatement();
      }
      if(match(new List<TokenType>() { TokenType.FOR }))
      {
        return forStatement();
      }
      if (match(new List<TokenType>() { TokenType.PRINT }))
      {
        return printStatement();
      }
      if(match(new List<TokenType>() { TokenType.WHILE }))
      {
        return whileStatement();
      }

      if(match(new List<TokenType>() { TokenType.LEFT_BRACE }))
      {
        return new Block(block());
      }
      return expressionStatement();
    }

    private List<Stmt> block()
    {
      List<Stmt> statements = new List<Stmt>();

      while(!check(TokenType.RIGHT_BRACE) && !isAtEnd())
      {
        statements.Add(declaration());
      }

      consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

      return statements;
    }

    //forStmt       ->  "for" "(" (varDecl | exprStmt | ";" )
    //              expression? ";"
    //              expression? ")" statement ;
    private Stmt forStatement()
    {
      consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
      Stmt initializer;
      if(match(new List<TokenType>() { TokenType.SEMICOLON }))
      {
        initializer = null;
      }
      else if(match(new List<TokenType>() { TokenType.VAR }))
      {
        initializer = varDecl();
      }
      else
      {
        initializer = expressionStatement();
      }

      Expr condition = null;
      if(!check(TokenType.SEMICOLON))
      {
        condition = expression();
      }
      consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

      Expr increment = null;
      if(!check(TokenType.RIGHT_PAREN))
      {
        increment = expression();
      }

      consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses");

      Stmt body = statement();

      // if the increment is present add it to the end of the body
      if(increment != null)
      {
        body = new Block(new List<Stmt>() { body, new Expression(increment) });
      }

      // the meat of this 'desugaring' is changing the for loop into a while loop
      // and manually taking care of the increment & initializer
      if(condition == null)
      {
        condition = new Literal(true);
      }
      body = new While(condition, body);

      // if the initializer exists, add it as a statement to run
      // once before the while loop runs
      if(initializer != null)
      {
        body = new Block(new List<Stmt>() { initializer, body });
      }
      
      return body;

    }

    // printStatement -> "print" expression ";"
    private Stmt printStatement()
    {
      Expr value = expression();
      consume(TokenType.SEMICOLON, "Expect ';' after value.");
      return new Print(value);
    }

    //  ifStmt        ->  "if" "(" expression ")" statement
     //                   ( "else" statement )? ;
    private Stmt ifStatement()
    {
      consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'");
      Expr condition = expression();
      consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
      Stmt thenBranch = statement();
      Stmt elseBranch = null;
      if(match(new List<TokenType>() { TokenType.ELSE }))
      {
        elseBranch = statement();
      }

      return new If(condition, thenBranch, elseBranch);
    }
    // whileStmt     -> "while" "(" expression ")" statement ;
    private Stmt whileStatement()
    {
      consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'");
      Expr condition = expression();
      consume(TokenType.RIGHT_PAREN, "Expect ')' after while statement.");
      Stmt body = statement();
      return new While(condition, body);
    }

    // expressionStatement -> expression ";"
    private Stmt expressionStatement()
    {
      Expr value = expression();
      consume(TokenType.SEMICOLON, "Expect ';' after value.");
      return new Expression(value);
    }

    // expression -> equality ;
    private Expr expression()
    {
      return assignment();
    }

    // assignment	->	IDENTIFIER "=" assignment | logic_or ;
    private Expr assignment()
    {
      Expr expr = or();

      if (match(new List<TokenType>() { TokenType.EQUAL }))
      {
        Token equals = previous();
        Expr value = assignment();

        if(expr is Variable)
        {
          Token name = ((Variable)expr).name;
          return new Assign(name, value);
        }

        error(equals, "Invalid assignment target.");
      }

      return expr;
    }

    // logic_or    ->  logic_and( "or" logic_and)* ;
    private Expr or()
    {
      Expr expr = and();

      while (match(new List<TokenType>() { TokenType.OR }))
      {
        Token oper = previous();
        Expr right = and();
        expr = new Logical(expr, oper, right);
      }

      return expr;
    }

    // logic_and   ->  ternary ( "and" ternary)* ;
    private Expr and()
    {
      Expr expr = ternary();

      while (match(new List<TokenType>() { TokenType.AND }))
      {
        Token oper = previous();
        Expr right = ternary();
        expr = new Logical(expr, oper, right);
      }

      return expr;
    }

    // ternary		->	equality ("?" expression ":" ternary)? ;
    private Expr ternary()
    {
      Expr expr = equality();

      if (match(new List<TokenType>() { TokenType.QUESTION_MARK }))
      {
        Expr thenBranch = expression();
        consume(TokenType.COLON, "Expect ':' after ternary expression.");
        Expr elseBranch = ternary();
        expr = new Ternary(expr, thenBranch, elseBranch);

      }

      return expr;
    }

    // equality -> comparison ( ( "!=" | "==" ) comparison )* ;
    private Expr equality()
    {
      return BinaryRuleExpansion(comparison, new List<TokenType>() { TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL });
    }

    // comparison -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    private Expr comparison()
    {
      return BinaryRuleExpansion(term, new List<TokenType>() { TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL });
    }

    // term -> factor ( ( "-" | "+" ) factor )* ;
    private Expr term()
    {
      return BinaryRuleExpansion(factor, new List<TokenType>() { TokenType.MINUS, TokenType.PLUS });
    }

    // factor -> unary ( ( "/" | "*" ) unary )* ;
    private Expr factor()
    {
      return BinaryRuleExpansion(unary, new List<TokenType>() { TokenType.STAR, TokenType.SLASH });
    }

    // unary  ->  ( "!" | "-" ) unary | primary ;
    private Expr unary()
    {
      if(match(new List<TokenType>() { TokenType.BANG, TokenType.MINUS }))
      {
        Token oper = previous();
        Expr right = unary();
        return new Unary(oper, right);
      }

      return primary();
    }

    /*
     primary        -> 	"true" | "false" | "nil"
					              | NUMBER | STRING
					              | "(" expression ")"
					              | IDENTIFIER;
    */
    private Expr primary()
    {
      if(match(new List<TokenType>() { TokenType.FALSE }))
      {
        return new Literal(false);
      }
      if (match(new List<TokenType>() { TokenType.TRUE }))
      {
        return new Literal(true);
      }
      if (match(new List<TokenType>() { TokenType.NIL }))
      {
        return new Literal(null);
      }

      if (match(new List<TokenType>() { TokenType.NUMBER, TokenType.STRING }))
      {
        return new Literal(previous().Literal);
      }

      if(match(new List<TokenType>() { TokenType.IDENTIFIER }))
      {
        return new Variable(previous());
      }

      if(match(new List<TokenType>() { TokenType.LEFT_PAREN }))
      {
        Expr expr = expression();
        consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
        return new Grouping(expr);
      }

      throw error(peek(), "Expected expression");
    }
    #endregion

    #region Utilities
    private Expr BinaryRuleExpansion(ruleDelegate rule, List<TokenType> types)
    {
      Expr expr = rule();

      /*
       * Note that if the parser never encounters an equality operator, 
       * then it never enters the loop. In that case, the equality() method 
       * effectively calls and returns comparison(). In that way, this 
       * method matches an equality operator or anything of higher precedence. 
       */
      while (match(types))
      {
        Token oper = previous();
        Expr right = rule();
        expr = new Binary(expr, oper, right);
      }

      return expr;
    }

    private Token consume(TokenType type, string message)
    {
      if(check(type))
      {
        return advance();
      }

      throw error(peek(), message);
    }

    private ParseError error(Token token, string message)
    {
      Lox.Error(token, message);
      return new ParseError();
    }

    private Boolean match(List<TokenType> types)
    {
      foreach (var type in types)
      {
        if(check(type))
        {
          advance();
          return true;
        }
      }

      return false;
    }

    private Boolean check(TokenType type)
    {
      if (isAtEnd())
      {
        return false;
      }

      return peek().Type == type;
    }

    private Token advance()
    {
      if (!isAtEnd())
      {
        current++;
      }
      return previous();
    }

    private Token peek()
    {
      return tokens[current];
    }

    private Token previous()
    {
      return tokens[current - 1];
    }

    private bool isAtEnd()
    {
      return peek().Type == TokenType.EOF;
    }

    private void synchronize()
    {
      advance();

      while(!isAtEnd())
      {
        if(previous().Type == TokenType.SEMICOLON)
        {
          return;
        }

        switch(peek().Type)
        {
          case TokenType.CLASS:
          case TokenType.FUN:
          case TokenType.VAR:
          case TokenType.FOR:
          case TokenType.IF:
          case TokenType.WHILE:
          case TokenType.PRINT:
          case TokenType.RETURN:
            return;
        }

        advance();
      }
    }
    #endregion
  }
}
