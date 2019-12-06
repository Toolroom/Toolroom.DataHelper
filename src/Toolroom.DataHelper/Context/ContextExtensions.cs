using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace Toolroom.DataHelper
{
    public static class ContextExtensions
    {
        public static void HandleSaveChanges(this DbContext context, int? editUserId)
        {
            context.ChangeTracker.DetectChanges();
            var detectAgain = false;

            {
                var changedItems = context.ChangeTracker.Entries();
                foreach (var item in changedItems)
                {
                    if (!(item.Entity is IXmlEntity xmlEntity)) 
                        continue;

                    xmlEntity.UpdatePreCommitValuesXml();
                    detectAgain = true;
                }
            }

            if (detectAgain)
            {
                context.ChangeTracker.DetectChanges();
                detectAgain = false;
            }

            {
                var changedItems = context.ChangeTracker.Entries();
                foreach (var item in changedItems)
                {
                    if (item.State == EntityState.Added || item.State == EntityState.Modified)
                    {
                        //store edit user id automatically
                        if (item.Entity is IEditUserEntity editUserEntity)
                            editUserEntity.EditUserId = editUserId;
                        
                        if (!(item.Entity is IComputationEntity computationEntity)) 
                            continue;

                        //compute entities
                        computationEntity.UpdatePreCommitValuesComputation();
                        
                        detectAgain = true;
                    }
                }
            }

            if (detectAgain)
            //{
                context.ChangeTracker.DetectChanges();
            //    detectAgain = false;
            //}
        }

        public static void ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
        {
            var xmlEntity = e.Entity as IXmlEntity;
            xmlEntity?.FillPropertiesFromXmlValues(typeof(XmlMappedAttribute), xmlEntity.XmlValues);
        }
    }
}
