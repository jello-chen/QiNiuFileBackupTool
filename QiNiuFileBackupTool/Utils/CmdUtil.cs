using System.Diagnostics;

namespace QiNiuFileBackupTool.Utils
{
    class CmdUtil
    {
        public static string Execute(string cmd, string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(cmd, arguments);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            }
        }
    }
}
