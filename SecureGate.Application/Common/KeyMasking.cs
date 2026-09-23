namespace SecureGate.Application.Common;

public static class KeyMasking
{
    public static string Mask(string? keyValue) =>
        string.IsNullOrEmpty(keyValue)
            ? ""
            : keyValue.Length <= 8
                ? new string('*', keyValue.Length)
                : $"{keyValue[..4]}…{keyValue[^4..]}";
}
