namespace lox
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

        public Expr? Parse() {
            try
            {
                return Expression();
            }
            catch (ParseError e)
            {
                return null;
            }
        }

        private Expr Expression() {
            return Equality();
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
