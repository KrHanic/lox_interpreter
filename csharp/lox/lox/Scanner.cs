namespace lox
{
    internal class Scanner
    {
        private string source;
        private List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> 
        {
            { "and",    TokenType.AND },
            { "class",  TokenType.CLASS },
            { "else",   TokenType.ELSE },
            { "false",  TokenType.FALSE },
            { "for",    TokenType.FOR },
            { "fun",    TokenType.FUN },
            { "if",     TokenType.IF },
            { "nil",    TokenType.NIL },
            { "or",     TokenType.OR },
            { "print",  TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super",  TokenType.SUPER },
            { "this",   TokenType.THIS },
            { "true",   TokenType.TRUE },
            { "var",    TokenType.VAR },
            { "while",  TokenType.WHILE }
        };

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd()) {
                start = current;
                ScanToken();
            }

            var eofToken = new Token(TokenType.EOF, "", null, line);
            tokens.Add(eofToken);
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;

                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;

                case '/':
                    if (Match('/')) {
                        while(Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else { 
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                case '"': StringLiteral(); break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Program.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private bool IsAlpha(char c)
        {
            bool isAlpha =
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_';
            return isAlpha;
        }

        private void Identifier()
        {
            while(IsAlphaNumeric(Peek())) Advance();

            string lexeme = source.Substring(start, current - start);
            bool keyExists = keywords.TryGetValue(lexeme, out TokenType type);
            if(!keyExists) type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        private bool IsAlphaNumeric(char c)
        {
            bool isAlphaNumeric = IsAlpha(c) || IsDigit(c);
            return isAlphaNumeric;
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            string lexeme = source.Substring(start, current - start);
            double literal = double.Parse(lexeme);
            AddToken(TokenType.NUMBER, literal);
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            char nextNextChar = source[current + 1];
            return nextNextChar;
        }

        private bool IsDigit(char c)
        {
            bool isDigit = c >= '0' && c <= '9';
            return isDigit;
        }

        private void StringLiteral()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Program.Error(line, "Unterminated string.");
                return;
            }

            Advance();

            string literal = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, literal);
        }

        private char Peek()
        {
            if(IsAtEnd()) return '\0';
            char nextChar = source[current];
            return nextChar;
        }

        private bool Match(char expected)
        {
            if(IsAtEnd()) return false;
            if(source[current] != expected) return false;

            current++;
            return true;
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string lexeme = source.Substring(start, current - start);
            var token = new Token(type, lexeme, literal, line);
            tokens.Add(token);
        }

        private char Advance()
        {
            char nextChar = source[current++];
            return nextChar;
        }

        private bool IsAtEnd()
        {
            bool isAtEnd = current >= source.Length;
            return isAtEnd;
        }
    }
}