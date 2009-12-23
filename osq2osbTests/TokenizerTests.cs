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
        [Test]
        public void MathExpression() {
            var tokens = Tokenizer.Tokenize("( 2 + 4 ) * 7 + eval ( \"2 pi\" ) / func ( a , b )").ToList();

            object[] expected = new object[] { "(", 2, "+", 4, ")", "*", 7, "+", "eval", "(", "2 pi", ")", "/", "func", "(", "a", ",", "b", ")" };

            Assert.AreEqual(expected.Length, tokens.Count());

            for(var i = 0; i < expected.Length; ++i) {
                Assert.AreEqual(expected[i], tokens[i].Value);
            }
        }

        [Test]
        public void Strings() {
            string input = "\"\\\"hello world\\\"";

            using(var inputReader = new StringReader(input)) {
                var token = Tokenizer.Token.ReadString(inputReader);

                Assert.AreEqual(-1, inputReader.Peek());    // EOF

                Assert.AreEqual(token.Type, Tokenizer.TokenType.String);
                Assert.AreEqual(token.Value, "\"hello world\"");
            }
        }
    }
}
