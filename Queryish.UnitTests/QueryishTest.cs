using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Queryish.UnitTest.Repository;
using System.Configuration;
using System.Linq;
using Queryish.UnitTest.Repository.Models;
using System.Collections.Generic;

namespace Queryish.UnitTests
{
    [TestClass]
    public class QueryishTest
    {

        public static string connectionString = "";
        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            connectionString = ConfigurationManager.AppSettings["DatabaseContext"];
            using (DatabaseContext context = new DatabaseContext(connectionString))
            {
                context.TestCases.Add(new TestCase() { Enabled = false, Name = "Jan", Score = Convert.ToDecimal(1.1), ScoreDate = new DateTime(2014, 1, 1) });
                context.TestCases.Add(new TestCase() { Enabled = false, Name = "Feb", Score = Convert.ToDecimal(2.2), ScoreDate = new DateTime(2014, 2, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "March", Score = Convert.ToDecimal(3.3), ScoreDate = new DateTime(2014, 3, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "April", Score = Convert.ToDecimal(4.1), ScoreDate = new DateTime(2014, 4, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "May", Score = Convert.ToDecimal(5.1), ScoreDate = new DateTime(2014, 5, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "June", Score = Convert.ToDecimal(6.1), ScoreDate = new DateTime(2014, 6, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "July", Score = Convert.ToDecimal(7.1), ScoreDate = new DateTime(2014, 7, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "Aug", Score = Convert.ToDecimal(8.1), ScoreDate = new DateTime(2014, 8, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "Sept", Score = Convert.ToDecimal(9.1), ScoreDate = new DateTime(2014, 9, 1) });
                context.TestCases.Add(new TestCase() { Enabled = true, Name = "Oct", Score = Convert.ToDecimal(10.1), ScoreDate = new DateTime(2014, 10, 1) });
                context.TestCases.Add(new TestCase() { Enabled = false, Name = "Nov", Score = Convert.ToDecimal(11.1), ScoreDate = new DateTime(2014, 11, 1) });
                context.TestCases.Add(new TestCase() { Enabled = false, Name = "Dec", Score = Convert.ToDecimal(12.1), ScoreDate = new DateTime(2014, 12, 1) });
                context.SaveChanges();
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            using (DatabaseContext context = new DatabaseContext(connectionString))
            {
                context.Database.ExecuteSqlCommand("Truncate Table TestCases");
            }
        }

        public void RunTest(Action<List<TestCase>>assert, Filter filter)
        {
            using (DatabaseContext context = new DatabaseContext(connectionString))
            {
                IQueryable<TestCase> query = context.TestCases.AsQueryable();
                var queryMaker = new QueryFactory<TestCase>();
                var data = queryMaker.Queryable<TestCase>(query, filter, "Id");
                assert(data.ToList());
            }

        }

        [TestMethod]
        public void ShouldGetAllRecords()
        {
            Filter filter = new Filter();
            Action <List<TestCase>> assert = (x) => Assert.AreEqual(12, x.Count());
            RunTest(assert, filter);
        }
        [TestMethod]
        public void ShouldGetFirst5Records()
        {
            Filter filter = new Filter();
            filter.Size = 5;
            filter.Page = 1;
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(5, x.Count());
            RunTest(assert, filter);
        }
        [TestMethod]
        public void ShouldGetNext5Records()
        {
            Filter filter = new Filter();
            filter.Size = 5;
            filter.Page = 2;
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(5, x.Count());
            RunTest(assert, filter);
        }
        [TestMethod]
        public void ShouldGetLast2Records()
        {
            Filter filter = new Filter();
            filter.Size = 5;
            filter.Page = 3;
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(2, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetJuneJulyAug()
        {
            Filter filter = new Filter();            
            filter.FilterCollection.Add(new FilterItem("Name", "June", "=="));
            filter.FilterCollection.Add(new FilterItem("Name", "July", "=="));
            filter.FilterCollection.Add(new FilterItem("Name", "Aug", "=="));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(3, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetJuneOrJulyAndScoreGreaterThan7()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("Name", "June", "=="));
            filter.FilterCollection.Add(new FilterItem("Name", "July", "=="));
            filter.FilterCollection.Add(new FilterItem("Score", "7", ">="));            
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(1, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetNameLikeJ()
        {
            Filter filter = new Filter();           
            filter.FilterCollection.Add(new FilterItem("Name", "J", "Like"));            
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(3, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetNotEnabled()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("Enabled", "true", "Not"));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(4, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetEnabled()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("Enabled", "true", "=="));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(8, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetGreaterThanOct()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("ScoreDate", "10/1/2014", ">"));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(2, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetGreaterThanOrEqualToOct()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("ScoreDate", "10/1/2014", ">="));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(3, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetLessThanOct()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("ScoreDate", "10/1/2014", "<"));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(9, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetLessThanOrEqualToOct()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("ScoreDate", "10/1/2014", "<="));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(10, x.Count());
            RunTest(assert, filter);
        }

        [TestMethod]
        public void ShouldGetScoreEqual0()
        {
            Filter filter = new Filter();
            filter.FilterCollection.Add(new FilterItem("Score", "0", "=="));
            Action<List<TestCase>> assert = (x) => Assert.AreEqual(0, x.Count());
            RunTest(assert, filter);
        }
        
    }
}
