using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityInjector.Attributes;



[assembly: AssemblyTitle("CMD2.ChisanuManager.Plugin")]
[assembly: AssemblyProduct(CMD2.ChisanuManager.Plugin.ChisanuUI.Version)]
[assembly: AssemblyVersion("0.0.1")]



namespace CMD2.ChisanuManager.Plugin
{

    [PluginFilter("CM3D2x64")]
    [PluginFilter("CM3D2x86")]
    [PluginFilter("CM3D2VRx64")]
    [PluginName(ChisanuUI.PluginName)]
    [PluginVersion(ChisanuUI.Version)]
    public class ChisanuUI : UIDragable
    {
        //Các dữ liệu info
        public const string PluginName = "ChisanuManagerPlugin";
        public const string Version = "0.0.1";
        public static PluginInfo infoPlugin = new PluginInfo(PluginName, Version);
        public Texture2D iconTexture;
        #region  Các thông số điều khiển UI
        //Góc trái phía trên so với chiều cao, rộng
        public float _ratioPosisionMainButton_Heigh = 0.07f;
        public float  _ratioPosisionMainButton_Width=0.94f;

        //Tỉ lệ kích thước so với màn hình
        public float _ratioSizeMainButton = 0.08f;

        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionMainButon = Vector2.zero;
        private Rect _rectMainButton;
        float _scaleUI = 1;
        float _top, _left, _size;
        #endregion

        #region Các cờ điều khiển
        private bool isAllowShow = true;

        private bool _isSmall = true;

        private static ChisanuUI chisanuUI = null;
        #endregion
        void Awake()
        {
            if (chisanuUI == null) chisanuUI = this;
            if (chisanuUI != this) Destroy(this.gameObject);

            if (Manager.manager==null)
            Manager.manager = new Manager();

            DontDestroyOnLoad(this.gameObject);

          byte[]  dataIcon = Convert.FromBase64String(DataStatic.iconMainButtonManager);
            iconTexture = new Texture2D(100, 100, TextureFormat.RGBA32, false);
            iconTexture.LoadImage(dataIcon);
            iconTexture.Apply();


        }
        
        public Rect rectMainButton
        {
            get { return new Rect( left, top, sizeButton, sizeButton); }
            set
            {
                _rectMainButton = value;
                left = value.xMin;
                top = value.yMin;
                sizeButton = value.height;
            }
        }
        private float sizeButton
        {
            get { return _size==0? Screen.height * _ratioSizeMainButton:_size; }
            set
            {
                _size = value;
            }
        }
        private float left
        {
            get { return _left==0? Screen.width * _ratioPosisionMainButton_Width:_left; }
            set { _left = value; }
        }

        private float top {
            get
            {
                return _top == 0 ? Screen.height * _ratioPosisionMainButton_Heigh : _top;
            }
            set
            {
                _top = value;
            }
        }
        void Start()
        {
            Manager.manager.allPlugin = new List<PluginInfo>();
            Manager.manager.allPlugin.Add(new PluginInfo("so 1", "1.0"));
            Manager.manager.allPlugin.Add(new PluginInfo("3 1", "1.0"));
            Manager.manager.allPlugin.Add(new PluginInfo("4 1", "1.0"));
            
        }

        void OnGUI()
        {
            /*
            isAllowShow = GUILayout.Toggle(isAllowShow, "Show");
            if (!isAllowShow) return;

            GUILayout.Label("_ratioPosisionMainButton_Heigh\t"+_ratioPosisionMainButton_Heigh.ToString());
            _ratioPosisionMainButton_Heigh = GUILayout.HorizontalSlider(_ratioPosisionMainButton_Heigh, 0, 1, null);

            GUILayout.Label("_ratioPosisionMainButton_Width\t"+_ratioPosisionMainButton_Width.ToString());       
            _ratioPosisionMainButton_Width = GUILayout.HorizontalSlider(_ratioPosisionMainButton_Width, 0, 1, null);

            GUILayout.Label("size\t" + _ratioSizeMainButton.ToString());
            _ratioSizeMainButton = GUILayout.HorizontalSlider(_ratioSizeMainButton, 0, 1, null);
            */
            if (GUI.Button(rectMainButton= CheckMouse( rectMainButton), iconTexture))
            {
                _isSmall = !_isSmall;
                _scaleUI = _isSmall ? 1 : 0;
            }
            if (_isSmall) return;
            GUI.Box(new Rect(
                left-sizeButton*Manager.manager.allPlugin.Count*_scaleUI,
                top,
                sizeButton*Manager.manager.allPlugin.Count*_scaleUI,
                sizeButton), "");
           // scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width / 3), GUILayout.Height(Screen.height));
            ShowAllPluginInUI();

           // GUILayout.EndScrollView();
           // GUI.EndGroup();

        }

        void ShowAllPluginInUI()
        {
            if(_scaleUI<1)
            {
                _scaleUI += Time.deltaTime;
            }

            for(int i=0; i<Manager.manager.allPlugin.Count; i++)
            {
              if(  GUI.Button(
                  new  Rect(left - sizeButton * (i + 1)*_scaleUI+0.1f*sizeButton,
                  top+ 0.1f * sizeButton, 
                  sizeButton * 0.8f*_scaleUI,
                  sizeButton*0.8f*_scaleUI),
                  Manager.manager.allPlugin[i].name))
                {
                    Debug.LogWarning(Manager.manager.allPlugin[i].name + "\t clicked");
                }
            }
        }

       
    }
    
   
}
