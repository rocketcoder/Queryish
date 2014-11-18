using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queryish
{
    /// <summary>
    /// A Search Tree is an encapsulation of all of the Expressions used to query from the EnvoicerDataStore    
    /// </summary>
    internal class QueryTree
    {
        internal System.Linq.Expressions.ParameterExpression OrderByParameter { get; set; }

        internal System.Linq.Expressions.ParameterExpression ThenByParameter { get; set; }

        internal System.Linq.Expressions.Expression SearchExpression { get; set; }

        internal System.Linq.Expressions.Expression OrderExpression { get; set; }

        internal System.Linq.Expressions.Expression ThenExpression { get; set; }

        internal System.Linq.Expressions.Expression TakeExpression { get; set; }

        internal System.Linq.Expressions.Expression SkipExpression { get; set; }

        internal System.Linq.Expressions.ParameterExpression WhereParameter { get; set; }
    }
}
