using System;
using System.Linq;

namespace Toolroom.DataHelper
{
    public static class TokenExtensions
    {
        #region Filters
        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, int tokenId, TokenValidity tokenValidity, DeletedState deletedState) where T : class, IToken
        {
            return query.Filter(tokenValidity, deletedState, tokenId);
        }

        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, string tokenKey, TokenValidity tokenValidity, DeletedState deletedState) where T : class, IToken
        {
            return query.Filter(tokenValidity, deletedState, null, tokenKey);
        }

        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, string tokenKey, int userId, TokenValidity tokenValidity, DeletedState deletedState) where T : class, IUserToken
        {
            return query.Filter(tokenValidity, deletedState, null, tokenKey).Where(_ => _.UserId == userId);
        }

        private static IQueryable<T> Filter<T>(this IQueryable<T> query, TokenValidity tokenValidity, DeletedState deletedState = DeletedState.NotDeleted, int? tokenId = null, string tokenKey = null) where T : class, IToken
        {
            query = query.OfDeletedState(deletedState);

            switch (tokenValidity)
            {
                case TokenValidity.Valid:
                    query = query.Where(_ => _.ValidUntil >= DateTime.UtcNow);
                    break;
                case TokenValidity.NotValid:
                    query = query.Where(_ => _.ValidUntil < DateTime.UtcNow);
                    break;
                case TokenValidity.Any:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenValidity));
            }

            if (tokenId.HasValue)
                query = query.Where(_ => _.Id == tokenId.Value);

            if (tokenKey != null)
                query = query.Where(_ => _.TokenKey == tokenKey);

            return query;
        }


        #endregion
    }
}