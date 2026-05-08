using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace _KITSystem.Grid.Unitest
{
    public class GridTesting
    {
        private IGridManager grid;

        [SetUp]
        public void Setup()
        {
            grid = new FixedUniformGrid(20, 20, 1, 1024);
        }

        [Test]
        public void Insert_ShouldAddObject()
        {
            bool inserted = grid.Insert(
                1,
                Vector3.zero);

            Assert.IsTrue(inserted);

            int count = grid.Query(Vector3.zero, 1, out var results);

            Assert.IsTrue(count > 0);

            Assert.Contains(1, results);
        }

        [Test]
        public void Insert_SameCell_ShouldNotDuplicate()
        {
            grid.Insert(1, Vector3.zero);

            grid.Insert(
                1,
                new Vector3(0.2f, 0f, 0.2f));

            int count = grid.Query(Vector3.zero, 1, out var results);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Insert_NewCell_ShouldMoveObject()
        {
            grid.Insert(1, Vector3.zero);

            grid.Insert(
                1,
                new Vector3(10f, 0f, 10f));

            List<int> results;
                
            grid.Query(Vector3.zero, 1, out results);

            Assert.IsFalse(results.Contains(1));
            
            grid.Query(new Vector3(10f, 0f, 10f), 1, out results);

            Assert.Contains(1, results);
        }

        [Test]
        public void Remove_ShouldRemoveObject()
        {
            grid.Insert(1, Vector3.zero);

            bool removed = grid.Remove(1);

            Assert.IsTrue(removed);

            int count;
            List<int> results;
                
            count = grid.Query(Vector3.zero, 1, out results);

            Assert.AreEqual(count, 0);

            Assert.IsFalse(results.Contains(1));
        }

        [Test]
        public void Query_ShouldReturnFalse_WhenEmpty()
        {
            int count;

            count = grid.Query(Vector3.zero, 1, out _);

            Assert.AreEqual(count, 0);
        }

        [Test]
        public void Insert_NewCell_ShouldRemoveFromOldCell()
        {
            // arrange
            grid.Insert(
                1,
                Vector3.zero);

            // move sang cell khác
            grid.Insert(
                1,
                new Vector3(10f, 0f, 10f));

            List<int> results;
            
            // act

            grid.Query(Vector3.zero, 1, out results);
            
            List<int> oldResults = new(results);
            
            int count = grid.Query(new Vector3(10f, 0f, 10f), 1, out results);

            List<int> newResults = new(results);

            // assert
            Assert.IsFalse(
                oldResults.Contains(1),
                "Old cell still contains moved object.");

            Assert.IsTrue(
                newResults.Contains(1),
                "New cell does not contain moved object.");

            Assert.AreEqual(
                1,
                count,
                "Object duplicated after moving.");
        }
    }
}