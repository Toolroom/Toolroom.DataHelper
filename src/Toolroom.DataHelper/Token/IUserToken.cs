namespace Toolroom.DataHelper
{
    public interface IUserToken : IToken
    {
        int UserId { get; set; }
    }
}