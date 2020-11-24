using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Text;

namespace ShivOhm.Infrastructure
{
    public static class ConnectionTools
    {
        // all params are optional
        public static void ChangeDbConnection(
            this DbContext source,            
            string ConnectionStringName)
        /* this would be used if the
        *  connectionString name varied from 
        *  the base EF class name */
        {
            try
            {
                //// use the const name if it's not null, otherwise
                //// using the convention of connection string = EF contextname
                //// grab the type name and we're done
                //var configNameEf = string.IsNullOrEmpty(ConnectionStringName)
                //    ? source.GetType().Name
                //    : ConnectionStringName;

                //// add a reference to System.Configuration
                //var entityCnxStringBuilder = new EntityConnectionStringBuilder
                //    (ConnectionStringName);

                //// init the sqlbuilder with the full EF connectionstring cargo
                //var sqlCnxStringBuilder = new SqlConnectionStringBuilder
                //    (ConnectionStringName);

                // now flip the properties that were changed
                source.Database.SetConnectionString(ConnectionStringName);
                
                
            }
            catch (Exception ex)
            {
                throw ex.InnerException ?? new Exception(message: ex.Message);
                // set log item if required
            }
        }
    }
}
