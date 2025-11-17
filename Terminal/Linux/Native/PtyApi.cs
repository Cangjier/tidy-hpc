using System.Runtime.InteropServices;

namespace TidyHPC.Terminal.Linux.Native;
internal static class PtyApi
{
    [Flags]
    public enum WaitPidFlags : int
    {
        WNOHANG = 0x00000001,    // 非阻塞模式，立即返回
        WUNTRACED = 0x00000002   // 报告已停止的子进程
    }

    // PTY 相关的 P/Invoke 声明
    [DllImport("libc", SetLastError = true)]
    public static extern int openpty(out int master, out int slave, IntPtr name, IntPtr termios, IntPtr winsize);

    [DllImport("libc", SetLastError = true)]
    public static extern int fork();

    [DllImport("libc", SetLastError = true)]
    public static extern int login_tty(int fd);

    [DllImport("libc", SetLastError = true)]
    public static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    public static extern int kill(int pid, int sig);

    [DllImport("libc", SetLastError = true)]
    public static extern int waitpid(int pid, out int status, int options);

    [DllImport("libc", SetLastError = true)]
    public static extern int ioctl(int fd, int request, ref WinSize winsize);

    [DllImport("libc", SetLastError = true)]
    public static extern int dup2(int oldfd, int newfd);

    [DllImport("libc", SetLastError = true)]
    public static extern int execve(string file, string[] argv, string[] envp);

    [StructLayout(LayoutKind.Sequential)]
    public struct WinSize
    {
        public ushort ws_row;
        public ushort ws_col;
        public ushort ws_xpixel;
        public ushort ws_ypixel;
    }
}
