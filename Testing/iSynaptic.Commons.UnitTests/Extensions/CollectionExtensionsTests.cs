﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;

using iSynaptic.Commons.Extensions;

namespace iSynaptic.Commons.Extensions
{
    [TestFixture]
    public class CollectionExtensionsTests : BaseTestFixture
    {
        [Test]
        public void RemoveWithNull()
        {
            ICollection<int> col = null;
            Assert.Throws<ArgumentNullException>(() => col.Remove(1, 2, 3));
        }

        [Test]
        public void RemoveWithNullItems()
        {
            var col = new List<int> { 1, 2, 3 };

            col.Remove((int[])null);
            Assert.IsTrue(col.SequenceEqual(new int[] { 1, 2, 3 }));
        }

        [Test]
        public void RemoveWithEmptyItems()
        {
            var col = new List<int> { 1, 2, 3 };

            col.Remove(new int[] { });
            Assert.IsTrue(col.SequenceEqual(new int[] { 1, 2, 3 }));
        }
    }
}
