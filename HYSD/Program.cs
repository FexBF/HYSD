using Autofac;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HYSD
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 这里填入你刚才生成的 GUID
            // 加 "Global\\" 是为了让它在终端服务（如远程桌面）的所有会话中都生效
            const string MutexName = "Global\\{14435012-63DA-4577-B0A9-CFC3BB468931}";
            bool createdNew;

            // 使用带有 GUID 的名字创建 Mutex
            using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("程序已经在运行了，请不要重复打开。");
                    return;
                }

                if (!HslCommunication.Authorization.SetAuthorizationCode("c0b5dc3b-6b47-428b-9790-d6284e9dcad1"))
                {
                    MessageBox.Show("授权失败，请联系开发者获取授权码！", "授权失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                try
                {
                    // 1. 注册依赖
                    AutofacConfig.Register();

                    // 2. 运行应用
                    // 如果 FormMain 使用了构造函数注入，请使用 Resolve
                    using (var scope = AutofacConfig.Container.BeginLifetimeScope())
                    {
                        var form = scope.Resolve<FormMain>();
                        Application.Run(form);
                    }
                }
                catch (Exception ex)
                {
                    // 捕获启动阶段的严重错误
                    Log.Fatal(ex, "Application terminated unexpectedly");
                }
                finally
                {
                    // 3. 确保日志被刷新到文件
                    Log.CloseAndFlush();
                }
            }
        }
    }
}