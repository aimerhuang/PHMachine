using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.ZHPHMachine.Command
{
    /// <summary>
    /// 
    /// </summary>
    public class PingIp
    {
        Thread thread;
        int ping_time = 1000 * 60;
        System.Timers.Timer timer;
        public string Ip;
        public delegate void PingHandle(string ip, bool flag);
        public static event PingHandle pingEvent;
        public bool PingStop = false;


        public PingIp(string ip)
        {
            Ip = ip;
            thread = new Thread(new ThreadStart(RunSecondThread));
            thread.Start();
        }

        void RunSecondThread()
        {
            timer = new System.Timers.Timer(ping_time);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (PingStop == false)
            {
                AutoPingIP(Ip);
            }
            else
            {
                timer.Stop();
            }
        }



        void AutoPingIP(string ip)
        {
            Ping p = new Ping();
            PingOptions ops = new PingOptions();
            ops.DontFragment = true;
            string d = "test data";
            byte[] buf = Encoding.ASCII.GetBytes(d);
            int timeout = 3000;

            PingReply pr = p.Send(ip, timeout, buf, ops);
            if (ip != "")
            {
                if (pr.Status == IPStatus.Success)
                {
                    if (pingEvent != null)
                        pingEvent(ip, true);
                }
                else
                {
                    if (pingEvent != null)
                    {
                        PingStop = true;
                        pingEvent(ip, false);
                    }
                }
            }
        }

        public void StopTh()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            if (thread != null)
            {
                if (thread.IsAlive)
                    thread.Abort();
            }
        }
        /// <summary>
        /// Ping命令检测网络是否畅通
        /// </summary>
        /// <param name="urls">URL数据</param>
        /// <param name="errorCount">ping时连接失败个数</param>
        /// <returns></returns>
        public static bool MyPing(string[] urls, out int errorCount)
        {
            bool isconn = true;
            Ping ping = new Ping();
            errorCount = 0;
            try
            {
                PingReply pr;
                for (int i = 0; i < urls.Length; i++)
                {
                    pr = ping.Send(urls[i]);
                    if (pr.Status != IPStatus.Success)
                    {
                        isconn = false;
                        errorCount++;
                    }
                    //Console.WriteLine("Ping " + urls[i] + "    " + pr.Status.ToString());
                }
            }
            catch
            {
                isconn = false;
                errorCount = urls.Length;
            }
            //if (errorCount > 0 && errorCount < 3)
            //  isconn = true;
            return isconn;
        }
    }
}
