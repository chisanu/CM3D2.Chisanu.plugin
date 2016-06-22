using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using UnityObsoleteGui.CM3D2.AddYotogiSlider.Plugin;
[PluginFilter("CM3D2x64"), PluginName("Touch Communication"), PluginVersion("1.0.0.1"), PluginFilter("CM3D2x86")]
public class TouchCommunication : PluginBase
{
    // Fields
    private int currentLevel = 0;
    private int fpsMax = 60;
    private int frameCount = 0;
    private GameObject hitTarget;
    private List<TouchableMaid> maids = new List<TouchableMaid>();
    private static List<string> motions = new List<string>();
    private bool mouseClicking;
    private bool mouseLongPressing;
    private bool mousePressing;
    private int mousePressingFrames;
    private static List<string> sounds = new List<string>();

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
        this.maids.RemoveAll(delegate (TouchableMaid m) {
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
        this.maids.ForEach(delegate (TouchableMaid m) {
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
        this.maids.RemoveAll(delegate (TouchableMaid m) {
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
                if (ControlSignal.delegateLeftClick_X_Update != null&&maid !=null)
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
                if (ControlSignal.delegateRemoveLeftClick_X != null&&hitTarget!=null)
                {
                    ControlSignal.delegateRemoveLeftClick_X(hitTarget.name);
                }
                this.hitTarget = null;

            }
        }
    }

    // Nested Types
    private enum ActivityType
    {
        Click,
        DoubleClick,
        Drag,
        LongClick
    }

    private enum Personality
    {
        Anesan = 4,
        Cool = 2,
        Other = 0x63,
        Pride = 0,
        Pure = 1,
        Yandere = 3
    }

    private static class Settings
    {
        // Fields
        private static XDocument _xml;
        private static string _xmlFile = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Config\TouchCommunication.xml");

        // Methods
        public static void Load()
        {
            try
            {
                _xml = XDocument.Load(_xmlFile);
            }
            catch
            {
                _xml = new XDocument(new XDeclaration("1.0", "utf-8", null), new object[] { new XElement("TouchCommunication", new object[] { new XElement("Keys", new XElement("Key", new object[] { new XAttribute("Function", "PluginToggle"), new XAttribute("Code", "f11") })), new XElement("PluginEnabled", "true") }) });
            }
        }

        public static bool PluginEnabledToggle()
        {
            PluginEnabled = !PluginEnabled;
            return PluginEnabled;
        }

        public static void Save()
        {
            try
            {
                _xml.Save(_xmlFile);
            }
            catch
            {
            }
        }

        // Properties
        public static string KeyPluginToggle
        {
            get
            {
                return (from e in _xml.Descendants("Key")
                        where e.Attribute("Function").Value == "PluginToggle"
                        select e.Attribute("Code").Value).FirstOrDefault<string>();
            }
        }

        public static bool PluginEnabled
        {
            get
            {
                return (_xml.Descendants("PluginEnabled").FirstOrDefault<XElement>().Value == "true");
            }
            set
            {
                _xml.Descendants("PluginEnabled").FirstOrDefault<XElement>().Value = value ? "true" : "false";
            }
        }
    }

    private class TouchableMaid
    {
        // Fields
        private bool _isFaceChanged = false;
        private bool _isSpeakingLoop;
        private jiggleBone _jbMuneL;
        private jiggleBone _jbMuneR;
        private Maid _maid;
        private int _mureYureTimer = -1;
        private string _originalPose;
        private int _persistPoseSeconds;
        private TouchCommunication.Personality _personal;
        private int _preExciteLevel = -1;
        private List<TouchCommunication.TouchTarget> _touchTargets = new List<TouchCommunication.TouchTarget>();
        private bool _waiting = false;

        // Methods
        public TouchableMaid(Maid maid)
        {
            this._maid = maid;
            try
            {
                this._personal = (TouchCommunication.Personality)Enum.Parse(typeof(TouchCommunication.Personality), this._maid.Param.status.personal.ToString());
            }
            catch
            {
                this._personal = TouchCommunication.Personality.Other;
            }
            this._persistPoseSeconds = -1;
            this._touchTargets.Add(new TouchCommunication.TouchTarget(this._maid, "target_mune", new Vector3(0.1f, 0.1f, 0.1f), "_IK_muneR", null));
            this._touchTargets.Add(new TouchCommunication.TouchTarget(this._maid, "target_mune", new Vector3(0.1f, 0.1f, 0.1f), "_IK_muneL", null));
            this._touchTargets.Add(new TouchCommunication.TouchTarget(this._maid, "target_hip", new Vector3(0.12f, 0.12f, 0.12f), "_IK_hipL", "Hip_L"));
            this._touchTargets.Add(new TouchCommunication.TouchTarget(this._maid, "target_hip", new Vector3(0.12f, 0.12f, 0.12f), "_IK_hipR", "Hip_R"));
            this._touchTargets.Add(new TouchCommunication.TouchTarget(this._maid, "target_vagina", new Vector3(0.08f, 0.08f, 0.08f), "Bip01 Pelvis", null));
        }

        public void AttachTargets()
        {
            foreach (TouchCommunication.TouchTarget target in this._touchTargets)
            {
                if (target != null)
                {
                    target.Attach();
                }
            }
        }

        private void ChangeFace(bool afterTouch)
        {
            if (afterTouch)
            {
                if (this.ExciteLevel == 0)
                {
                    this._maid.FaceAnime("目を見開いて", 0f, 0);
                    this._maid.FaceBlend("頬０涙０");
                }
                else if (this.ExciteLevel == 1)
                {
                    this._maid.FaceAnime("拗ね", 0f, 0);
                    this._maid.FaceBlend("頬１涙０");
                }
                else if (this.ExciteLevel == 2)
                {
                    this._maid.FaceAnime("エロ痛み１", 0f, 0);
                    this._maid.FaceBlend("頬２涙０");
                }
                else if (this.ExciteLevel == 3)
                {
                    this._maid.FaceAnime("エロ痛み２", 0f, 0);
                    this._maid.FaceBlend("頬３涙０");
                }
                else if (this.ExciteLevel == 4)
                {
                    this._maid.FaceAnime("エロ絶頂", 0f, 0);
                    this._maid.FaceBlend("頬３涙１");
                }
            }
            else if (this.ExciteLevel == 0)
            {
                this._maid.FaceAnime("むー", 0f, 0);
                this._maid.FaceBlend("頬０涙０");
            }
            else if (this.ExciteLevel == 1)
            {
                this._maid.FaceAnime("少し怒り", 0f, 0);
                this._maid.FaceBlend("頬１涙０");
            }
            else if (this.ExciteLevel == 2)
            {
                this._maid.FaceAnime("エロ興奮２", 0f, 0);
                this._maid.FaceBlend("頬２涙０");
            }
            else if (this.ExciteLevel == 3)
            {
                this._maid.FaceAnime("エロ興奮３", 0f, 0);
                this._maid.FaceBlend("頬３涙０");
            }
            else if (this.ExciteLevel == 4)
            {
                this._maid.FaceAnime("エロ放心", 0f, 0);
                this._maid.FaceBlend("頬３涙１");
            }
        }

        public void CheckMotion()
        {
            if (this.PersistPoseSeconds == 0)
            {
                this._maid.CrossFade(this.OriginalPose, false, true, false, 1f, 1f);
                this.PersistPoseSeconds = -1;
                if (this.ExciteLevel == 4)
                {
                    this._maid.Param.SetCurExcite(0);
                    this.ChangeFace(false);
                    this.Waiting = false;
                }
            }
            else if (this.PersistPoseSeconds > 0)
            {
                this.PersistPoseSeconds--;
            }
            if (!(!this._isFaceChanged || this._maid.AudioMan.isPlay()))
            {
                this.ChangeFace(false);
                this._isFaceChanged = false;
            }
        }

        public void CheckMuneYure()
        {
            if (this._mureYureTimer == 0)
            {
                if (this._jbMuneL != null)
                {
                    this._jbMuneL.bGravity = 0.1f;
                    this._jbMuneL.bDamping = 0.3f;
                    this._jbMuneR.bGravity = 0.1f;
                    this._jbMuneR.bDamping = 0.3f;
                }
                this._mureYureTimer = -1;
            }
            else if (this._mureYureTimer != -1)
            {
                this._mureYureTimer--;
            }
        }

        public void CheckSpeak(GameObject target)
        {
            if ((((this._maid != null) && this._isSpeakingLoop) && (((target != null) && (target.transform.parent.name != this._maid.name)) || (target == null))) && ((this._maid.AudioMan != null) && (this._maid.AudioMan.isLoop() && this._maid.AudioMan.isPlay())))
            {
                this._maid.AudioMan.Stop(0.5f);
            }
        }

        public void DestroyTargets()
        {
            foreach (TouchCommunication.TouchTarget target in this._touchTargets)
            {
                if (target != null)
                {
                    target.Destroy();
                }
            }
        }

        public void Reset()
        {
            if (this._jbMuneL != null)
            {
                this._jbMuneL.bGravity = 0.1f;
                this._jbMuneL.bDamping = 0.3f;
                this._jbMuneR.bGravity = 0.1f;
                this._jbMuneR.bDamping = 0.3f;
            }
            if (this._isSpeakingLoop)
            {
                this._maid.AudioMan.Stop(0.5f);
            }
            if (!string.IsNullOrEmpty(this.OriginalPose))
            {
                this._maid.CrossFade(this.OriginalPose, false, true, false, 1f, 1f);
            }
        }

        private void Speak(string part, TouchCommunication.ActivityType activity)
        {
            var typeArray = new[] {
                new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "target_mune", ExciteLevel = 1, OggNames = new string[] { "S0_05680" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "target_vagina", ExciteLevel = 1, OggNames = new string[] { "S0_05684" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 0, OggNames = new string[] { "S0_01959", "S0_05667", "S0_05671", "S0_05674", "S0_05682", "S0_05683", "S0_08228", "S0_08333", "S0_09839", "S0_13023", "S0_14302", "S0_14556", "S0_14612", "S0_14696", "S0_05668" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 1, OggNames = new string[] { "S0_01950", "S0_05672", "S0_08326", "S0_09750", "S0_09987", "S0_13023", "S0_14416", "S0_14417", "S0_14730", "S0_05670" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 2, OggNames = new string[] { "S0_01888", "S0_05669", "S0_05673", "S0_05685", "S0_09695", "S0_09826", "S0_09834", "S0_13792", "S0_14644", "S0_14786" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 3, OggNames = new string[] { "S0_01912", "S0_05674", "S0_05686", "S0_09696", "S0_09698", "S0_09841", "S0_10025", "S0_14457", "S0_14651", "S0_14811" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 4, OggNames = new string[] { "S0_09450", "S0_09670", "S0_09868", "S0_14534" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 0, OggNames = new string[] { "S0_01316", "S0_01318", "S0_01320", "S0_01321", "S0_01322", "S0_01340" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 1, OggNames = new string[] { "S0_01312", "S0_01323" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 2, OggNames = new string[] { "S0_01346" } }, new { Personal = TouchCommunication.Personality.Pride, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 3, OggNames = new string[] { "S0_01327", "S0_01347" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "target_mune", ExciteLevel = 1, OggNames = new string[] { "S2_04511" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "target_vagina", ExciteLevel = 1, OggNames = new string[] { "S2_04514" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 0, OggNames = new string[] { "S2_04501", "S2_04502", "S2_04509", "S2_04512", "S2_04513", "S2_09120", "S2_09124", "S2_09822", "S2_09949", "S2_14407", "S2_14702" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 1, OggNames = new string[] { "S2_01390", "S2_01472", "S2_04498", "S2_04502", "S2_04503", "S2_04510", "S2_04516", "S2_09982", "S2_10045", "S2_14406", "S2_14740" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 2, OggNames = new string[] { "S2_01472", "S2_01474", "S2_01477", "S2_04499", "S2_04504", "S2_04515", "S2_09817", "S2_10044", "S2_14616", "S2_14645", "S2_14746", "S2_14761" } },
                new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 3, OggNames = new string[] { "S2_01393", "S2_01474", "S2_01486", "S2_05137", "S2_10025", "S2_11967", "S2_14429" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 4, OggNames = new string[] { "S2_09355", "S2_09445", "S2_09448", "S2_09451", "S2_09454", "S2_09457", "S2_09852", "S2_14506" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 0, OggNames = new string[] { "S2_01152" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 1, OggNames = new string[] { "S2_01150", "S2_01492" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 2, OggNames = new string[] { "S2_01174", "S2_01493", "S2_01505" } }, new { Personal = TouchCommunication.Personality.Pure, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 3, OggNames = new string[] { "S2_01327", "S2_01176", "S2_01496" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "target_mune", ExciteLevel = 1, OggNames = new string[] { "S1_05478" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "target_mune", ExciteLevel = 4, OggNames = new string[] { "S1_09516" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 0, OggNames = new string[] { "S1_05464", "S1_05465", "S1_05468", "S1_09270", "S1_10049", "S1_10113", "S1_14642", "S1_14854", "S1_14866" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 1, OggNames = new string[] { "S1_05470", "S1_05479", "S1_05480", "S1_09926", "S1_10039", "S1_10112", "S1_14617", "S1_14619", "S1_14793", "S1_14837", "S1_14845", "S1_14856", "S1_14857" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 2, OggNames = new string[] { "S1_05477", "S1_05482", "S1_09691", "S1_09889", "S1_13078", "S1_14416", "S1_14707", "S1_14805", "S1_14838" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 3, OggNames = new string[] { "S1_05467", "S1_05471", "S1_09188", "S1_09501", "S1_09507", "S1_09510", "S1_09884", "S1_09891", "S1_14599", "S1_14708", "S1_14849" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Click, Part = "general", ExciteLevel = 4, OggNames = new string[] { "S1_09919", "S1_10078", "S1_14597", "S1_14704" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 0, OggNames = new string[] { "S1_03264" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 1, OggNames = new string[] { "S1_02311", "S1_02382" } }, new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 2, OggNames = new string[] { "S1_02381", "S1_02384" } },
                new { Personal = TouchCommunication.Personality.Cool, Activity = TouchCommunication.ActivityType.Drag, Part = "general", ExciteLevel = 3, OggNames = new string[] { "S1_02315" } }
             };
            IEnumerable<string> source = typeArray.Where(_ => _.Personal == this.Personal && _.Activity == activity && (_.Part == part || _.Part == "general") && (_.ExciteLevel == this.ExciteLevel || _.ExciteLevel == -1)).SelectMany(o => (IEnumerable<string>)o.OggNames).Intersect<string>((IEnumerable<string>)TouchCommunication.sounds);
            if (source.Count<string>() > 0)
            {
                string str = (from i in source
                              orderby Guid.NewGuid()
                              select i).First<string>();
                if (activity == TouchCommunication.ActivityType.Drag)
                {
                    if (!(this._maid.AudioMan.isPlay() && (this._preExciteLevel == this.ExciteLevel)))
                    {
                        this._maid.AudioMan.LoadPlay(str + ".ogg", 0.5f, true, true);
                        this._isSpeakingLoop = true;
                    }
                }
                else
                {
                    this._maid.AudioMan.LoadPlay(str + ".ogg", 0.5f, true, false);
                    this._isSpeakingLoop = false;
                }
            }
        }

        public void Touch(string part, TouchCommunication.ActivityType activity)
        {
            if ((activity == TouchCommunication.ActivityType.Click) && (part == "target_mune"))
            {
                this._jbMuneL = TouchCommunication.GetFieldValue<TBody, jiggleBone>(this._maid.body0, "jbMuneL");
                this._jbMuneR = TouchCommunication.GetFieldValue<TBody, jiggleBone>(this._maid.body0, "jbMuneR");
                if (this._jbMuneL != null)
                {
                    this._jbMuneL.bGravity = 0.3f;
                    this._jbMuneL.bDamping = 0.3f;
                    this._jbMuneR.bGravity = 0.3f;
                    this._jbMuneR.bDamping = 0.3f;
                    this._mureYureTimer = 10;
                }
            }
            this._preExciteLevel = this.ExciteLevel;
            int num = 0;
            if (part == "target_mune")
            {
                num = 7;
            }
            else if (part == "target_hip")
            {
                num = 5;
            }
            else if (part == "target_vagina")
            {
                num = 9;
            }
            this._maid.Param.AddCurExcite(num);
            if (!this.Waiting)
            {
                this.ChangeFace(true);
                this._isFaceChanged = true;
                if (this.ExciteLevel == 4)
                {
                    this.Speak(part, TouchCommunication.ActivityType.Click);
                }
                else
                {
                    this.Speak(part, activity);
                }
                if (this.ExciteLevel == 4)
                {
                    if (string.IsNullOrEmpty(this.OriginalPose))
                    {
                        this.OriginalPose = this._maid.body0.LastAnimeFN;
                    }
                    // Nếu dc cho phép trong Hscene mới dc ra
                    if (ControlSignal._isAllowIkumInHScene)
                    {
                        if (part == "target_vagina")
                        {
                            this._maid.CrossFade("aibu_cli_seikantaizeccyou_f_once_.anm", false, false, false, 1f, 1f);
                            this._maid.CrossFade("aibu_zeccyougo_f.anm", false, true, true, 0.5f, 1f);
                        }
                        else if (part == "target_mune")
                        {
                            this._maid.CrossFade("aibu_tikubi_seikantaizeccyou_f_once_.anm", false, false, false, 1f, 1f);
                            this._maid.CrossFade("aibu_tikubi_seikantaizeccyougo_f.anm", false, true, true, 0.5f, 1f);
                        }
                        else
                        {
                            this._maid.CrossFade("aibu_hibu_zeccyou_f_once_.anm", false, false, false, 3f, 1f);
                            this._maid.CrossFade("aibu_zeccyougo_f.anm", false, true, true, 0.5f, 1f);
                        }
                        this._maid.AddPrefab("Particle/pSio2_cm3D2", "pSio2_cm3D2", "_IK_vagina", new Vector3(0f, 0f, -0.01f), new Vector3(0f, 180f, 0f));
                        if ((part == "target_vagina") && TouchCommunication.GetRandomBool(50))
                        {
                            this._maid.AddPrefab("Particle/pNyou_cm3D2", "pNyou_cm3D2", "_IK_vagina", new Vector3(0f, -0.047f, 0.011f), new Vector3(20f, -180f, 180f));
                            GameMain.Instance.SoundMgr.PlaySe("SE011.ogg", false);
                        }
                        this.PersistPoseSeconds = 15;
                        this.Waiting = true;
                    }
                    
                }
                else if ((this.PersistPoseSeconds == -1) && (activity == TouchCommunication.ActivityType.Click))
                {
                    Predicate<string> match = null;
                    Predicate<string> predicate2 = null;
                    string search;
                    this.OriginalPose = this._maid.body0.LastAnimeFN;
                    string[] strArray = Path.GetFileNameWithoutExtension(this.OriginalPose).Split(new char[] { '_' });
                    if (strArray.Length == 1)
                    {
                        search = strArray[0];
                    }
                    else if (strArray.Length > 1)
                    {
                        search = strArray[0] + "_" + strArray[1];
                    }
                    else
                    {
                        search = "";
                    }
                    if (TouchCommunication.GetRandomBool(50) && (this.ExciteLevel <= 1))
                    {
                        if (match == null)
                        {
                            match = m => m.Contains("itazura_once") && m.Contains(search);
                        }
                        string str = TouchCommunication.motions.Find(match);
                        if (str != null)
                        {
                            this._maid.CrossFade(str + ".anm", false, false, false, 0.5f, 1f);
                            this.PersistPoseSeconds = 5;
                        }
                        if (predicate2 == null)
                        {
                            predicate2 = m => m.Contains("itazurago") && m.Contains(search);
                        }
                        string str2 = TouchCommunication.motions.Find(predicate2);
                        if (str2 != null)
                        {
                            this._maid.CrossFade(str2 + ".anm", false, true, true, 1f, 1f);
                        }
                    }
                }
            }
        }

        // Properties
        public int ExciteLevel
        {
            get
            {
                int num = this._maid.Param.status.cur_excite;
                if (num < 10)
                {
                    return 0;
                }
                if (num < 100)
                {
                    return 1;
                }
                if (num < 200)
                {
                    return 2;
                }
                if (num < 300)
                {
                    return 3;
                }
                return 4;
            }
        }

        public bool IsSpeakingLoop
        {
            get
            {
                return this._isSpeakingLoop;
            }
            set
            {
                this._isSpeakingLoop = value;
            }
        }

        public Maid Maid
        {
            get
            {
                return this._maid;
            }
        }

        public string OriginalPose
        {
            get
            {
                return this._originalPose;
            }
            set
            {
                this._originalPose = value;
            }
        }

        public int PersistPoseSeconds
        {
            get
            {
                return this._persistPoseSeconds;
            }
            set
            {
                this._persistPoseSeconds = value;
            }
        }

        public TouchCommunication.Personality Personal
        {
            get
            {
                return this._personal;
            }
            set
            {
                this._personal = value;
            }
        }

        public List<TouchCommunication.TouchTarget> TouchTargets
        {
            get
            {
                return this._touchTargets;
            }
        }

        public bool Waiting
        {
            get
            {
                return this._waiting;
            }
            set
            {
                this._waiting = value;
            }
        }
    }

    private class TouchTarget
    {
        // Fields
        private Maid _maid;
        private string _part1Name;
        private string _part2Name;
        private GameObject _rawObject;

        // Methods
        public TouchTarget(Maid maid, string name, Vector3 scale, string part1Name, string part2Name)
        {
            this._maid = maid;
            this._part1Name = part1Name;
            this._part2Name = part2Name;
            this._rawObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            this._rawObject.name = name;
            this._rawObject.transform.localScale = scale;
            this._rawObject.renderer.enabled = false;
            this._rawObject.gameObject.AddComponent<BoxCollider>().isTrigger = true;
            this._rawObject.transform.parent = this._maid.transform;
        }

        public void Attach()
        {
            Transform transform = null;
            Transform transform2 = null;
            try
            {
                transform = CMT.SearchObjName(this._maid.body0.m_Bones.transform, this._part1Name, true);
            }
            catch
            {
            }
            if (this._part2Name != null)
            {
                try
                {
                    transform2 = CMT.SearchObjName(this._maid.body0.m_Bones.transform, this._part2Name, true);
                }
                catch
                {
                }
            }
            if ((transform != null) && (transform2 != null))
            {
                Vector3 position = transform2.transform.position;
                Vector3 vector2 = transform.transform.position;
                this._rawObject.transform.position = new Vector3((vector2.x + position.x) / 2f, (vector2.y + position.y) / 2f, (vector2.z + position.z) / 2f);
            }
            else if (transform != null)
            {
                this._rawObject.transform.position = transform.transform.position;
            }
        }

        public void Destroy()
        {
            if (!((UnityEngine.Object)this._rawObject != (UnityEngine.Object)null))
                return;
            UnityEngine.Object.Destroy((UnityEngine.Object)this._rawObject);
        }
    }
}




