using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShivOhm.Infrastructure
{
    public class Repository<TEntity> where TEntity : class
    {
        //internal TDbContext dbcontext;
        internal GenericDbContext dbcontext;
        internal DbSet<TEntity> dbSet;

        public Repository(GenericDbContext context)
        {
            // dbcontext = context;
            dbcontext = context;
            dbSet = context.Set<TEntity>();
        }
        public virtual void Add(TEntity entity)
        {
            try
            {
                dbSet.Add(entity);
                dbcontext.SaveChanges();
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual void Update(TEntity entity, string columns)
        {
            try
            {
                if (columns != null)
                {
                    var dbEntry = dbcontext.Entry(entity);
                    var includeProperties = columns.Split(',');
                    foreach (var includeProperty in includeProperties)
                    {
                        dbEntry.Property(includeProperty).IsModified = true;
                    }
                }
                dbSet.Update(entity);
                dbcontext.SaveChanges();
            }
            catch (Exception Ex)
            {

                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual void Update(TEntity entity, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            try
            {
                if (includeProperties != null)
                {
                    var dbEntry = dbcontext.Entry(entity);
                    foreach (var includeProperty in includeProperties)
                    {
                        dbEntry.Property(includeProperty).IsModified = true;
                    }
                }

                dbSet.Update(entity);
                dbcontext.SaveChanges();
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual bool SoftDelete(object id)
        {
            try
            {
                TEntity entityToDelete = dbSet.Find(id);
                if (dbcontext.Entry(entityToDelete).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToDelete);
                }
                dbSet.Remove(entityToDelete);
                dbcontext.SaveChanges();
                return true;
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual bool Delete(object id)
        {
            try
            {
                TEntity entityToDelete = dbSet.Find(id);
                if (dbcontext.Entry(entityToDelete).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToDelete);
                }
                dbSet.Remove(entityToDelete);
                dbcontext.SaveChanges();
                return true;
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual TEntity ReadOne(object id)
        {
            try
            {
                return dbcontext.Set<TEntity>().Find(id);
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual List<TEntity> ReadAll(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            try
            {
                IQueryable<TEntity> query = dbSet;

                if (filter != null)
                    query = query.Where(filter);

                if (orderBy != null)
                    query = orderBy(query);

                return query.ToList();
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public virtual TEntity ReadOne(Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                IQueryable<TEntity> query = dbSet;
                return query.FirstOrDefault(filter);
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public PaginationModel<TEntity> ReadAll(int page, int pageSize, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, Expression<Func<TEntity, bool>> filter = null)
        {
            try
            {
                IQueryable<TEntity> query = dbcontext.Set<TEntity>().AsQueryable();

                int totalCount = query.Count();
                int filteredCount = totalCount;

                if (filter != null)
                {
                    query = query.Where(filter);
                    filteredCount = query.Count();
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                }
                if (page > 0 && pageSize > 0)
                {
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                List<TEntity> pageData = query.ToList();

                return new PaginationModel<TEntity>
                {
                    TotalCount = totalCount,
                    FilteredCount = filteredCount,
                    PageData = pageData,
                };
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public List<E> Query<E>(string Query, Dictionary<string, object> Parameters = null) where E : class
        {
            try
            {
                IEnumerable<E> result = ExtentionMethods.ReadAllQuery<E>(dbcontext, Query, Parameters);
                return result.ToList();
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public PaginationModel<E> Query<E>(string Query, int pageNo, int pageSize, string orderby, Dictionary<string, object> Parameters = null) where E : class
        {
            try
            {
                string paginationQuery = $@"declare @skipRows int = {(pageNo - 1) * pageSize},
                                            @takeRows int = {pageSize},
                                            @count int = 0
                                    
                                    ;WITH Orders_cte AS (
                                        {Query}
                                    )
                                    
                                    SELECT                                         
                                        tCountOrders.CountOrders AS TotalRows,*
                                    FROM Orders_cte
                                        CROSS JOIN (SELECT Count(*) AS CountOrders FROM Orders_cte) AS tCountOrders
                                    ORDER BY {orderby}
                                    OFFSET @skipRows ROWS
                                    FETCH NEXT @takeRows ROWS ONLY;";

                IEnumerable<E> result = ExtentionMethods.ReadAllQuery<E>(dbcontext, paginationQuery, Parameters);

                return new PaginationModel<E>
                {
                    TotalCount = Convert.ToInt32(ExecuteScalar(paginationQuery, Parameters)),
                    FilteredCount = result.Count(),
                    PageData = result.ToList()
                };
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public object ExecuteScalar(string sql, Dictionary<string, object> Parameters = null)
        {
            try
            {
                object result;
                using (DbCommand connection = dbcontext.Database.GetDbConnection().CreateCommand())
                {
                    if (connection.Connection.State != ConnectionState.Open)
                        connection.Connection.Open();

                    using (DbCommand command = connection)
                    {

                        if (Parameters != null)
                        {
                            foreach (KeyValuePair<string, object> param in Parameters)
                            {
                                DbParameter dbParameter = command.CreateParameter();
                                dbParameter.ParameterName = param.Key;
                                dbParameter.Value = param.Value;
                                command.Parameters.Add(dbParameter);
                            }
                        }

                        command.CommandText = sql;
                        result = command.ExecuteScalar();

                    }
                }
                return result;
            }
            catch (Exception Ex)
            {

                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
        public bool Exist(string sql, Dictionary<string, object> Parameters = null)
        {
            try
            {
                return ExecuteScalar(sql, Parameters) != null;
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }

        public long Count(string sql, Dictionary<string, object> Parameters = null)
        {
            try
            {
                return Convert.ToInt64(ExecuteScalar(sql, Parameters));
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> Parameters = null)
        {
            try
            {
                int result;
                using (DbCommand connection = dbcontext.Database.GetDbConnection().CreateCommand())
                {
                    if (connection.Connection.State != ConnectionState.Open)
                        connection.Connection.Open();

                    using (DbCommand command = connection)
                    {

                        if (Parameters != null)
                        {
                            foreach (KeyValuePair<string, object> param in Parameters)
                            {
                                DbParameter dbParameter = command.CreateParameter();
                                dbParameter.ParameterName = param.Key;
                                dbParameter.Value = param.Value;
                                command.Parameters.Add(dbParameter);
                            }
                        }

                        command.CommandText = sql;
                        result = command.ExecuteNonQuery();

                    }
                }
                return result;
            }
            catch (Exception Ex)
            {

                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }

    }
    public static class ExtentionMethods
    {
        public static IEnumerable<E> ReadAllQuery<E>(this DbContext dbContext, string Sql, Dictionary<string, object> Parameters = null) where E : class
        {
            using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = Sql;
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();

                if (Parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in Parameters)
                    {
                        DbParameter dbParameter = cmd.CreateParameter();
                        dbParameter.ParameterName = param.Key;
                        dbParameter.Value = param.Value;
                        cmd.Parameters.Add(dbParameter);
                    }
                }

                using (DbDataReader dataReader = cmd.ExecuteReader())
                {

                    while (dataReader.Read())
                    {
                        E dataRow = GetDataRow<E>(dataReader);
                        yield return dataRow;

                    }
                }


            }
        }
        private static E GetDataRow<E>(DbDataReader dataReader) where E : class
        {
            try
            {
                var dataRow = new ExpandoObject() as IDictionary<string, object>;
                for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                {
                    dataRow.Add(dataReader.GetName(fieldCount), dataReader[fieldCount].ToString() == string.Empty ? null : dataReader[fieldCount]);
                }
                E a = Activator.CreateInstance<E>();
                Mapper<E>.Map((ExpandoObject)dataRow, a);
                return a;
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
    }

    // By using a generic class we can take advantage
    // of the fact that .NET will create a new generic type
    // for each type T. This allows us to avoid creating
    // a dictionary of Dictionary<string, PropertyInfo>
    // for each type T. We also avoid the need for the 
    // lock statement with every call to Map.
    public static class Mapper<T>
        // We can only use reference types
        where T : class
    {
        private static readonly Dictionary<string, PropertyInfo> _propertyMap;

        static Mapper()
        {
            // At this point we can convert each
            // property name to lower case so we avoid 
            // creating a new string more than once.
            _propertyMap =
                typeof(T)
                .GetProperties()
                .ToDictionary(
                    p => p.Name.ToLower(),
                    p => p
                );
        }

        public static void Map(ExpandoObject source, T destination)
        {
            try
            {
                // Might as well take care of null references early.
                if (source == null)
                    throw new ArgumentNullException("source");
                if (destination == null)
                    throw new ArgumentNullException("destination");

                // By iterating the KeyValuePair<string, object> of
                // source we can avoid manually searching the keys of
                // source as we see in your original code.
                foreach (var kv in source)
                {
                    PropertyInfo p;
                    if (_propertyMap.TryGetValue(kv.Key.ToLower(), out p))
                    {
                        var propType = p.PropertyType;
                        if (kv.Value == null)
                        {
                            if (propType.IsByRef) // && propType.Name != "Nullable`1")
                            {
                                // Throw if type is a value type 
                                // but not Nullable<>
                                throw new ArgumentException("not nullable");
                            }
                        }
                        //else if (kv.Value.GetType() != propType)
                        //{
                        //    // You could make this a bit less strict 
                        //    // but I don't recommend it.
                        //    throw new ArgumentException("type mismatch");
                        //}
                        p.SetValue(destination, kv.Value, null);
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex.InnerException ?? new Exception(message: Ex.Message);
            }
        }
    }
}
