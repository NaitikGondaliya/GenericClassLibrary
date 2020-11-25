using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

                        List<PropertyInfo> prop = objentity.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttributes(typeof(ExcludeColumnAttribute), false).Count() > 0).ToList();
                        if (prop.Any())
                        {
                            foreach (PropertyInfo propertyInfo in prop)
                            {
                                ExcludeColumnAttribute AttExcluded = (ExcludeColumnAttribute)Attribute.GetCustomAttributes(propertyInfo, typeof(ExcludeColumnAttribute)).FirstOrDefault();
                                if (!AttExcluded.AllowAdd)
                                {
                                    modelBuilder.Entity(objentity).Property(propertyInfo.Name).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);                                    
                                }
                                if (!AttExcluded.AllowUpdate)
                                {
                                    modelBuilder.Entity(objentity).Property(propertyInfo.Name).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                                }
                                if (!AttExcluded.AllowRead)
                                {
                                    modelBuilder.Entity(objentity).Ignore(propertyInfo.Name);
                                }
                            }
                        }
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
