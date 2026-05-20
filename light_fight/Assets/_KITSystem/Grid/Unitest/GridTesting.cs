using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using _KITSystem.Utils;
using UnityEngine;

namespace _KITSystem.Grid.Unitest
{
    public class GridTesting
    {
        private IGridManager grid;

        private static bool Contains(int[] array, int count, int value)
        {
            for (int i = 0; i < count; i++)
            {
                if (array[i] == value) return true;
            }
            return false;
        }

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

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.IsTrue(count > 0);

            Assert.IsTrue(Contains(results, count, 1));
        }

        [Test]
        public void Insert_SameCell_ShouldNotDuplicate()
        {
            grid.Insert(1, float2.zero);

            grid.Insert(1, new float2(0.2f, 0.2f));

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Insert_NewCell_ShouldMoveObject()
        {
            grid.Insert(1, float2.zero);

            grid.Insert(1, new float2(10f, 10f));

            int count1 = grid.Query(float2.zero, new float2(1, 1), out var oldResults);

            Assert.IsFalse(Contains(oldResults, count1, 1));

            int count2 = grid.Query(new float2(10f, 10f), new float2(1, 1), out var newResults);

            Assert.IsTrue(Contains(newResults, count2, 1));
        }

        [Test]
        public void Remove_ShouldRemoveObject()
        {
            grid.Insert(1, float2.zero);

            bool removed = grid.Remove(1);

            Assert.IsTrue(removed);

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.AreEqual(0, count);

            Assert.IsFalse(Contains(results, count, 1));
        }

        [Test]
        public void Query_ShouldReturnZero_WhenEmpty()
        {
            int count = grid.Query(float2.zero, new float2(1, 1), out _);

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Insert_NewCell_ShouldRemoveFromOldCell()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(1, new float2(10f, 10f));

            int count1 = grid.Query(float2.zero, new float2(1, 1), out var oldResults);
            int count2 = grid.Query(new float2(10f, 10f), new float2(1, 1), out var newResults);

            Assert.IsFalse(Contains(oldResults, count1, 1), "Old cell still contains moved object.");
            Assert.IsTrue(Contains(newResults, count2, 1), "New cell does not contain moved object.");
            Assert.AreEqual(1, count2, "Object duplicated after moving.");
        }

        [Test]
        public void Insert_MultipleUnits_SameCell()
        {
            grid.Insert(1, new float2(0.1f, 0.1f));
            grid.Insert(2, new float2(0.2f, 0.2f));
            grid.Insert(3, new float2(0.3f, 0.3f));

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.AreEqual(3, count);
            Assert.IsTrue(Contains(results, count, 1));
            Assert.IsTrue(Contains(results, count, 2));
            Assert.IsTrue(Contains(results, count, 3));
        }

        [Test]
        public void Insert_MultipleUnits_DifferentCells()
        {
            grid.Insert(1, new float2(0, 0));
            grid.Insert(2, new float2(10, 10));
            grid.Insert(3, new float2(20, 20));

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.AreEqual(1, count);
            Assert.IsTrue(Contains(results, count, 1));
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

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.AreEqual(1, count);
            Assert.IsTrue(Contains(results, count, 1));
        }

        [Test]
        public void Query_LargeRadius_ShouldReturnAll()
        {
            grid.Insert(1, new float2(-5, -5));
            grid.Insert(2, new float2(5, 5));
            grid.Insert(3, new float2(-5, 5));
            grid.Insert(4, new float2(5, -5));

            int count = grid.Query(float2.zero, new float2(10, 10), out var results);

            Assert.AreEqual(4, count);
        }

        [Test]
        public void Insert_SameUnit_MultipleTimes_DifferentCells()
        {
            grid.Insert(1, new float2(0, 0));
            grid.Insert(1, new float2(5, 5));
            grid.Insert(1, new float2(10, 10));

            int rc1 = grid.Query(new float2(0, 0), new float2(1, 1), out var r1);
            int rc2 = grid.Query(new float2(5, 5), new float2(1, 1), out var r2);
            int rc3 = grid.Query(new float2(10, 10), new float2(1, 1), out var r3);

            Assert.IsFalse(Contains(r1, rc1, 1));
            Assert.IsFalse(Contains(r2, rc2, 1));
            Assert.IsTrue(Contains(r3, rc3, 1));
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
                int count = grid.Query(new float2(i * 2f, i * 2f), new float2(0.5f, 0.5f), out var results);
                if (i % 2 == 0)
                {
                    Assert.AreEqual(0, count, $"Unit {i} should be removed");
                }
                else
                {
                    Assert.AreEqual(1, count, $"Unit {i} should exist");
                    Assert.IsTrue(Contains(results, count, i));
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

            int count = grid.Query(float2.zero, new float2(1000, 1000), out var results);

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

            int count = grid.Query(float2.zero, new float2(5, 5), out var results);

            Assert.AreEqual(3, count);

            var unique = new HashSet<int>();
            foreach (var r in results)
            {
                if (r > 0) unique.Add(r);
            }
            Assert.AreEqual(count, unique.Count, "Results contain duplicates");
        }

        [Test]
        public void Insert_LargeUnitIds()
        {
            grid.Insert(100000, float2.zero);
            grid.Insert(200000, new float2(0.1f, 0.1f));

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);

            Assert.AreEqual(2, count);
            Assert.IsTrue(Contains(results, count, 100000));
            Assert.IsTrue(Contains(results, count, 200000));
        }

        [Test]
        public void Remove_HeadOfCell()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(2, new float2(0.1f, 0.1f));

            int c1 = grid.Query(float2.zero, new float2(1, 1), out var before);
            Assert.AreEqual(2, c1);

            grid.Remove(2);

            int c2 = grid.Query(float2.zero, new float2(1, 1), out var after);
            
            Assert.AreEqual(1, c2);
            Assert.IsTrue(Contains(after, c2, 1));
        }

