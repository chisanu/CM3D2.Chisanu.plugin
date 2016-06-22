using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CMD2.ChisanuManager.Plugin
{
    public delegate void VoidMethod();
    public delegate void OneParamMethod<T>(T param);
    public delegate void TwoParamMethod<A, B>(A paramA, B paramB);

  
 
    public class Manager
    {
        #region Quản lí các plugin khác

        private static Manager _manager=null;
        public static Manager manager
        {
            set { if (_manager == null) _manager = value;}
            get { return _manager==null?manager=new Manager():_manager; }
        }

        public  List<PluginInfo> allPlugin = new List<PluginInfo>();

        //Các thông tin trao đổi thêm giữa các plugin
        public Dictionary<string, UnityEngine.Object> extraInfoManager = new Dictionary<string, UnityEngine.Object>();

        public Manager()
        {
           // manager = new Manager();
            allPlugin = new List<PluginInfo>();
        }
        #endregion



    }

    public class PluginInfo
    {
       public  string name;
       public  string version;
       public  string iconPath;
       public  bool isActiveInScene;   //Cho phép kích hoạt trong scene
       public  bool isShowing;         // true để mở giao diện UI

        //Các thông tin mở rộng của plugin
        public Dictionary<string, UnityEngine.Object> extraInfoPlugin = new Dictionary<string, UnityEngine.Object>();

        public PluginInfo(string _name, string _version, string _iconPath=null, Dictionary<string, UnityEngine.Object> _extraData=null)
        {
            this.name = _name;
            this.version = _version;
            this.iconPath = _iconPath;
            this.extraInfoPlugin = _extraData;
        }
        public PluginInfo() { }
    }
}
