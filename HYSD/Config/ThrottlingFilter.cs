using Microsoft.Extensions.Caching.Memory;
using Serilog.Core;
using Serilog.Events;
using System;

public class ThrottlingFilter : ILogEventFilter
{
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly TimeSpan _suppressionWindow;

    // 构造函数，传入抑制时间窗口（例如 5 分钟）
    public ThrottlingFilter(TimeSpan suppressionWindow)
    {
        _suppressionWindow = suppressionWindow;
    }

    public bool IsEnabled(LogEvent logEvent)
    {
        // 只对 Warning 及以上级别（包含 Error, Fatal）进行节流，Debug/Info 正常记录
        if (logEvent.Level < LogEventLevel.Debug)
            return true;

        // 生成唯一的缓存键（基于消息模板和异常类型，而不是具体的参数值）
        string cacheKey = GenerateCacheKey(logEvent);

        // 如果缓存中已存在该键，说明在时间窗口内已经记录过相同错误，丢弃本次日志
        if (_cache.TryGetValue(cacheKey, out _))
        {
            return false; // 返回 false 表示不记录此日志
        }

        // 如果不存在，则加入缓存，并设置过期时间
        _cache.Set(cacheKey, true, _suppressionWindow);
        return true; // 返回 true 表示记录此日志
    }

    private string GenerateCacheKey(LogEvent logEvent)
    {
        // 使用 MessageTemplate 而不是渲染后的文本，因为 "连接数据库失败: {Server}" 不管 Server 是什么，都算同一类错误
        string key = logEvent.MessageTemplate.Text;

        if (logEvent.Exception != null)
        {
            // 附加异常类型，因为相同消息但不同异常类型应该视为不同错误
            key += "_" + logEvent.Exception.GetType().Name;
        }

        return key;
    }
}
