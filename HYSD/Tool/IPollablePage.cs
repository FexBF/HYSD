using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HYSD
{
    /// <summary>
    /// 需要响应 PLC 数据刷新的页面统一实现此接口，
    /// 由 FormMain.SwitchPage 在切走时调用 StopPolling()（取消订阅），
    /// 切入时调用 StartPolling()（订阅 DataUpdated 事件），避免后台循环泄漏。
    ///
    /// ★ 事件驱动改造：方法名保留 StartPolling/StopPolling 以兼容现有 SwitchPage 调用，
    /// 但语义已从"启动轮询循环"变为"订阅 / 取消订阅 DataUpdated 事件"。
    /// </summary>
    public interface IPollablePage
    {
        /// <summary>订阅 PLC 数据刷新事件（内部需保证可重入、不重复订阅）</summary>
        void StartPolling();

        /// <summary>取消订阅 PLC 数据刷新事件</summary>
        void StopPolling();
    }

    /// <summary>
    /// 所有需要响应 PLC 数据的 UserControl 的基类（事件驱动版）。
    /// 子类只需 override OnDataUpdated() 实现具体的 UI 刷新逻辑，
    /// 基类负责订阅 / 取消订阅 IReadDataService.DataUpdated，并把回调切回 UI 线程。
    /// </summary>
    public abstract class PlcDataUserControl : UserControl, IPollablePage
    {
        private readonly IReadDataService _readData;
        private int _subscribed; // 0=未订阅, 1=已订阅（Interlocked 防重入）

        protected PlcDataUserControl(IReadDataService readData)
        {
            _readData = readData;
        }

        /// <summary>子类实现：每次 PLC 数据刷新后要做的 UI 刷新工作。
        /// 此方法会被基类通过 BeginInvoke 切回 UI 线程调用，无需自己再 Invoke。</summary>
        protected abstract void OnDataUpdated();

        /// <summary>是否允许本次刷新（默认：控件已创建句柄、未释放、PLC 在运行）。
        /// 子类可 override 增加额外条件。</summary>
        protected virtual bool CanUpdate()
        {
            return _readData != null
                && _readData.isRunning
                && !this.IsDisposed
                && this.IsHandleCreated;
        }

        public void StartPolling()
        {
            if (Interlocked.CompareExchange(ref _subscribed, 1, 0) != 0) return;
            if (_readData == null) { Interlocked.Exchange(ref _subscribed, 0); return; }
            _readData.DataUpdated += OnDataUpdatedInternal;
        }

        public void StopPolling()
        {
            if (Interlocked.Exchange(ref _subscribed, 0) == 0) return;
            if (_readData == null) return;
            try { _readData.DataUpdated -= OnDataUpdatedInternal; } catch { }
        }

        private void OnDataUpdatedInternal(object sender, EventArgs e)
        {
            // 事件由后台读取线程触发；切回 UI 线程后再执行刷新
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (!CanUpdate()) return;
            try
            {
                this.BeginInvoke((Action)OnDataUpdated);
            }
            catch (InvalidOperationException) { /* 句柄未就绪，忽略 */ }
            catch { /* 吞掉单次异常，保持订阅存活 */ }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopPolling();
            }
            base.Dispose(disposing);
        }
    }
}
