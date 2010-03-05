using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osq;
using osq.Parser;

using NUnit.Framework;

namespace osq.Tests {
    [TestFixture]
    class LocationTests {
        [Test]
        public void NewlineAdvance() {
            var location = new Location();

            Assert.AreEqual(1, location.LineNumber);
            Assert.AreEqual(1, location.Column);

            location.AdvanceCharacter('\n');
            Assert.AreEqual(2, location.LineNumber);
            Assert.AreEqual(1, location.Column);

            location.AdvanceCharacter('\n');
            Assert.AreEqual(3, location.LineNumber);
            Assert.AreEqual(1, location.Column);

            location.AdvanceCharacter('\r');
            Assert.AreEqual(3, location.LineNumber);
            Assert.AreEqual(1, location.Column);

            location.AdvanceCharacter('\r');
            Assert.AreEqual(4, location.LineNumber);
            Assert.AreEqual(1, location.Column);
        }

        [Test]
        new public void ToString() {
            Assert.AreEqual("filename: line 42, column 18", (new Location("filename", 42, 18)).ToString());
            Assert.AreEqual("filename: line 1, column 1", (new Location("filename")).ToString());
            Assert.AreEqual("line 42, column 18", (new Location(42, 18)).ToString());
            Assert.AreEqual("line 1, column 1", (new Location()).ToString());
        }
    }
}
