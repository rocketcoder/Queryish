using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Queryish
{
    public class Filter
    {
        public string OrderColumn { get; set; }

        public int Size { get; set; }

        public int Page { get; set; }

        public string OrderDirection { get; set; }
        public List<FilterItem> FilterCollection { get; set; }

        public Filter()
        {
            FilterCollection = new List<FilterItem>();
        }
    }
}
