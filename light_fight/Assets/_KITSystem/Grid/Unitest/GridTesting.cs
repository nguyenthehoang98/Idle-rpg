using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace _KITSystem.Grid.Unitest
{
    public class GridTesting
    {
        private IGridManager grid;

        [SetUp]
        public void Setup()
        {
            grid = new FixedUniformGrid(1);
        }

        [Test]
        public void Insert_ShouldAddObject()
        {
            bool inserted = grid.Insert(1, float2.zero);

            Assert.IsTrue(inserted);

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.IsTrue(count > 0);

            Assert.Contains(1, results);
        }

        [Test]
        public void Insert_SameCell_ShouldNotDuplicate()
        {
            grid.Insert(1, float2.zero);

            grid.Insert(1, new float2(0.2f, 0.2f));

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Insert_NewCell_ShouldMoveObject()
        {
            grid.Insert(1, float2.zero);

            grid.Insert(1, new float2(10f, 10f));

            grid.Query(float2.zero, 1, out var results);

            Assert.IsFalse(results.Contains(1));

            grid.Query(new float2(10f, 10f), 1, out results);

            Assert.Contains(1, results);
        }

        [Test]
        public void Remove_ShouldRemoveObject()
        {
            grid.Insert(1, float2.zero);

            bool removed = grid.Remove(1);

            Assert.IsTrue(removed);

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(0, count);

            Assert.IsFalse(results.Contains(1));
        }

        [Test]
        public void Query_ShouldReturnZero_WhenEmpty()
        {
            int count = grid.Query(float2.zero, 1, out _);

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Insert_NewCell_ShouldRemoveFromOldCell()
        {
            grid.Insert(1, float2.zero);

            grid.Insert(1, new float2(10f, 10f));

            grid.Query(float2.zero, 1, out var oldResults);
            int count = grid.Query(new float2(10f, 10f), 1, out var newResults);

            Assert.IsFalse(oldResults.Contains(1), "Old cell still contains moved object.");
            Assert.IsTrue(newResults.Contains(1), "New cell does not contain moved object.");
            Assert.AreEqual(1, count, "Object duplicated after moving.");
        }

        [Test]
        public void Insert_MultipleUnits_SameCell()
        {
            grid.Insert(1, new float2(0.1f, 0.1f));
            grid.Insert(2, new float2(0.2f, 0.2f));
            grid.Insert(3, new float2(0.3f, 0.3f));

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(3, count);
            Assert.Contains(1, results);
            Assert.Contains(2, results);
            Assert.Contains(3, results);
        }

        [Test]
        public void Insert_MultipleUnits_DifferentCells()
        {
            grid.Insert(1, new float2(0, 0));
            grid.Insert(2, new float2(10, 10));
            grid.Insert(3, new float2(20, 20));

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(1, count);
            Assert.Contains(1, results);
        }

        [Test]
        public void Remove_NonExistent_ShouldReturnFalse()
        {
            bool removed = grid.Remove(999);

            Assert.IsFalse(removed);
        }

        [Test]
        public void Insert_MoveBackAndForth()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(1, new float2(10, 10));
            grid.Insert(1, float2.zero);

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(1, count);
            Assert.Contains(1, results);
        }

        [Test]
        public void Remove_Reinsert()
        {
            grid.Insert(1, float2.zero);
            grid.Remove(1);
            grid.Insert(1, float2.zero);

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(1, count);
            Assert.Contains(1, results);
        }

        [Test]
        public void Query_LargeRadius_ShouldReturnAll()
        {
            grid.Insert(1, new float2(-5, -5));
            grid.Insert(2, new float2(5, 5));
            grid.Insert(3, new float2(-5, 5));
            grid.Insert(4, new float2(5, -5));

            int count = grid.Query(float2.zero, 10, out var results);

            Assert.AreEqual(4, count);
        }

        [Test]
        public void Insert_SameUnit_MultipleTimes_DifferentCells()
        {
            grid.Insert(1, new float2(0, 0));
            grid.Insert(1, new float2(5, 5));
            grid.Insert(1, new float2(10, 10));

            grid.Query(new float2(0, 0), 1, out var r1);
            grid.Query(new float2(5, 5), 1, out var r2);
            grid.Query(new float2(10, 10), 1, out var r3);

            Assert.IsFalse(r1.Contains(1));
            Assert.IsFalse(r2.Contains(1));
            Assert.IsTrue(r3.Contains(1));
        }

        [Test]
        public void Stress_InsertRemove_1000Units()
        {
            int length = 1000;
            for (int i = 0; i < length; i++)
            {
                grid.Insert(i, new float2(i * 2f, i * 2f));
            }

            for (int i = 0; i < length; i += 2)
            {
                Assert.IsTrue(grid.Remove(i), $"Failed to remove {i}");
            }

            for (int i = 0; i < length; i++)
            {
                int count = grid.Query(new float2(i * 2f, i * 2f), 0.5f, out var results);
                if (i % 2 == 0)
                {
                    Assert.AreEqual(0, count, $"Unit {i} should be removed");
                }
                else
                {
                    Assert.AreEqual(1, count, $"Unit {i} should exist");
                    Assert.Contains(i, results);
                }
            }
        }

        [Test]
        public void Stress_MixedOperations()
        {
            for (int i = 0; i < 500; i++)
            {
                grid.Insert(i, new float2(i, i));
            }

            for (int i = 0; i < 250; i++)
            {
                grid.Remove(i);
            }

            for (int i = 250; i < 500; i++)
            {
                grid.Insert(i, new float2(i + 100, i + 100));
            }

            for (int i = 0; i < 250; i++)
            {
                grid.Insert(i, new float2(i + 200, i + 200));
            }

            int count = grid.Query(float2.zero, 1000, out var results);

            Assert.AreEqual(500, count);

            var resultSet = new HashSet<int>(results);
            for (int i = 0; i < 500; i++)
            {
                Assert.IsTrue(resultSet.Contains(i), $"Unit {i} missing from results");
            }
        }

        [Test]
        public void Query_NoDuplicates()
        {
            grid.Insert(1, new float2(0, 0));
            grid.Insert(2, new float2(0.1f, 0.1f));
            grid.Insert(3, new float2(0.2f, 0.2f));

            int count = grid.Query(float2.zero, 5, out var results);

            Assert.AreEqual(3, count);

            var unique = new HashSet<int>(results);
            Assert.AreEqual(results.Count, unique.Count, "Results contain duplicates");
        }

        [Test]
        public void Insert_LargeUnitIds()
        {
            grid.Insert(100000, float2.zero);
            grid.Insert(200000, new float2(0.1f, 0.1f));

            int count = grid.Query(float2.zero, 1, out var results);

            Assert.AreEqual(2, count);
            Assert.Contains(100000, results);
            Assert.Contains(200000, results);
        }

        [Test]
        public void Query_ResultListIsIndependent()
        {
            grid.Insert(1, float2.zero);

            grid.Query(float2.zero, 1, out var results1);
            results1.Add(999);

            grid.Query(float2.zero, 1, out var results2);

            Assert.IsFalse(results2.Contains(999), "Query result was mutated by caller");
        }

        [Test]
        public void Remove_HeadOfCell()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(2, new float2(0.1f, 0.1f));

            grid.Query(float2.zero, 1, out var before);
            Assert.AreEqual(2, before.Count);

            grid.Remove(2);

            grid.Query(float2.zero, 1, out var after);
            Assert.AreEqual(1, after.Count);
            Assert.Contains(1, after);
        }

        [Test]
        public void Remove_MiddleOfCell()
        {
            grid.Insert(1, new float2(0.0f, 0.0f));
            grid.Insert(2, new float2(0.1f, 0.1f));
            grid.Insert(3, new float2(0.2f, 0.2f));

            grid.Remove(2);

            grid.Query(float2.zero, 1, out var results);
            Assert.AreEqual(2, results.Count);
            Assert.Contains(1, results);
            Assert.Contains(3, results);
            Assert.IsFalse(results.Contains(2));
        }

        [Test]
        public void Remove_TailOfCell()
        {
            grid.Insert(1, new float2(0.0f, 0.0f));
            grid.Insert(2, new float2(0.1f, 0.1f));
            grid.Insert(3, new float2(0.2f, 0.2f));

            grid.Remove(1);

            grid.Query(float2.zero, 1, out var results);
            Assert.AreEqual(2, results.Count);
            Assert.Contains(2, results);
            Assert.Contains(3, results);
            Assert.IsFalse(results.Contains(1));
        }

        [Test]
        public void Remove_AllFromCell_ShouldClearCell()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(2, new float2(0.1f, 0.1f));

            grid.Remove(1);
            grid.Remove(2);

            grid.Query(float2.zero, 1, out var results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Remove_Reinsert_ShouldWork()
        {
            grid.Insert(1, float2.zero);
            grid.Remove(1);
            grid.Insert(1, new float2(5f, 5f));

            grid.Query(float2.zero, 1, out var r1);
            grid.Query(new float2(5f, 5f), 1, out var r2);

            Assert.AreEqual(0, r1.Count);
            Assert.AreEqual(1, r2.Count);
            Assert.Contains(1, r2);
        }

        [Test]
        public void Remove_InterleavedOperations()
        {
            grid.Insert(1, new float2(0, 0));
            grid.Insert(2, new float2(0.1f, 0.1f));
            grid.Insert(3, new float2(0.2f, 0.2f));
            grid.Insert(4, new float2(0.3f, 0.3f));
            grid.Insert(5, new float2(0.4f, 0.4f));

            grid.Remove(3);
            grid.Remove(1);
            grid.Remove(5);

            grid.Query(float2.zero, 1, out var results);
            Assert.AreEqual(2, results.Count);
            Assert.Contains(2, results);
            Assert.Contains(4, results);
        }

        [Test]
        public void Remove_NonExistent_ShouldNotCorruptGrid()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(2, new float2(0.1f, 0.1f));

            grid.Remove(999);

            grid.Query(float2.zero, 1, out var results);
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void Remove_Duplicate_ShouldReturnFalse()
        {
            grid.Insert(1, float2.zero);
            Assert.IsTrue(grid.Remove(1));
            Assert.IsFalse(grid.Remove(1));
        }
    }
}
