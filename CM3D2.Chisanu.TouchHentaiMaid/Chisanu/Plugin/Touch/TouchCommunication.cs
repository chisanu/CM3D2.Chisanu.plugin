using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
namespace CM3D2.Chisanu.Plugin.Touch
{
   // [PluginFilter("CM3D2x64"), PluginName("Touch Communication"), PluginVersion("1.0.0.1"), PluginFilter("CM3D2x86")]
    public class TouchCommunication : PluginBase
    {
        // Fields
        private int currentLevel = 0;   // Giữ level hiện tại
        private int fpsMax = 60;        // Đơn vị chuẩn để tính toán là Drag hay click
        private int frameCount = 0;     // Đếm frame, so với Max CHÚ Ý CẦN LOẠI BỎ THẰNG NÀY, NÓ CHECK TRONG UPDATE

        private GameObject hitTarget;   // Gameobject làm triggerObject, là khối cầu để raycast

        /// <summary>
        /// Mỗi maid sẽ được gán 1 touchablemaid
        /// list này nên được cập nhật khi có maid mới vào scene
        /// </summary>
        private List<TouchableMaid> maids = new List<TouchableMaid>();

        /// <summary>
        /// Nhấn xuống= pressing: bắt đầu đếm frame
        /// Trong pressing nếu mouseup thì : hủy pressing+clicking= true nếu fram đếm thấp;
        /// như trên nhưng nếu framde đếm cao ,thì là drag= longPressing
        /// </summary>
        private bool mouseClicking;
        private bool mouseLongPressing;
        private bool mousePressing;
        private int mousePressingFrames;

        /// <summary>
        /// Danh sách motion và sounds
        /// </summary>
        public static List<string> motions = new List<string>();
        public static List<string> sounds = new List<string>();

        // Methods
        public void Awake()
        {
            DontDestroyOnLoad(this);
            motions = Enumerable.Select<string, string>(from f in GameUty.FileSystem.GetList("", AFileSystemBase.ListType.AllFile)
                                                        where f.EndsWith(".anm")
                                                        select f, new Func<string, string>(Path.GetFileNameWithoutExtension)).ToList<string>();
            sounds = (from f in GameUty.FileSystem.GetList("", AFileSystemBase.ListType.AllFile)
                      where f.EndsWith(".ogg")
                      select Path.GetFileNameWithoutExtension(f).ToUpper()).ToList<string>();
        }

        private void CheckMaids()
        {
            List<Maid> existMaids = new List<Maid>();
            for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
            {
                Predicate<TouchableMaid> match = null;
                Maid existMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
                if (existMaid != null)
                {
                    existMaids.Add(existMaid);
                    if (match == null)
                    {
                        match = m => (m.Maid != null) && (m.Maid.name == existMaid.name);
                    }
                    if (!this.maids.Exists(match))
                    {
                        this.maids.Add(new TouchableMaid(existMaid));
                        Console.WriteLine("Touch：" + existMaid.name + "をタッチ対象として追加完了。");
                    }
                }
            }
            this.maids.RemoveAll(delegate (TouchableMaid m)
            {
                if (!existMaids.Contains(m.Maid))
                {
                    if (m.Maid != null)
                    {
                        Console.WriteLine("Touch：" + m.Maid.name + "をタッチ対象から除外。");
                    }
                    m.DestroyTargets();
                    return true;
                }
                return false;
            });
            this.maids.ForEach(delegate (TouchableMaid m)
            {
                m.AttachTargets();
                m.CheckMotion();
                m.CheckSpeak(this.hitTarget);
            });
        }

        private void CheckMaidsMuneYure()
        {
            this.maids.ForEach(m => m.CheckMuneYure());
        }

        private void FinalaizePlugin()
        {
            this.maids.RemoveAll(delegate (TouchableMaid m)
            {
                Maid maid = m.Maid;
                m.Reset();
                if (maid != null)
                {
                    Console.WriteLine("Touch：" + m.Maid.name + "をタッチ対象から除外。");
                }
                m.DestroyTargets();
                return true;
            });
        }

        internal static FieldInfo GetFieldInfo<T>(string name)
        {
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            return typeof(T).GetField(name, bindingAttr);
        }

        internal static TResult GetFieldValue<T, TResult>(T inst, string name)
        {
            if (inst == null)
            {
                return default(TResult);
            }
            FieldInfo fieldInfo = GetFieldInfo<T>(name);
            if (fieldInfo == null)
            {
                return default(TResult);
            }
            return (TResult)fieldInfo.GetValue(inst);
        }

