using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Queryish
{
    public class QueryFactory<T>
    {
        private static ConcurrentDictionary<string, QueryTree> ExpressionCache = new ConcurrentDictionary<string, QueryTree>();        
        private Query<T> searcher;

        public QueryFactory() 
        {
            searcher = new Query<T>();
        }
        public IQueryable<T> Queryable<T>(IQueryable<T> dataSet, Filter filter, string sortProperty, bool overridePagination = false)
        {
            #region README
            /**********************************************************************************************************
             * This function turns a Filter into a Linq query and returns an IQueryable<EBillView>.  When we write 
             * Linq queries we are really writing Expression Trees that get translated into SQL.  This is great if you 
             * know what the query is at design time - but if you don't know the query until run time then you are kinda
             * stuck.  That is why this class exists.  It turns a Filter into an Expression Tree and then to SQL
             ******************************************************************************************************* */
            #endregion README            
            //Sanity Check - Provide sane defaults
            if (filter.Page <= 1)
                filter.Page = 1;

            if (filter.Size < 1)
                filter.Size = 100;

            if (string.IsNullOrEmpty(filter.OrderColumn))
                filter.OrderColumn = sortProperty;

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            //used as a key in the ExpressionCache
            string json = JsonConvert.SerializeObject(filter);

            ParameterExpression whereParameter = null;
            ParameterExpression orderByParameter = null;
            ParameterExpression thenByParameter = null;
            Expression searchExpression = null;
            Expression orderExpression = null;
            Expression thenExpression = null;
            Expression skipExpression = null;
            Expression takeExpression = null;

            //Is this Filter in the cache?  Then use that expression tree instead of making another one
            if (!ExpressionCache.ContainsKey(json))
            {
                whereParameter = parameter;
                orderByParameter = Expression.Parameter(typeof(T), "y");
                thenByParameter = Expression.Parameter(typeof(T), "z");
                searchExpression = BuildQueryExpression(filter, whereParameter);
                orderExpression = BuildOrderByExpression(filter, orderByParameter);
                thenExpression = BuildThenByExpression(thenByParameter, sortProperty);
                skipExpression = BuildSkipExpression(filter);
                takeExpression = BuildTakeExpression(filter);

                QueryTree searchTree = new QueryTree()
                {
                    WhereParameter = whereParameter,
                    OrderByParameter = orderByParameter,
                    ThenByParameter = thenByParameter,
                    SearchExpression = searchExpression,
                    OrderExpression = orderExpression,
                    ThenExpression = thenExpression,
                    SkipExpression = skipExpression,
                    TakeExpression = takeExpression
                };
                ExpressionCache.AddOrUpdate(json, searchTree, (x, y) => { return y; });
            }
            else
            {
                QueryTree searchTree = ExpressionCache[json];
                whereParameter = searchTree.WhereParameter;
                orderByParameter = searchTree.OrderByParameter;
                thenByParameter = searchTree.ThenByParameter;
                searchExpression = searchTree.SearchExpression;
                orderExpression = searchTree.OrderExpression;
                thenExpression = searchTree.ThenExpression;
                if (!overridePagination)
                {
                    skipExpression = searchTree.SkipExpression;
                    takeExpression = searchTree.TakeExpression;
                }
            }
            MethodCallExpression finalQuery = null;
            MethodCallExpression where = BuildWhereMethodCallExpression<T>(dataSet, whereParameter, searchExpression);
            MethodCallExpression whereOrder = BuildOrderByMethodCallExpression<T>(dataSet, where, orderByParameter, orderExpression, filter.OrderDirection);
            //MethodCallExpression whereOrderThen = BuildThenByMethodCallExpression<T>(dataSet, whereOrder, thenByParameter, thenExpression);
            finalQuery = whereOrder;
            if (!overridePagination)
            {
                MethodCallExpression whereOrderThenSkip = BuildSkipMethodCallExpression<T>(dataSet, finalQuery, skipExpression);
                MethodCallExpression whereOrderThenSkipTake = BuildTakeMethodCallExpression<T>(dataSet, whereOrderThenSkip, takeExpression);
                finalQuery = whereOrderThenSkipTake;
            }
            // Create an executable query from the expression tree.
            var query = dataSet.Provider.CreateQuery<T>(finalQuery);
            return query;
            
        }

        private static MethodCallExpression BuildWhereMethodCallExpression<T>(IQueryable<T> dataSet, ParameterExpression parameter, Expression searchExpression)
        {
            if (searchExpression == null)
                searchExpression = Expression.Equal(Expression.Constant(true), Expression.Constant(true));

            return Expression.Call(
                typeof(Queryable), "Where",
                new Type[] { dataSet.ElementType },
                dataSet.Expression,
                Expression.Lambda<Func<T, bool>>(searchExpression, new ParameterExpression[] { parameter }));
        }

        private static MethodCallExpression BuildOrderByMethodCallExpression<T>(IQueryable<T> dataSet, MethodCallExpression where, ParameterExpression parameter, Expression orderExpression, string sortDirection)
        {
            bool sortAscending = true;
            if (!string.IsNullOrEmpty(sortDirection) && sortDirection.ToLower() == "desc")
                sortAscending = false;
            
            Type[] orderType = null;
            Expression lambda = null;

            string methodName = "OrderBy";
            if(sortAscending)
                methodName = "OrderByDescending";

            if (orderExpression.Type == typeof(DateTime))
            {
                orderType = new Type[] { dataSet.ElementType, typeof(DateTime) };
                lambda = BuildLambdaExpression<DateTime, T>(orderExpression, parameter);
            }
            else if (orderExpression.Type == typeof(int))
            {
                orderType = new Type[] { dataSet.ElementType, typeof(int) };
                lambda = BuildLambdaExpression<int, T>(orderExpression, parameter);
            }
            else if (orderExpression.Type == typeof(double))
            {
                orderType = new Type[] { dataSet.ElementType, typeof(double) };
                lambda = BuildLambdaExpression<double, T>(orderExpression, parameter);
            }
            else if (orderExpression.Type == typeof(decimal))
            {
                orderType = new Type[] { dataSet.ElementType, typeof(decimal) };
                lambda = BuildLambdaExpression<decimal, T>(orderExpression, parameter);
            }
            else
            {
                orderType = new Type[] { dataSet.ElementType, typeof(string) };
                lambda = BuildLambdaExpression<string, T>(orderExpression, parameter);
            }

            return  Expression.Call(
              typeof(Queryable),
              methodName,
              orderType,
              where,
              lambda);
        }

        private static Expression BuildLambdaExpression<U, T>(Expression orderExpression, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<T, U>>(orderExpression, new ParameterExpression[] { parameter });
        }

        private static MethodCallExpression BuildThenByMethodCallExpression<T>(IQueryable<T> dataSet, MethodCallExpression whereOrder, ParameterExpression parameter, Expression orderExpression)
        {
            return Expression.Call(
              typeof(Queryable),
              "ThenBy",
              new Type[] { dataSet.ElementType, typeof(string) },
              whereOrder,
              Expression.Lambda<Func<T, string>>(orderExpression, new ParameterExpression[] { parameter }));
        }

        private static MethodCallExpression BuildSkipMethodCallExpression<T>(IQueryable<T> dataSet, MethodCallExpression whereOrderThen, Expression skipExpression)
        {
            return Expression.Call(
              typeof(Queryable),
              "Skip",
              new Type[] { dataSet.ElementType },
              whereOrderThen,
              skipExpression);
        }

        private static MethodCallExpression BuildTakeMethodCallExpression<T>(IQueryable<T> dataSet, MethodCallExpression whereOrderThenSkip, Expression takeExpression)
        {
            return Expression.Call(
              typeof(Queryable),
              "Take",
              new Type[] { dataSet.ElementType },
              whereOrderThenSkip,
              takeExpression);
        }

        private Expression BuildQueryExpression(Filter filter, ParameterExpression parameter)
        {
            List<Expression> ands = new List<Expression>();
            Dictionary<string, List<FilterItem>> filters = new Dictionary<string, List<FilterItem>>();

            //Build the Dictionary.  columns will be or'd and then and'd
            foreach (var search in filter.FilterCollection)
            {
                string key = search.Column + search.Operation;
                if (!filters.Keys.Contains(key))
                {
                    List<FilterItem> searchCollection = new List<FilterItem>();
                    searchCollection.Add(search);
                    filters.Add(key, searchCollection);
                }
                else
                {
                    filters[key].Add(search);
                }
            }
            //Create or expressions
            foreach (var item in filters)
            {
                if (item.Value.Count > 0)
                {
                    ands.Add(BuildOrExpression(item.Value, parameter));
                }
            }
            //Create and expression
            Expression searchExpression = BuildAndExpression(ands);
            return searchExpression;
        }

        private Expression BuildOrExpression(List<FilterItem> searchList, ParameterExpression parameter)
        {
            Expression expression = null;
            foreach (var searchItem in searchList)
            {
                var searchExpression = searcher.BuildSearchFunction(searchItem, parameter);
                if (expression == null)
                {
                    expression = searchExpression;
                }
                else
                {
                    if (searchItem.Operation.Equals("Not", StringComparison.InvariantCultureIgnoreCase))
                    {
                        expression = Expression.And(expression, searchExpression);
                    }
                    else
                    {
                        expression = Expression.Or(expression, searchExpression);
                    }
                }
            }
            return expression;
        }

        private Expression BuildAndExpression(List<Expression> andList)
        {
            Expression expression = null;
            foreach (var and in andList)
            {
                if (expression == null)
                {
                    expression = and;
                }
                else
                {
                    expression = Expression.And(expression, and);
                }
            }
            return expression;
        }

        private Expression BuildOrderByExpression(Filter filter, ParameterExpression orderParameter)
        {      
            string orderColumn = filter.OrderColumn;
            return Expression.Property(orderParameter, orderColumn);
        }

        private Expression BuildThenByExpression(ParameterExpression orderParameter, string orderColumn)
        {
            return Expression.Property(orderParameter, orderColumn);
        }

        private Expression BuildSkipExpression(Filter filter)
        {
            return Expression.Constant((filter.Page -1 ) * filter.Size);
        }

        private Expression BuildTakeExpression(Filter filter)
        {
            return Expression.Constant(filter.Size);
        }
    }
}
