using System;
using System.Linq;
using MedExpert.Core.Helpers;
using NUnit.Framework;

namespace MedExpert.UnitTests
{
    // 7 digits mostly
    public class CommonTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSeverityProject()
        {
            var baseVector = new double[] {1, 1, 1};
            var toProjectVector = new double[] {4, 4, 4};
            var projection = baseVector.Project(toProjectVector);
            Assert.IsTrue(projection.All(x => x == 4));
            Assert.Pass();
        }
    }
}