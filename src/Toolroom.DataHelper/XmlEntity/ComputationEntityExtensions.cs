using System;

namespace Toolroom.DataHelper
{
    public static class ComputationEntityExtensions
    {
        public static void UpdatePreCommitValuesComputation(this IComputationEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            entity.Compute();
        }
    }
}