using System.Collections;
using System.Collections.Generic;

namespace HYSD
{
    /// <summary>
    /// 线程安全的报警数据集合。
    /// ★ 修复：原代码使用 HashSet&lt;AlarmData&gt;，被 Alarm.DoWork（后台线程）
    /// 和 FormMain.AlarmCheck（UI 线程）并发读写，HashSet 非线程安全，
    /// 可能导致死循环、数据错乱甚至程序崩溃。
    /// 此包装内部用 lock 保护所有访问，并实现 IEnumerable 以兼容原有 LINQ 调用
    /// （FirstOrDefault / ToList 等会自动获取快照枚举，避免枚举期间被修改抛异常）。
    /// </summary>
    public class ConcurrentAlarmSet : IEnumerable<AlarmData>
    {
        private readonly HashSet<AlarmData> _set = new HashSet<AlarmData>();
        private readonly object _lock = new object();

        public bool Add(AlarmData item)
        {
            lock (_lock) return _set.Add(item);
        }

        public bool Remove(AlarmData item)
        {
            lock (_lock) return _set.Remove(item);
        }

        public bool Contains(AlarmData item)
        {
            lock (_lock) return _set.Contains(item);
        }

        public int Count
        {
            get { lock (_lock) return _set.Count; }
        }

        /// <summary>
        /// 返回快照枚举器，确保遍历期间即使其他线程修改集合也不会抛 InvalidOperationException。
        /// </summary>
        public IEnumerator<AlarmData> GetEnumerator()
        {
            List<AlarmData> snapshot;
            lock (_lock) snapshot = new List<AlarmData>(_set);
            return snapshot.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>线程安全地清空</summary>
        public void Clear()
        {
            lock (_lock) _set.Clear();
        }
    }
}
