using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;



namespace TJPU_AutoLogin
{


    public partial class MainWindow : Form
    {
        /***************************************************************************************/
        public string web_url_TJPUNet = "http://172.23.4.5";
        public string web_url_Baidu = "http://www.baidu.com";

        bool IsAutoRun;

        public string[] name = new string[1000];
        public string[] pass = new string[1000];

        int txtRows;
        int IDnum;
        int NowID;

        /***************************************************************************************/

        public MainWindow()//内部代码自动生成
        {
            InitializeComponent();
          
            
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.Text = "AutoLogin";
            pictureBox1.Image = Image.FromFile(Application.StartupPath + "\\tjpu.jpg");

            try
            {
                RegistryKey loca_chek = Registry.CurrentUser;
                RegistryKey run_Check = loca_chek.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                if (run_Check.GetValue("NetAutoLogin").ToString().ToLower() != "false")
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }

            catch { }
            

            webBrowser1.Visible = false;
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(web_url_Baidu);




            string filepath1 = Application.StartupPath + "\\ID.txt";

            txtRows = GetRows(filepath1);
            IDnum = txtRows - 2;

            NowID = 0;

            IDLoad(filepath1, IDnum);

            timer1.Interval = 5000;
            timer1.Enabled = true;

            timer2.Interval = 2000;
            timer2.Enabled = false;

            timer3.Interval = 1000;
            timer3.Enabled = false;

        }

        private void timer1_Tick(object sender, EventArgs e)//主，定时刷新Baidu 
        {
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;

            webBrowser1.Refresh();
            timer2.Enabled = true;
           
        }
        private void timer2_Tick(object sender, EventArgs e)//等待主刷新结束 查看Baidu是否加载
        {

            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;


            if (webBrowser1.DocumentTitle != "百度一下，你就知道")
            {
                //MessageBox.Show(webBrowser1.DocumentTitle);
                webBrowser1.Navigate(web_url_TJPUNet);
                timer3.Enabled = true;
            }
            else timer1.Enabled = true;

        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;

            if ((NowID >=IDnum)||(NowID>=1000)) NowID = 0;

            TryToConnect(name[NowID], pass[NowID]);

            NowID++;

            webBrowser1.Navigate(web_url_Baidu);

            timer2.Enabled = true;

        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)//角标
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示    
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                小图标.Visible = false;
            }

        }

        private void F_Main_SizeChanged(object sender, EventArgs e)//角标
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                小图标.Visible = true;
            }
        }

        private void F_Main_FormClosing(object sender, FormClosingEventArgs e)//角标
        {
            if (MessageBox.Show("是否确认退出自动登录程序？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                // 关闭所有的线程
                this.Dispose();
                this.Close();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void 还原ToolStripMenuItem_Click(object sender, EventArgs e)//角标
        {
            WindowState = FormWindowState.Normal;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)//角标
        {
            if (MessageBox.Show("是否确认退出自动登录程序？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                // 关闭所有的线程
                this.Dispose();
                this.Close();
            }
        }



        private void button1_Click(object sender, EventArgs e)//批量添加按钮
        {
            if (MessageBox.Show("请在确认后弹出的文本中导入,之后重启程序。", "了解", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo.FileName = Application.StartupPath + "\\ID.txt";
                pro.Start();//关键

            }

        }

        private int GetRows(string FilePath)//获取ID.txt行数
        {
            int lines = 0;  //用来统计txt行数
            FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            while (sr.ReadLine() != null)
            {
                lines++;
            }

            fs.Close();
            sr.Close();

            return lines;
        }

        private void IDLoad(string FilePath,int num)//从ID.txt中导入账号
        {

            FileStream MyInFile1 = null;

            MyInFile1 = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            StreamReader myfile = new StreamReader(MyInFile1);
            string str = "";
            str = myfile.ReadLine();
            str = myfile.ReadLine();
            str = myfile.ReadLine();
            string[] aa = str.Split(',');

            for (int i = 0; i < num; i++)
            {
                try
                {
                    name[i] = aa[0];
                    pass[i] = aa[1];

                    str = myfile.ReadLine();
                    aa = str.Split(',');
                }
                catch { }

            }

            MyInFile1.Close();
            myfile.Close();
        }

        private void TryToConnect(string NewID, string NewPass)//尝试登录
        {
            try
            {
                //MessageBox.Show("正在尝试ID:" + NewID + "\n" + "PASS:" + NewPass);
                HtmlElement tbUserid = webBrowser1.Document.All["DDDDD"];
                HtmlElement tbPasswd = webBrowser1.Document.All["upass"];

                if (!(tbUserid == null || tbPasswd == null))
                {
                    tbUserid.SetAttribute("value", NewID);
                    tbPasswd.SetAttribute("value", NewPass);

                }

                string str = null;

                for (int i = 0; i < webBrowser1.Document.All.Count; i++)
                {

                    str = webBrowser1.Document.All[i].GetAttribute("name");

                    if (str != "")
                    {

                        if (str != null && str.Trim() == "0MKKey")
                        {

                            HtmlElement he = webBrowser1.Document.All[i];

                            he.InvokeMember("click");

                            return;

                        }
                    }

                }
            }

            catch { }

            

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
   
            if (checkBox1.Checked == true) IsAutoRun = true;
            else IsAutoRun = false;
            try
            {
                //获取程序执行路径..
                string starupPath = Application.ExecutablePath;
                //class Micosoft.Win32.RegistryKey. 表示Window注册表中项级节点,此类是注册表装.
                RegistryKey loca = Registry.CurrentUser;
                RegistryKey run = loca.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                //SetValue:存储值的名称
                if (IsAutoRun == false) run.SetValue("NetAutoLogin", false);//取消开机运行
                else run.SetValue("NetAutoLogin", starupPath);//设置开机运行
                loca.Close();

            }
            catch
            { }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("BUG提交：QQ:2015531274");
        }
    }
}
