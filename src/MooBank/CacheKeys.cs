namespace Asm.MooBank;

public static class CacheKeys
{
    public const string UserCacheKeyFormat = "User_{0}_Claims";

    public static string UserCacheKey(Guid userId) => String.Format(UserCacheKeyFormat, userId);

    public static class ReferenceData
    {
        public const string CacheTag = "ReferenceData";

        public const string ExchangeRates = nameof(ExchangeRates);

        public const string CpiChanges = nameof(CpiChanges);
    }
}
