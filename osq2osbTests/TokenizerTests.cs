using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq2osb;
using osq2osb.Parser;

using NUnit.Framework;

namespace osq2osb.Tests {
    [TestFixture]
    class TokenizerTests {
        private void CheckTokenization(string input, object[] expected) {
            var tokens = Tokenizer.Tokenize(input).ToList();

            Assert.AreEqual(expected.Length, tokens.Count());

            for(var i = 0; i < expected.Length; ++i) {
                Assert.AreEqual(expected[i], tokens[i].Value);
            }
        }

        [Test]
        public void MathExpression() {
            CheckTokenization(
                "( 2 + 4 ) * 7 + eval ( \"2 pi\" ) / func ( a , b )",
                new object[] { "(", 2, "+", 4, ")", "*", 7, "+", "eval", "(", "2 pi", ")", "/", "func", "(", "a", ",", "b", ")" }
            );
        }

        [Test]
        public void Parentheses() {
            CheckTokenization(
                "((rand()) - (-(0.5))) / 4",
                new object[] { "(", "(", "rand", "(", ")", ")", "-", "(", "-", "(", 0.5, ")", ")", ")", "/", 4 }
            );
        }

        [Test]
        public void Strings() {
            string input = "\"\\\"hello world\\\"";

            using(var rawReader = new StringReader(input))
            using(var reader = new LocatedTextReaderWrapper(rawReader)) {
                var token = Tokenizer.Token.ReadToken(reader);

                Assert.AreEqual(-1, reader.Peek());    // EOF

                Assert.AreEqual(token.Type, Tokenizer.TokenType.String);
                Assert.AreEqual(token.Value, "\"hello world\"");
            }
        }
    }
}
