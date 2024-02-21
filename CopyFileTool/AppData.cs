using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CopyFileTool
{


    /// <summary>
    /// 整个app的数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AppData
    {
        public static AppData Inst { get; private set; } = new AppData();

        /// <summary>
        /// 每个控件的数据字典
        /// </summary>
        [JsonProperty]
        public Dictionary<int, string>? dictUserControlData;

        /// <summary>
        /// 反正尝试读取一下文件
        /// </summary>
        public void LoadFile()
        {
            lock (this) {
                try {
                    string json = File.ReadAllText("appdata.json");
                    AppData? tempObj = JsonConvert.DeserializeObject<AppData>(json);
                    Inst = tempObj ?? Inst;
                }
                catch (Exception) {

                }
            }

        }

        /// <summary>
        ///  保存到文件
        /// </summary> 
        public void WriteFile()
        {
            lock (this) {
                try {
                    string? json = JsonConvert.SerializeObject(this);
                    File.WriteAllText("appdata.json", json);
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 通过一个controlIndex来获取数据
        /// </summary>
        /// <param name="controlIndex"></param>
        /// <returns>保存的json数据</returns>
        public string? GetData(int controlIndex)
        {
            lock (this) {
                if (dictUserControlData != null &&
                dictUserControlData.ContainsKey(controlIndex)) {
                    return dictUserControlData[controlIndex];
                }
            }
            return null;
        }

        /// <summary>
        /// 保存数据.
        /// </summary>
        /// <param name="controlIndex"></param>
        /// <param name="json"></param>
        public void SetData(int controlIndex, string json)
        {
            lock (this) {
                if (dictUserControlData == null) {
                    dictUserControlData = new Dictionary<int, string>();
                }
                dictUserControlData[controlIndex] = json;

                //每次发生改变了就存一下吧
                WriteFile();
            }
        }
    }
}
