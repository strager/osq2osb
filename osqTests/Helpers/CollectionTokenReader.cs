using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq.Parser;

namespace osq.Tests.Helpers {
    public class CollectionTokenReader : ITokenReader {
        private IList<Token> tokens;
        private int curToken;

        public Location CurrentLocation {
            get {
                return null;
            }
        }

        public CollectionTokenReader(IEnumerable<Token> tokens) {
            this.tokens = tokens.ToList();
            this.curToken = 0;
        }

        public Token PeekToken() {
            if(curToken >= tokens.Count) {
                return null;
            }

            return tokens[curToken];    
        }

        public Token ReadToken() {
            var token = PeekToken();
            ++curToken;
            return token;
        }
    }
}
