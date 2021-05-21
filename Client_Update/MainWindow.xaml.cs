using System;
using System.Windows;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client_Update
{
    public delegate void dele();  //声明委托
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        string owner;
        string repo;
        string access_token;
        string path;
        string cookies;
        int download_counts = 0;
        int download_total = 0;
        List<Hashtable> Gitee_Mods_List;
        class OutputShow : INotifyPropertyChanged //绑定对象  
        {
            public string show;//显示
            public event PropertyChangedEventHandler PropertyChanged;
            public string Show
            {
                get { return show; }
                set
                {
                    show = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Show"));
                }
            }
        }

        // public dele de; //声明委托变量
        //Thread th; //声明一个线程
        /// <summary>
        /// 当前值
        /// </summary>
        /// 


        public MainWindow()
        {
            InitializeComponent();
            path = Directory.GetCurrentDirectory() + "\\.minecraft\\mods";
            try
            {
                Directory.GetDirectories(path);
                path_block.Text = path;
                progressBar1.Value = 0.1;
                progressBar1.Maximum = 1.01;
                progressBar1.Minimum = 0;
            }
            catch
            {
                System.Windows.MessageBox.Show("路径不正确，请移动到整合包主目录（保证自己能看到.minecraft文件夹）");
                //System.Windows.Forms.Application.Exit();
                path_block.Text = "路径不正确，请移动到整合包主目录（保证自己能看到.minecraft文件夹）";
            }
        }

        void fmt_output(string _out)
        {
            //Thread.Sleep(1);

            Thread thread = new Thread(new ThreadStart(() =>
            {
                
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    Console.WriteLine("TEST_OUTPUT:" + _out);
                    dynamic _temp = output_text.Text;
                    Console.WriteLine("in: " + _temp);
                    dynamic _buffer = _temp + _out + "\n";
                    Console.WriteLine("out: " + _buffer);
                    OutputShow log_show = new OutputShow();
                    log_show.show = _buffer;
                    output_text.DataContext = log_show;
                    System.Windows.Forms.Application.DoEvents();
                }));

            }));
            thread.Start();


        }
        public string get_mods()
        {
            //output_text.AppendText("TEST");
            fmt_output("检测本地mods内容：");
            ArrayList paths = new ArrayList();
            //string path = Directory.GetCurrentDirectory() + "\\.minecraft\\mods";
            DirectoryInfo dir = new DirectoryInfo(path);
            //获得mods名称
            foreach (FileSystemInfo fsi in dir.GetFileSystemInfos())
            {
                if (fsi is FileInfo)
                {
                    //获得文件
                    FileInfo fi = (FileInfo)fsi;

                    //output_text.Text += (char)13 + fi.Name.ToString();
                    paths.Add(fi.Name.ToString());
                    
                    //output_text.Text += (char)13 + client_mods_str;
                }
            }
            string client_mods_str = JsonConvert.SerializeObject(paths);
            fmt_output(client_mods_str);
            return client_mods_str;
        }
        public void request_del_string(string client_str)
        {
            WebClient Client = new WebClient();
            fmt_output("删除mod(s)：");
            string res = "";
            string url = "http://39.97.175.209:5000/api/gitee_mods_del";    //定义地址
            //string cmd = "api_key=xxx&api_secret=xxx&image=xxx";    //组装参数
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(client_str);     //Encoding值取决于目标服务的编码
            Client.Headers.Add("Content-Type", "application/json");
            bytes = Client.UploadData(url, "POST", bytes);    //POST
            string reply = System.Text.Encoding.UTF8.GetString(bytes);   //解析回应
            if (reply== "S,A,M,E")
            {
                //output_text.Text += "No file needs to del" + (char)13;
                fmt_output("No file needs to del");
                return;
            }
            string[] del_mods = reply.Split(',');
            foreach (string item in del_mods)
            {
                //output_text.Text +="DEL " +item + (char)13;
                fmt_output("DEL: " + item);
                File.Delete(path+"\\" + item);
            }

            //return reply;
            
            
        }
        public void request_cookie_string()
        {
            WebClient Client = new WebClient();
            string res = "";
            string url = "http://39.97.175.209:5000/api/gitee_cookies";    //定义地址
            //string cmd = "api_key=xxx&api_secret=xxx&image=xxx";    //组装参数
            byte[] bytes;     //Encoding值取决于目标服务的编码
            Client.Headers.Add("Content-Type", "application/json");
            bytes = Client.DownloadData(url);    //POST
            string reply = System.Text.Encoding.UTF8.GetString(bytes);   //解析回应
            cookies = reply;
            //output_text.Text +="cookies: "+cookies + (char)13;
            //return reply;


        }
        public void request_download_json(string client_str)
        {
            WebClient Client = new WebClient();
            fmt_output("下载更新内容：");
            
            string res = "";
            string url = "http://39.97.175.209:5000/api/gitee_mods_add";    //定义地址
            //string cmd = "api_key=xxx&api_secret=xxx&image=xxx";    //组装参数
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(client_str);     //Encoding值取决于目标服务的编码
            Client.Headers.Add("Content-Type", "application/json");
            bytes = Client.UploadData(url, "POST", bytes);    //POST
            string reply = System.Text.Encoding.UTF8.GetString(bytes);   //解析回应
            if (reply == "\"SAME\"")
            {
                fmt_output("No file needs to download");
                //output_text.Text +="No file needs to add" + (char)13;
                return;
            }
            //output_text.Text += "获得Gitee Cookies：" + (char)13;
            request_cookie_string();
            //output_text.Text += "开始下载进程：" + (char)13;
            dynamic del_mods = JsonConvert.DeserializeObject(reply);
            //WebClient dl = new WebClient();
            //dl.Headers.Set("Cookie", cookies);
            //dl.Headers.Set("accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            //Client.Headers.Set("method", "GET");
            download_counts = 0;
            JArray download_total_list = del_mods;
            download_total = download_total_list.Count;
            foreach (string item in del_mods)
            {
                WebClient dl = new WebClient();
                dl.Headers.Set("Cookie", cookies);
                dl.Headers.Set("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                Client.Headers.Set("method", "GET");
                fmt_output("ADD: " + item);
                //output_text.Text +="ADD " + item + (char)13;
                string dl_str = "https://gitee.com/stugiii/mc_endtime_may_2021/raw/main/mods/" +item;
                Uri dl_uri = new Uri(dl_str);
                //dl_uri.
                string filename_str = path + "\\" + item;
                //dl.DownloadFileCompleted += client_DownloadFileCompleted;
                //dl.DownloadProgressChanged += client_DownloadProgressChanged;
                dl.DownloadProgressChanged += wc_DownloadProgressChanged;
                dl.DownloadFileCompleted += wc_DownloadFileCompleted;
                dl.DownloadFileAsync(dl_uri,filename_str);
            }

            //return reply;


        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                Console.WriteLine(e.ProgressPercentage + "% | " + e.BytesReceived + " bytes out of " + e.TotalBytesToReceive + " bytes retrieven.");
                
                log_download.Text = e.ProgressPercentage + "% | " + e.BytesReceived + " bytes out of " + e.TotalBytesToReceive + " bytes retrieven.";
            }));

            // 50% | 5000 bytes out of 10000 bytes retrieven.
        }
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                

                if (e.Cancelled)
                {
                    System.Windows.MessageBox.Show("The download has been cancelled");
                    return;
                }

                if (e.Error != null) // We have an error! Retry a few times, then abort.
                {
                    System.Windows.MessageBox.Show("An error ocurred while trying to download file");

                    return;
                }
                download_counts += 1;

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine(download_counts / download_total);
                   this.Dispatcher.BeginInvoke((ThreadStart)delegate { this.progressBar1.Value = (double)download_counts / (double)download_total; }); //progressBar1是进度条控件的名字。

                }));
                thread.Start();
                
                //MessageBox.Show("File succesfully downloaded");
                log_download.Text = "File succesfully downloaded";
            }));

        }

        //--------------------------------------------------
        //void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    if (e.UserState != null)
        //    {
        //        output_text.Text = e.UserState.ToString() + ",下载完成";
        //    }
        //}

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = (int)e.TotalBytesToReceive;
            progressBar1.Value = (int)e.BytesReceived;
            //progressBar1.Text = e.ProgressPercentage + "%";
        }
        private void UPD_click(object sender, RoutedEventArgs e)
        {

            output_text.Text = "";
            
            //output_text.Text += "检测本地mods内容：" +(char)13;
            string mods_str = get_mods();
            //output_text.Text = mods_str + (char)13;
            
            //output_text.Text += "删除mod(s)：" + (char)13;
            Task task_del = new Task(() =>
            {
                request_del_string(mods_str);
            });
            task_del.Start();
            Task task_download = new Task(() =>
            {
                request_download_json(mods_str);
            });
            task_download.Start();

            task_del.Wait();
            task_download.Wait();
            //output_text.Text += "下载更新内容：" + (char)13;

            System.Windows.MessageBox.Show("检测更新完成");
            //开始处理

        }
        public void Output_text_TextChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Sync_click(object sender, RoutedEventArgs e)
        {


        }

        private void client_DownloadProgressChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }




        //---------------------------------------Gitee Way------------------------------------
        private JArray Gitee_Get_Mods(string _owner="stugiii",string _repo= "mc_endtime_may_2021",string _sha= "main",string _access_token= "a6fc2b1467c4d9f77f65e5651dfcf845")
        {
            //https://gitee.com/api/v5/repos/stugiii/mc_endtime_may_2021/git/trees/main?access_token=a6fc2b1467c4d9f77f65e5651dfcf845&recursive=1
            WebClient client = new WebClient();
            client.Headers.Set("Content-Type", "application/json;charset=UTF-8");
            string url_buffer = "https://gitee.com/api/v5/repos/"+_owner+"/"+_repo+"/git/trees/"+_sha+ "?access_token="+_access_token+"&recursive=1";
            repo = _repo;
            access_token = _access_token;
            owner =  _owner;
            Console.WriteLine(url_buffer);
            byte[] bytes = client.DownloadData(url_buffer);
            string reply = System.Text.Encoding.UTF8.GetString(bytes);   //解析回应
            dynamic _mods_obj = JsonConvert.DeserializeObject(reply);
            Console.WriteLine(_mods_obj["tree"]);
            JArray _mods_obj_processed = _mods_obj["tree"];
            
            foreach (JObject item in _mods_obj_processed)
            {
                if (item.GetValue("path").ToString()== ".gitattributes" || item.GetValue("path").ToString() == "mods")
                {
                    _mods_obj_processed.Remove(item);
                    break;
                }

            }
            foreach (JObject item in _mods_obj_processed)
            {
                if (item.GetValue("path").ToString() == "mods")
                {
                    _mods_obj_processed.Remove(item);
                    break;
                }

            }
            Console.WriteLine(_mods_obj_processed);
            List<Hashtable> _gitee_mods_list_temp = _mods_obj["tree"].ToObject<List<Hashtable>>();
            Gitee_Mods_List = _gitee_mods_list_temp;
            return _mods_obj["tree"];
        }
        private Hashtable Gitee_Compare_Difference(JArray _mods_gitee, JArray _mods_client,int model) //1 is add
        {
            List<string> _res_diff;
            Hashtable _res_table = new Hashtable();
            dynamic _gitee_mods_list_temp_keys = Gitee_Mods_List;
            Console.WriteLine(Gitee_Mods_List.GetType());
            List<string> _gitee_mods_list = new List<string>();
            Hashtable _gitee_mods_table = new Hashtable();
            List<string> _client_mods_list = new List<string>();
            //List<string> diff_del =_gitee_mods_list.Except(_mods_client).ToList();
            foreach (Hashtable jt in _gitee_mods_list_temp_keys)
            {
                _gitee_mods_list.Add(jt["path"].ToString().Replace("mods/",""));
                _gitee_mods_table.Add(jt["path"].ToString().Replace("mods/", ""), jt["sha"]);
            }
            foreach (dynamic jt in _mods_client)
            {
                _client_mods_list.Add(jt.ToString());
            }
            List<string> diff_del = new List<string>();
            Console.WriteLine(_gitee_mods_list);
            Console.WriteLine(_client_mods_list);
            if (model == 1)
            {
                _res_diff = _gitee_mods_list.Except(_client_mods_list).ToList();
            }
            else { _res_diff = _client_mods_list.Except(_gitee_mods_list).ToList(); } 
            foreach(string item in _res_diff)
            {
                if (_gitee_mods_table.Contains(item))
                {
                    _res_table.Add(item,_gitee_mods_table[item]);
                }
            }
            Console.WriteLine(_res_table);
            return _res_table;
        }
        private void file_del_mods(Hashtable del_mods)
        {
            foreach (string item in del_mods.Keys)
            {
                //output_text.Text +="DEL " +item + (char)13;
                fmt_output("DEL: " + item);
                File.Delete(path + "\\" + item);
            }
        }
        private void file_download_mods(Hashtable add_mods)
        {
            WebClient Client = new WebClient();
            fmt_output("下载更新内容：");
            if (add_mods.Count==0)
            {
                fmt_output("No file needs to download");
                //output_text.Text +="No file needs to add" + (char)13;
                return;
            }
            download_counts = 0;
            download_total = add_mods.Count;
            //foreach (string item in del_mods) {
            //    _mods_gitee.
            //}
            foreach (string item in add_mods.Keys)
            {
                //https://gitee.com/api/v5/repos/{owner}/{repo}/git/blobs/{sha}
                WebClient client = new WebClient();
                client.Headers.Set("Content-Type", "application/json;charset=UTF-8");
                string url_buffer = "https://gitee.com/api/v5/repos/" + owner + "/" + repo + "/git/blobs/" + add_mods[item] + "?access_token=" + access_token;
                Console.WriteLine(url_buffer);
                byte[] bytes = client.DownloadData(url_buffer);
                string reply = System.Text.Encoding.UTF8.GetString(bytes);   //解析回应
                dynamic _mod_obj = JsonConvert.DeserializeObject(reply);
                //Console.WriteLine(_mod_obj);
                string _buffer_content = _mod_obj["content"];
                // base64解码
                bytes = Convert.FromBase64String(_buffer_content);
                //_buffer_content =  Encoding.Default.GetString(bytes);
                string outPath = path + "\\" + item;
                using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
                Console.WriteLine("download and write done: "+outPath);
            }
        }
        private void Gitee_Button_Click(object sender, RoutedEventArgs e)
        {
            output_text.Text = "";
            JArray _mods_gitee =Gitee_Get_Mods();
            JArray _mods_client = (JArray)JsonConvert.DeserializeObject(get_mods());
            Hashtable diff_del = Gitee_Compare_Difference(_mods_gitee, _mods_client,2);
            //Console.WriteLine("diff_del: "+diff_del.ToString());
            //foreach(string st in diff_del)
            //{
            //    Console.WriteLine("diff_del: " + st);

            //}
            Hashtable diff_add = Gitee_Compare_Difference(_mods_gitee, _mods_client, 1);
            //foreach (string st in diff_del.Keys)
            //{
            //    Console.WriteLine("diff_del: " + st);

            //}
            //foreach (string st in diff_add.Keys)
            //{
            //    Console.WriteLine("diff_add: " + st);

            //}
            file_del_mods(diff_del);
            file_download_mods(diff_add);
        }
    }
    

}
