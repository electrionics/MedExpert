using System;
using System.Collections.Generic;
using System.Linq;
using MedExpert.Core;
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

        [Test]
        public void TestGetMatched()
        {
            var tree = new List<TreeItem<double>>
            {
                new()
                {
                    Item = 1,
                    Children = new List<TreeItem<double>>
                    {
                        new()
                        {
                            Item = 0.8
                        }
                    }
                },
                new()
                {
                    Item = 0.5,
                    Children = new List<TreeItem<double>>()
                    {
                        new()
                        {
                            Item = 0.4
                        }
                    }
                }
            };

            var result = tree.GetMatched(x => x > 0.4, new HashSet<bool> {true});
            
            Assert.Less(Math.Abs(result.MakeFlat().Sum() - 2.3), 0.001);
        }

        [Test]
        public void TestGetMatchedBranch()
        {
            var tree = new List<TreeItem<double?>>
            {
                new()
                {
                    Item = 1,
                    Children = new List<TreeItem<double?>>
                    {
                        new()
                        {
                            Item = 0.8
                        }
                    }
                },
                new()
                {
                    Item = 0.5,
                    Children = new List<TreeItem<double?>>()
                    {
                        new()
                        {
                            Item = null
                        }
                    }
                }
            };
            
            var result = tree.GetMatchedBranch(x => x.Any(y => y is not null), new HashSet<bool> {true});
            
            Assert.Less(Math.Abs(result.MakeFlat().Sum(x => x ?? 0) - 2.3), 0.001);
        }
    }
}