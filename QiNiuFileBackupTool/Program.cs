using QiNiuFileBackupTool.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QiNiuFileBackupTool
{
    class Program
    {
        private static readonly string qrsctl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools\\qrsctl-v3.2.20170501.exe");
        private static readonly Regex bucketRegex = new Regex("\\[(.*)\\]");

        static void Main(string[] args)
        {
            Console.Title = "QiNiu File Backup Tool";
            ConsoleColor consoleColor = Console.ForegroundColor;

            Console.WriteLine("Please input your qiniu username:");
            var username = Console.ReadLine();
            Console.WriteLine("Please input your qiniu password:");
            var password = Console.ReadLine();

            // login
            var loginOutput = CmdUtil.Execute(qrsctl, $"login {username} {password}");
            if(!string.IsNullOrWhiteSpace(loginOutput))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(loginOutput);
                return;
            }

            // check buckets
            var checkBucketsOutput = CmdUtil.Execute(qrsctl, "buckets");
            var bucketMatch = bucketRegex.Match(checkBucketsOutput);
            if(!bucketMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(checkBucketsOutput);
                return;
            }
            var buckets = bucketMatch.Groups[1].Value.Split(' ');
            if(buckets.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No buckets.");
                return;
            }

            // list and download
            for (int i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                var bucketDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, bucket);
                if (!Directory.Exists(bucketDir))
                    Directory.CreateDirectory(bucketDir);

                var markersOutput = CmdUtil.Execute(qrsctl, $@"listprefix {bucket} """"");
                var markers = markersOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
                if (markers.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"No markers.");
                }
                else
                {
                    Parallel.ForEach(markers, marker =>
                    {
                        var markerDir = Path.Combine(bucketDir, marker);
                        var downloadOutput = CmdUtil.Execute(qrsctl, $"get {bucket} {marker} {markerDir}");
                        if (downloadOutput != "\n")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Download {markerDir} failed:{downloadOutput}");
                        }
                    });
                }
            }

            Console.ForegroundColor = consoleColor;

            Console.WriteLine("QiNiu File Backup Finished.");

            Console.ReadKey();
        }
    }
}
