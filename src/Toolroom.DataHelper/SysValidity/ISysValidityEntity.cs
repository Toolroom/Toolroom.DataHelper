using System;

namespace Toolroom.DataHelper
{
    public interface ISysValidityEntity
    {
        DateTime SysStartTime { get; }
        DateTime SysEndTime { get; }
    }
}