        [Test]
        public void Remove_MiddleOfCell()
        {
            grid.Insert(1, new float2(0.0f, 0.0f));
            grid.Insert(2, new float2(0.1f, 0.1f));
            grid.Insert(3, new float2(0.2f, 0.2f));

            grid.Remove(2);

            int count = grid.Query(float2.zero, new float2(1, 1), out var results);
            Assert.AreEqual(2, count);
            Assert.IsTrue(Contains(results, count, 1));
            Assert.IsTrue(Contains(results, count, 3));
            Assert.IsFalse(Contains(results, count, 2));
        }

        [Test]
        public void Remove_TailOfCell()
        {
            grid.Insert(1, new float2(0.0f, 0.0f));
            grid.Insert(2, new float2(0.1f, 0.1f));
            grid.Insert(3, new float2(0.2f, 0.2f));

            grid.Remove(1);

            int c = grid.Query(float2.zero, new float2(1, 1), out var results);
            
            Assert.AreEqual(2, c);
            Assert.IsTrue(Contains(results, c, 2));
            Assert.IsTrue(Contains(results, c, 3));
            Assert.IsFalse(Contains(results, c, 1));
        }

        [Test]
        public void Remove_AllFromCell_ShouldClearCell()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(2, new float2(0.1f, 0.1f));

            grid.Remove(1);
            grid.Remove(2);

            int c = grid.Query(float2.zero, new float2(1, 1), out var results);
            Assert.AreEqual(0, c);
        }

        [Test]
        public void Remove_Reinsert_ShouldWork()
        {
            grid.Insert(1, float2.zero);
            grid.Remove(1);
            grid.Insert(1, new float2(5f, 5f));

            int c1 = grid.Query(float2.zero, new float2(1, 1), out var r1);
            int c2 = grid.Query(new float2(5f, 5f), new float2(1, 1), out var r2);

            Assert.AreEqual(0, c1);
            Assert.AreEqual(1, c2);
            Assert.IsTrue(Contains(r2, c2, 1));
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

            int c = grid.Query(float2.zero, new float2(1, 1), out var results);
            Assert.AreEqual(2, c);
            Assert.IsTrue(Contains(results, c, 2));
            Assert.IsTrue(Contains(results, c, 4));
        }

        [Test]
        public void Remove_NonExistent_ShouldNotCorruptGrid()
        {
            grid.Insert(1, float2.zero);
            grid.Insert(2, new float2(0.1f, 0.1f));

            grid.Remove(999);

            int c = grid.Query(float2.zero, new float2(1, 1), out var results);
            Assert.AreEqual(2, c);
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