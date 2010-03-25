using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq.Tests.Helpers {
    public class CollectionTokenReader : ITokenReader {
        private IList<Token> tokens;
        private int curToken;

        public CollectionTokenReader(IEnumerable<Token> tokens) {
            this.tokens = tokens.ToList();
            this.curToken = 0;
        }

        public Token ReadToken() {
            if(curToken >= tokens.Count) {
                return null;
            }

            return tokens[curToken++];
        }
    }
}
