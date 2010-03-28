namespace osq.Parser {
    public enum TokenType {
        Symbol,
        Number,
        Identifier,
        String,
        WhiteSpace
    }

    public class Token {
        public TokenType TokenType {
            get;
            set;
        }

        public object Value {
            get;
            set;
        }

        public Location Location {
            get;
            set;
        }

        public Token(TokenType type, object value, Location location = null) {
            TokenType = type;
            Value = value;
            Location = location;
        }

        public override string ToString() {
            return Value.ToString();
        }

        public override bool Equals(object obj) {
            var objAsToken = obj as Token;

            if(objAsToken == null) {
                return false;
            }

            return Equals(objAsToken);
        }

        public bool Equals(Token otherToken) {
            if(otherToken == null) {
                return false;
            }

            if(otherToken.Location == null) {
                if(this.Location != null) {
                    return false;
                }
            } else if(!otherToken.Location.Equals(this.Location)) {
                return false;
            }

            if(!otherToken.TokenType.Equals(this.TokenType)) {
                return false;
            }

            if(otherToken.Value == null) {
                if(this.Value != null) {
                    return false;
                }
            } else if(!otherToken.Value.Equals(this.Value)) {
                return false;
            }

            return true;
        }

        public bool IsSymbol(string expected) {
            return TokenType == TokenType.Symbol && (string)Value == expected;
        }
    }
}