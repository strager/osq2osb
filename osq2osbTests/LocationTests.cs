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
    }
}
