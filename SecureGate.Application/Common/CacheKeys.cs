namespace SecureGate.Application.Common;

public static class CacheKeys
{
    public static string ApiKey(string keyValue) => $"apikey:{keyValue}";
}