        private GameObject GetHitTarget(Vector3 inputPosition)
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(inputPosition);
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, float.PositiveInfinity))
                {
                    return hitInfo.collider.gameObject;
                }
                return null;
            }
            return null;
        }

        internal static bool GetRandomBool(int probability)
        {
            return ((from i in Enumerable.Range(1, 100)
                     orderby Guid.NewGuid()
                     select i).First<int>() <= probability);
        }

        private void InitializePlugin()
        {
        }

        public void LateUpdate()
        {
        }

        public void OnApplicationQuit()
        {
            Settings.Save();
        }

        public void OnLevelWasLoaded(int level)
        {
            this.currentLevel = level;
            this.maids.ForEach(m => m.DestroyTargets());
            this.maids = new List<TouchableMaid>();
            this.mousePressing = false;
            this.mouseLongPressing = false;
            this.mouseClicking = false;
            this.mousePressingFrames = 0;
            this.hitTarget = null;
        }

        internal static void SetFieldValue<T>(object inst, string name, object val)
        {
            FieldInfo fieldInfo = GetFieldInfo<T>(name);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(inst, val);
            }
        }

        public void Start()
        {
            Settings.Load();
            if (Settings.PluginEnabled)
            {
                this.InitializePlugin();
            }
        }

        public void Update()
        {

            Predicate<TouchableMaid> match = null;
            if ((Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) && Input.GetKeyDown(Settings.KeyPluginToggle))
            {
                if (Settings.PluginEnabledToggle())
                {
                    GameMain.Instance.SoundMgr.PlaySe("SE001.ogg", false);
                    this.InitializePlugin();
                }
                else
                {
                    GameMain.Instance.SoundMgr.PlaySe("SE003.ogg", false);
                    this.FinalaizePlugin();
                }
            }
            if (Settings.PluginEnabled)
            {
                this.CheckMaidsMuneYure();
                ///Dm không thể tin nổi là nó check event trong update, cứ mỗi giây check 1 lần vl thật
                if (this.frameCount == this.fpsMax)
                {
                    this.frameCount = 0;
                    this.CheckMaids();
                }
                else
                {
                    this.frameCount++;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    this.hitTarget = this.GetHitTarget(Input.mousePosition);
                    if (this.hitTarget != null)
                    {
                        this.mousePressing = true;
                    }
                }
                else if (this.mousePressing && Input.GetMouseButtonUp(0))
                {
                    this.mousePressing = false;
                    this.mouseLongPressing = false;
                    if (this.mousePressingFrames <= (this.fpsMax / 4))
                    {
                        this.mouseClicking = true;
                    }
                    this.mousePressingFrames = 0;
                }
                if (this.mousePressing)
                {
                    this.hitTarget = this.GetHitTarget(Input.mousePosition);
                    if (this.mousePressingFrames > (this.fpsMax / 4))
                    {
                        this.mouseLongPressing = true;
                    }
                    this.mousePressingFrames++;
                }
                if (this.hitTarget != null)
                {
                    if (match == null)
                    {
                        match = m => ((m.Maid != null) && (this.hitTarget.transform.parent != null)) && (m.Maid.name == this.hitTarget.transform.parent.name);
                    }



                    TouchableMaid maid = this.maids.Find(match);
                    ///Cập nhật lại các thông số, để tăng độ rộng vùng H
                    if (ControlSignal.delegateLeftClick_X_Update != null && maid != null)
                    {
                        ControlSignal.delegateLeftClick_X_Update(hitTarget.name);
                    }
                    if (this.mouseClicking)
                    {
                        if (maid != null)
                        {
                            maid.Touch(this.hitTarget.name, ActivityType.Click);
                            if (ControlSignal.delegateLeftClick_X != null && maid != null)
                            {
                                ControlSignal.delegateLeftClick_X(hitTarget.name);
                            }

                        }
                        this.mouseClicking = false;
                    }
                    else if ((this.mouseLongPressing && ((this.mousePressingFrames % this.fpsMax) == 0)) && (maid != null))
                    {
                        maid.Touch(this.hitTarget.name, ActivityType.Drag);
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {

                    // khai báo không chạm nữa
                    if (ControlSignal.delegateRemoveLeftClick_X != null && hitTarget != null)
                    {
                        ControlSignal.delegateRemoveLeftClick_X(hitTarget.name);
                    }
                    this.hitTarget = null;

                }
            }
        }

    }
}