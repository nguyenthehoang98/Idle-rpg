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
            grid = new FixedUniformGrid(1);
        }

        [Test]
        public void Insert_ShouldAddObject()
        {
            bool inserted = grid.Insert(
                1,
                Vector3.zero);

            Assert.IsTrue(inserted);

            List<int> results = new();

            bool found = grid.Query(
                Vector3.zero,
                1f,
                results);

            Assert.IsTrue(found);

            Assert.Contains(1, results);
        }

        [Test]
        public void Insert_SameCell_ShouldNotDuplicate()
        {
            grid.Insert(1, Vector3.zero);

            grid.Insert(
                1,
                new Vector3(0.2f, 0f, 0.2f));

            List<int> results = new();

            grid.Query(
                Vector3.zero,
                1f,
                results);

            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void Insert_NewCell_ShouldMoveObject()
        {
            grid.Insert(1, Vector3.zero);

            grid.Insert(
                1,
                new Vector3(10f, 0f, 10f));

            List<int> results = new();

            grid.Query(
                Vector3.zero,
                1f,
                results);

            Assert.IsFalse(results.Contains(1));

            results.Clear();

            grid.Query(
                new Vector3(10f, 0f, 10f),
                1f,
                results);

            Assert.Contains(1, results);
        }

        [Test]
        public void Remove_ShouldRemoveObject()
        {
            grid.Insert(1, Vector3.zero);

            bool removed = grid.Remove(1);

            Assert.IsTrue(removed);

            List<int> results = new();

            bool found = grid.Query(
                Vector3.zero,
                1f,
                results);

            Assert.IsFalse(found);

            Assert.IsFalse(results.Contains(1));
        }

        [Test]
        public void Query_ShouldReturnFalse_WhenEmpty()
        {
            List<int> results = new();

            bool found = grid.Query(
                Vector3.zero,
                1f,
                results);

            Assert.IsFalse(found);

            Assert.AreEqual(0, results.Count);
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

            // act
            List<int> oldResults = new();

            grid.Query(
                Vector3.zero,
                1f,
                oldResults);

            List<int> newResults = new();

            grid.Query(
                new Vector3(10f, 0f, 10f),
                1f,
                newResults);

            // assert
            Assert.IsFalse(
                oldResults.Contains(1),
                "Old cell still contains moved object.");

            Assert.IsTrue(
                newResults.Contains(1),
                "New cell does not contain moved object.");

            Assert.AreEqual(
                1,
                newResults.Count,
                "Object duplicated after moving.");
        }
    }
}