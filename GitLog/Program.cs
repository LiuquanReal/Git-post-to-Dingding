using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitLog
{
    class Program
    {
        /// <summary>
        /// 浙江工商群机器人
        /// </summary>
        static string url = "https://oapi.dingtalk.com/robot/send?access_token=7501e77b96cb28fef0022f01db771abd473b7d0e9a4357b55990cb3c8634c825";
        static void Main(string[] args)
        { ;
            string gitLog = GetGitLog();
            PostMsg(GitLogFormat(gitLog));
        }

        static string GetGitLog()
        {
            string cmd = "git log --pretty=format:\"LogMessage:%an:% s\" -1";
            return RunCMD(cmd);
        }
        /// <summary>
        /// 运行cmd并且输入命令
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <returns>返回文字</returns>
        static string RunCMD(string cmdStr)
        {
            //string str = Console.ReadLine();

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(cmdStr + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            return output;
        }

        static string EncodingTransfer(string str,Encoding srcEncoding)
        {
            byte[] bytes = srcEncoding.GetBytes(str);
            byte[] nbytes = Encoding.Convert(srcEncoding, Encoding.UTF8, bytes);
            return Encoding.UTF8.GetString(nbytes);
        }

        static string GitLogFormat(string msg)
        {
            string returnMsg = "";
            foreach (Match m in Regex.Matches(msg,@"\nLogMessage:[\S\s]*:[\S\s]*"))
            {
                returnMsg = m.ToString();
            }
            string[] info = returnMsg.Split(':');
            return "项目有新的提交，请及时拉取!\n提交者：\n" + info[1] + "\n提交说明：\n" + info[2];
        }

        static void PostMsg(string message)
        {
            Post(url, SetNormalText(message));
        }

        static string SetNormalText(string sendText)
        {
            string s = "{\"msgtype\": \"text\",\"text\": {\"content\": \"" + sendText + "\"}}";
            return s;
        }
        

        /// <summary>
        /// 发送到钉钉
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
         static string Post(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            byte[] bytes = Encoding.UTF8.GetBytes(postDataStr);
            request.ContentLength = bytes.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(bytes, 0, bytes.Length);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码 
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }
    }
}
