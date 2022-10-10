using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CopyFileTool
{
    /// <summary>
    /// ControlCopyFile.xaml 的交互逻辑
    /// </summary>
    public partial class ControlCopyFile : UserControl
    {
        public ControlCopyFile()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 用来保存到json里的数据
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class JsonData
        {
            [JsonProperty]
            public string? srcPath;

            [JsonProperty]
            public string? dstPath;
        }

        /// <summary>
        /// 控件的序号
        /// </summary>
        public int ControlIndex
        {
            get { return (int)GetValue(ControlIndexProperty); }
            set { SetValue(ControlIndexProperty, value); }
        }

        /// <summary>
        /// 用Propdp的代码片段生成这个
        /// </summary>
        public static readonly DependencyProperty ControlIndexProperty =
              DependencyProperty.Register("ControlIndex", typeof(int), typeof(ControlCopyFile), new PropertyMetadata(0));


        /// <summary>
        /// 把控件中的文本拆分成一段段
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        List<string> SplitString(string text)
        {
            List<string> result = new List<string>();
            //中间一段匹配 非双引号,非单引号,非逗号,非换行字符,非制表符
            Regex regex = new Regex("([\"'\\n\\t])([^\"',\\n\\t]+)([\"'\\n\\t])", RegexOptions.None);
            MatchCollection mc = regex.Matches(text);
            foreach (Match m in mc)
            {
                result.Add(m.Groups[2].Value);
            }
            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> listSrcPath = SplitString(srcText.Text);//控件中的文本
            List<string> listDstPath = SplitString(dstText.Text);//控件中的文本

            //如果用引号都找不到匹配,那么就直接加整段的吧
            if (listSrcPath.Count == 0)
            {
                listSrcPath.Add(srcText.Text);
            }
            if (listDstPath.Count == 0)
            {
                listDstPath.Add(dstText.Text);
            }

            if (listSrcPath.Count != listDstPath.Count)
            {
                MessageBox.Show(Application.Current.MainWindow, "源和目标的数组长度不一致!");
                return;
            }

            SaveState();
            int successCount = 0;
            for (int i = 0; i < listSrcPath.Count; i++)
            {
                string srcPath = listSrcPath[i];
                string dstPath = listDstPath[i];
                try
                {
                    //移除引号,实际上这里没有引号了
                    srcPath = srcPath.Replace("\"", "");
                    dstPath = dstPath.Replace("\"", "");

                    FileInfo srcFile = new FileInfo(srcPath);
                    if (srcFile.Exists)
                    {
                        FileInfo dstFile = new FileInfo(dstPath);
                        if (CopyFile(srcFile.FullName, dstFile.FullName))
                        {
                            successCount++;
                        }
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            MessageBox.Show(Application.Current.MainWindow, $"拷贝完成!成功拷贝了{successCount}个文件.");
        }

        /// <summary>
        /// 复制文件,用共享模式打开文件,然后读取写入到目标位置.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        private bool CopyFile(string sourcePath, string destPath)
        {
            bool success = false;
            FileStream? fsR = null;
            FileStream? fsW = null;
            FileInfo f = new FileInfo(sourcePath);
            try
            {
                //注意这里的FileShare设置,用共享模式打开文件,免得被占用
                fsR = File.Open(sourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                fsW = File.Create(destPath);
                long fileLength = f.Length;

                byte[] buffer = new byte[1024 * 1024 * 1];
                int n = 0;
                while (true)
                {
                    //读写文件
                    n = fsR.Read(buffer, 0, buffer.Length);
                    if (n == 0)
                    {
                        break;
                    }
                    fsW.Write(buffer, 0, n);
                    fsW.Flush();
                    Thread.Yield();
                }
                success = true;//成功拷贝了
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            fsR?.Close();
            fsW?.Close();
            return success;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //只有在这个Load事件的时候,ControlIndex才有值.
            string? json = AppData.Inst.GetData(ControlIndex);
            if (json != null)
            {
                JsonData? dataObj = JsonConvert.DeserializeObject<JsonData>(json);
                if (dataObj != null)
                {
                    srcText.Text = dataObj.srcPath;
                    dstText.Text = dataObj.dstPath;
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("控件卸载");
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 保存当前状态
        /// </summary>
        private void SaveState()
        {
            JsonData jsonData = new JsonData()
            {
                srcPath = srcText.Text,
                dstPath = dstText.Text
            };
            string json = JsonConvert.SerializeObject(jsonData);
            AppData.Inst.SetData(ControlIndex, json);

        }
    }
}
