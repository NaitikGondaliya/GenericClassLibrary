using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShivOhm.Infrastructure
{
    public class GenericDbContext : DbContext
    {

        public GenericDbContext(DbContextOptions<GenericDbContext> options) : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            IEnumerable<Type> lstEntity = GetTypesWithMyAttribute(AppDomain.CurrentDomain.GetAssemblies());
            if (lstEntity.Any())
            {
                foreach (Type objentity in lstEntity)
                {
                    if (objentity.BaseType?.BaseType == null)
                    {
                        TableAttribute[] TableName = (TableAttribute[])Attribute.GetCustomAttributes(objentity, typeof(TableAttribute));
                        modelBuilder.Entity(objentity).ToTable(TableName.FirstOrDefault()?.TableName);
                    }
                }
            }
        }

        private static IEnumerable<Type> GetTypesWithMyAttribute(Assembly[] assembly)
        {
            foreach (Assembly objassembly in assembly)
            {
                foreach (Type type in objassembly.GetTypes())
                {
                    if (Attribute.IsDefined(type, typeof(TableAttribute)))
                        yield return type;
                }
            }
        }
    }
}
