namespace Sonban.Api.Classes;

public static class CommonExtensions {
        
    public static bool IsNullOrEmpty(this string value) {
        return string.IsNullOrEmpty(value);
    }
    
    public static Guid ToGuid(this long value)
    {
        byte[] guidData = new byte[16];
        Array.Copy(BitConverter.GetBytes(value), guidData, 8);
        return new Guid(guidData);
    }
    
    public static long ToLong(this Guid guid)
    {
        if (BitConverter.ToInt64(guid.ToByteArray(), 8) != 0)
            throw new OverflowException("Value was either too large or too small for an Int64.");
        return BitConverter.ToInt64(guid.ToByteArray(), 0);
    }
}
