using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    public interface IReadDataService
    {
        void Start();
        void Stop();

        /// <summary>
        /// PLC 数据刷新完成事件（事件驱动核心）。
        /// 由 ReadDataService 在每次成功读取一轮 PLC 数据后触发，
        /// 订阅方（页面 / 主窗体）在回调中刷新 UI，无需再各自轮询。
        /// 事件在后台读取线程触发，UI 订阅方需自行 BeginInvoke 切回 UI 线程。
        /// </summary>
        event EventHandler DataUpdated;

        /// <summary>
        /// PLC 连接状态变化事件（true=已连接, false=断开）。
        /// 仅在状态发生跃迁时触发，避免重复通知。
        /// </summary>
        event EventHandler<bool> ConnectionChanged;

        /// <summary>当前 PLC 是否已连接</summary>
        bool IsConnected { get; }

        object TryGetValueD29(string tagName);
        object TryGetValueD19(string tagName);
        object TryGetValueD15(string tagName);
        object TryGetValueW(string tagName);
        object TryGetValueC(string tagName);
        object TryGetValueD10(string tagName);
        bool isRunning { get; }
    }
}
