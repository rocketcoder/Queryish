using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queryish
{
    public class FilterItem
    {
        public string Value { get; set; }

        public string Column { get; set; }

        public string Operation { get; set; }

        public FilterItem(string column, string value, string operation)
        {
            this.Column = column;
            this.Value = value;
            this.Operation = operation;
        }

    }
}
