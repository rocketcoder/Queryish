using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queryish.UnitTest.Repository.Models
{
    public class TestCase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Score { get; set; }
        public DateTime ScoreDate { get; set; }
        public bool Enabled { get; set; }
    }
}
