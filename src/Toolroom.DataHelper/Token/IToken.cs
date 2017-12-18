using System;

namespace Toolroom.DataHelper
{
    public interface IToken : IIsDeletedFlagEntity
    {
        int Id { get; set; }
        string TokenKey { get; set; }
        DateTime ValidUntil { get; set; }
    }
}