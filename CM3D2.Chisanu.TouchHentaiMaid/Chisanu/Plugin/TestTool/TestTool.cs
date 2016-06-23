using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CM3D2.Chisanu.TouchHentaiMaid.Plugin.TestTool
{
    class TestTool:MonoBehaviour
    {

     
            public const string PluginName = "GUIManager";
            public const string Version = "0.0.1";
            string tile = "Dữ lệu bone";
            private AudioClip clips;
            bool _isLOadCompleted = false;

            private GameObject _rawObject = null;
            private Maid _maid;

            Transform transform1;
            Transform transform2;
            string _label;

            AudioSource sources;
            void Start()
            {
                sources = GetComponent<AudioSource>();
                if (sources == null)
                    sources = gameObject.AddComponent<AudioSource>();
                sources.volume = 1;
                sources.loop = true;
                sources.playOnAwake = true;
            }
            public Maid CheckMaids()
            {
                //  List<Maid> existMaids = new List<Maid>();
                for (int nMaidNo = 0; nMaidNo < GameMain.Instance.CharacterMgr.GetMaidCount(); ++nMaidNo)
                {
                    Maid existMaid = GameMain.Instance.CharacterMgr.GetMaid(nMaidNo);
                    if ((UnityEngine.Object)existMaid != (UnityEngine.Object)null)
                    {
                        //  existMaids.Add(existMaid);
                        return existMaid;

                    }
                }
                return null;
            }
            GameObject CreateTestgameObject()
            {
                if (_maid == null) _maid = CheckMaids();
                this._rawObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this._rawObject.name = name;
                this._rawObject.transform.localScale = Vector3.one * _sider;
                this._rawObject.renderer.enabled = true;
                this._rawObject.gameObject.AddComponent<BoxCollider>().isTrigger = true;
                this._rawObject.transform.parent = this._maid.transform;
                _rawObject.AddComponent<TriggerObject>();

                return _rawObject;
            }
            void ClickIntarget(string _target, string _target_2 = null)
            {
                transform1 = null;
                transform2 = null;
                if (_maid == null) CheckMaids();
                if (_rawObject == null) _rawObject = CreateTestgameObject();
                transform1 = CMT.SearchObjName(this._maid.body0.m_Bones.transform, _target, true);
                if (transform1 == null)
                {
                    Debug.LogWarning(_target + "\t not exit");
                    return;
                }
                transform2 = CMT.SearchObjName(this._maid.body0.m_Bones.transform, _target_2, true);
                if (transform2 == null)
                {
                    Debug.LogWarning(_target + "\t not exit");
                    return;
                }
                if (string.IsNullOrEmpty(_target_2) || transform2 == null)
                {
                    _rawObject.transform.position = transform1.position;
                    Debug.LogWarning("Khong co transform 2");

                }
                else
                {
                    _rawObject.transform.position = (transform1.position + transform2.position) / 2;

                }
                this._rawObject.transform.localScale = Vector3.one * _sider;

            }

            public void Update()
            {
                if (transform1 != null)
                {
                    _rawObject.transform.position = (transform1.position + transform2.position) / 2;

                    if (transform2 != null)
                    {
                        this._rawObject.transform.localScale = Vector3.one * _sider;

                        return;
                    }
                    _rawObject.transform.position = transform1.position;

                }
            }
            public Vector2 scrollPosition = Vector2.zero;
            Rect _rect = new Rect(Screen.width / 10, 10, Screen.width / 2, Screen.height - 20);
            bool _isShow = false;
            float _sider;
            public string _dataTest = "datatest here 1";
            public string _dataTest_2 = "datatest here2";
            public string _dataTest_3 = "Slot ID";
            public string _path = "path is here";

            private string _nameOldMusic;
            void OnGUI()
            {
                _isShow = GUILayout.Toggle(_isShow, "Show");
                if (!_isShow) return;
                _path = GUILayout.TextField(_path, 50);

                if (GUILayout.Button("Stop Music"))
                {
                    GameMain.Instance.SoundMgr.StopBGM(0.0f);
                    var _auSources = GameMain.Instance.SoundMgr.GetAudioSourceBgm();
                    if (_auSources != null) _nameOldMusic = _auSources.clip.name;
                    Debug.Log("name of old source " + _auSources);
                }
                if (GUILayout.Button("rePlay Music"))
                {
                    GameMain.Instance.SoundMgr.PlayBGM(_nameOldMusic, 0, true);
                }

                if (GUILayout.Button("Play Music"))
                {
                    StopCoroutine("loadMusic");
                    StartCoroutine("loadMusic", _path);
                }
                if (GUILayout.Button("Play Music from path"))
                {
                    if (_isLOadCompleted) sources.Play();
                }
                _sider = GUILayout.HorizontalSlider(_sider, 0, 2, null);
                GUILayout.Label(_sider.ToString());

                _dataTest = GUILayout.TextField(_dataTest, 25);
                _dataTest_2 = GUILayout.TextField(_dataTest_2, 25);
                _dataTest_3 = GUILayout.TextField(_dataTest_3, 25);
                if (GUILayout.Button(_dataTest))
                {
                    ClickIntarget(_dataTest, _dataTest_2);
                }
                if (GUILayout.Button("Use slot ID"))
                {
                    GetTransWithSlotID(_dataTest_3);
                }
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width / 3), GUILayout.Height(Screen.height));
                GUILayout.Label(tile);
                /* foreach (var _var in m_strDefSlotName)
                 {
                     if (GUILayout.Button(_var))
                     {
                         ClickIntarget(_var);
                     }
                 }*/
                GUILayout.EndScrollView();


            }

            void GetTransWithSlotID(string _idString)
            {
                TBody.SlotID _enum = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), _idString);
                transform1 = null;
                transform2 = null;
                if (_maid == null) CheckMaids();
                if (_rawObject == null) _rawObject = CreateTestgameObject();
                //  transform1= _maid.body0.goSlot[(int)_enum].trsBoneAttach;
                transform1 = _maid.body0.goMoza.transform;
                if (transform1 == null) Debug.LogWarning(_idString + "\t khong ton tai");
            }

            IEnumerator loadMusic(string url)
            {
                _isLOadCompleted = false;
                string _url = string.Format("file://{0}", url);
                WWW www = new WWW(_url);
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                    Debug.LogError(www.error);
                else
                    sources.clip = www.audioClip;
                _isLOadCompleted = true;
            }
        }

    
}
