﻿namespace lox
{
    public class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;   
        }

        public List<Stmt> Parse() {
            List<Stmt> statements = new();
            while (!IsAtEnd()) {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration() {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();
                return Statement();
            }
            catch (ParseError error)
            {
                Sync();
                return null;
            }
        }

        private Stmt VarDeclaration() {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL)) {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement() {
            if (Match(TokenType.FOR))        return ForStatement();
            if (Match(TokenType.IF))         return IfStatement();
            if (Match(TokenType.PRINT))      return PrintStatement();
            if (Match(TokenType.WHILE))      return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt ForStatement() {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer = null;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else { 
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON)) { 
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN)) { 
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = Statement();

            if (increment is not null) { 
                body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment)});
            }

            if (condition is null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer is not null) {
                body = new Stmt.Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt WhileStatement() {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt IfStatement() {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE)) { 
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private List<Stmt> Block() {
            List<Stmt> statements = new();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt PrintStatement() {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Exprect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ExpressionStatement() { 
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr Expression() {
            return Assignment();
        }

        private Expr Assignment() {
            Expr expr = Or();

            if (Match(TokenType.EQUAL)) {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr.GetType() == typeof(Expr.Variable)) {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or() {
            Expr expr = And();

            while (Match(TokenType.OR)) { 
                Token operator_ = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, operator_, right);
            }

            return expr;
        }

        private Expr And() {
            Expr expr = Equality();

            while (Match(TokenType.AND)) {
                Token operator_ = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, operator_, right);
            }

            return expr;
        }

        private Expr Equality() {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token operator_ = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, operator_, right);
            }

            return expr;
        }

        public Expr Comparison() {
            Expr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) { 
                Token operator_ = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, operator_, right);
            }

            return expr;
        }

        private Expr Term() {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS)) { 
                Token operator_ = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, operator_, right);
            }

            return expr;
        }

        private Expr Factor() {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR)) {
                Token operator_ = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, operator_, right);
            }

            return expr;
        }

        private Expr Unary() {
            if (Match(TokenType.BANG, TokenType.MINUS)) {
                Token operator_ = Previous();
                Expr right = Unary();
                return new Expr.Unary(operator_, right);
            }

            return Primary();
        }

        private Expr Primary() { 
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING)) { 
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.IDENTIFIER)) {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN)) { 
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenType type, string message) {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message) { 
            Program.Error(token, message);
            return new ParseError();
        }

        private void Sync() { 
            Advance();

            while (!IsAtEnd()) {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type) { 
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

                Advance();
            }
        }

        private bool Match(params TokenType[] types) {
            foreach (TokenType t in types) {
                if (Check(t)) {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType t) { 
            if(IsAtEnd()) return false;
            return Peek().Type == t;
        }

        private Token Advance() { 
            if(!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd() {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek() {
            return _tokens[_current];
        }

        private Token Previous() {
            return _tokens[_current - 1];
        }
    }
}
