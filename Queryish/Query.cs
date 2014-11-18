using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Queryish
{
    enum PropertyType
    {
        date,
        number,
        text,
        boolean,
        integer
    }

    internal class Query<T>
    {
        protected Dictionary<string, PropertyType> dataTypeMap;
        #region Constructor
        public Query()
        {
            dataTypeMap = new Dictionary<string, PropertyType>();
            foreach (var property in typeof(T).GetProperties())
            {
                string name = property.Name;
                string type = property.PropertyType.Name;
                if(type == "Float" || type == "Decimal")
                    dataTypeMap.Add(name.ToLower(), PropertyType.number);
                else if(type == "Int32")
                    dataTypeMap.Add(name.ToLower(), PropertyType.integer);
                else if (type == "DateTime")
                    dataTypeMap.Add(name.ToLower(), PropertyType.date);
                else if (type == "Boolean")
                    dataTypeMap.Add(name.ToLower(), PropertyType.boolean);
                else
                    dataTypeMap.Add(name.ToLower(), PropertyType.text);
            }

        }
        #endregion Constructor

        #region Methods
        internal Expression BuildSearchFunction(FilterItem search, ParameterExpression fieldParameter)
        {
            Type dataType = typeof(string);
            ConstantExpression searchExpression = null;
            string searchColumn = search.Column.ToLower();
            PropertyType searchType = dataTypeMap[searchColumn];
            if (searchType == PropertyType.date)
            {
                dataType = typeof(DateTime);
                searchExpression = Expression.Constant(Convert.ToDateTime(search.Value), dataType);
            }
            else if (searchType == PropertyType.integer)
            {
                dataType = typeof(int);
                searchExpression = Expression.Constant(Convert.ToInt32(search.Value), dataType);
            }
            else if (searchType == PropertyType.number)
            {
                dataType = typeof(decimal);
                searchExpression = Expression.Constant(Convert.ToDecimal(search.Value), dataType);
            }
            else if (searchType == PropertyType.boolean)
            {
                dataType = typeof(bool);
                searchExpression = Expression.Constant(Convert.ToBoolean(search.Value), dataType);
            }
            else
            {
                searchExpression = Expression.Constant(search.Value, dataType);
            }
            return BuildSearchExpression(search.Operation, fieldParameter, search.Column, searchExpression, dataType);
        }

        private Expression BuildSearchExpression(string operation, ParameterExpression fieldParameter, string column, ConstantExpression searchExpression, Type dataType)
        {
            Expression searchOperation = Expression.Equal(Expression.Property(fieldParameter, column), searchExpression);

            if (operation.Equals("Not", StringComparison.CurrentCultureIgnoreCase))
            {
                searchOperation = Expression.NotEqual(Expression.Property(fieldParameter, column), searchExpression);
            }
            else if (operation.Equals(">=") && dataType != typeof(string))
            {
                searchOperation = Expression.GreaterThanOrEqual(Expression.Property(fieldParameter, column), searchExpression);
            }
            else if (operation.Equals("<=") && dataType != typeof(string))
            {
                searchOperation = Expression.LessThanOrEqual(Expression.Property(fieldParameter, column), searchExpression);
            }
            else if (operation.Equals(">") && dataType != typeof(string))
            {
                searchOperation = Expression.GreaterThan(Expression.Property(fieldParameter, column), searchExpression);
            }
            else if (operation.Equals("<") && dataType != typeof(string))
            {
                searchOperation = Expression.LessThan(Expression.Property(fieldParameter, column), searchExpression);
            }
            else if (operation.Equals("Like", StringComparison.CurrentCultureIgnoreCase) && dataType == typeof(string))
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                searchOperation = Expression.Call(Expression.Property(fieldParameter, column), method, searchExpression);                
            }
            else if (operation.Equals("Not Like", StringComparison.CurrentCultureIgnoreCase) && dataType == typeof(string))
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                searchOperation = Expression.Not(Expression.Call(Expression.Property(fieldParameter, column), method, searchExpression));
            }
            else if ((operation.Equals(">=") || operation.Equals("<=") || operation.Equals(">") || operation.Equals("<")) && dataType == typeof(string))
            {
                MethodInfo method = typeof(string).GetMethod("CompareTo",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    CallingConventions.Any,
                    new[] { typeof(string) },
                    null);
                var compareToOperation = Expression.Call(Expression.Property(fieldParameter, column), method, searchExpression);

                Expression zeroConstant = Expression.Constant(0, typeof(int));
                if (operation.Equals(">="))
                {
                    searchOperation = Expression.GreaterThanOrEqual(zeroConstant, compareToOperation);
                }
                else if (operation.Equals(">"))
                {
                    searchOperation = Expression.GreaterThan(zeroConstant, compareToOperation);
                }
                else if (operation.Equals("<="))
                {
                    searchOperation = Expression.LessThanOrEqual(zeroConstant, compareToOperation);
                }
                else if (operation.Equals("<"))
                {
                    searchOperation = Expression.LessThan(zeroConstant, compareToOperation);
                }
            }

            return searchOperation;
        }
        #endregion Methods
    }
}
