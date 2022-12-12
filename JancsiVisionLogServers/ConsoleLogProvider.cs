using System;
using System.Collections.Generic;
using System.Text;

namespace JancsiVisionLogServers
{
    public class ConsoleLogProvider : ILogProvider
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="msg"></param>
        public void LogError(string msg)
        {
            Console.WriteLine($"log:Error{msg}");
        }
        /// <summary>
        /// 正确信息
        /// </summary>
        /// <param name="msg"></param>
        public void LogInfo(string msg)
        {
            Console.WriteLine($"log:Info{msg}");
        }
    }
}
