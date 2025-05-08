using System;

class Program
{
    static void Main(string[] args)
    {
        // 获取当前UTC时间戳（毫秒）
        long utcTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Console.WriteLine($"当前UTC时间戳（毫秒）: {utcTimestamp}");

        // 将时间戳转换回DateTime
        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(utcTimestamp).DateTime;
        Console.WriteLine($"转换后的DateTime: {dateTime}");

        // 如果需要本地时间
        DateTime localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(utcTimestamp).LocalDateTime;
        Console.WriteLine($"转换后的本地时间: {localDateTime}");

        // 另一种获取时间戳的方法（秒）
        long utcTimestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Console.WriteLine($"当前UTC时间戳（秒）: {utcTimestampSeconds}");

        // 从秒级时间戳转换回DateTime
        DateTime dateTimeFromSeconds = DateTimeOffset.FromUnixTimeSeconds(utcTimestampSeconds).DateTime;
        Console.WriteLine($"从秒级时间戳转换的DateTime: {dateTimeFromSeconds}");
    }
} 