using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;

namespace osq.Tests {
    [TestFixture]
    class TokenizerTests {
        private static IEnumerable<Token> ReadTokensFromString(string input) {
            using(var reader = new LocatedTextReaderWrapper(input)) {
                return Token.ReadTokens(reader).ToList();
            }
        }

        private void CheckTokenization(string input, object[] expected) {
            var tokens = ReadTokensFromString(input).ToList();

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
        public void MultiCharOperators() {
            CheckTokenization(
                "<<=>=> = ==! = != ===",
                new object[] { "<", "<=", ">=", ">", "=", "==", "!", "=", "!=", "==", "=" }
            );
        }

        [Test]
        public void Strings() {
            string input = "\"\\\"hello \\\\\\t\\r\\nworld\\\"\"";

            using(var reader = new LocatedTextReaderWrapper(input)) {
                var token = Token.ReadToken(reader);

                Assert.AreEqual(-1, reader.Peek());    // EOF

                Assert.AreEqual(token.TokenType, TokenType.String);
                Assert.AreEqual(token.Value, "\"hello \\\t\r\nworld\"");
            }
        }

        [Test]
        public void BadStrings() {
            Func<string, TestDelegate> stringTester = (input) => () => {
                using(var reader = new LocatedTextReaderWrapper(input)) {
                    Token.ReadToken(reader);
                }
            };

            Assert.Throws<MissingDataException>(stringTester("\""));
            Assert.Throws<MissingDataException>(stringTester("\"\\"));
            Assert.Throws<BadDataException>(stringTester("\"\\?\""));
        }
    }
}
