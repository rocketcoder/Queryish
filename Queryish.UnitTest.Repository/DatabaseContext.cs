using Queryish.UnitTest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queryish.UnitTest.Repository
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TestCase> TestCases { get; set; }

        public DatabaseContext(string connectionString)
            : base(connectionString)
        {
            
        }

        public DatabaseContext()
        {

        }
    }
}
