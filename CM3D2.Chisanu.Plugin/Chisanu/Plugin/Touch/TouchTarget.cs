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

    /// <summary>
    /// là một đơn vị cảm biến
    /// </summary>
    public class TouchTarget
    {
        /// <summary>
        /// Tốt nhất là thêm 1 array transform để ràng buộc vị trí (2 trans cho anal)
        /// </summary>


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

        /// <summary>
        /// Hàm này rất nặng, vì thế không nên dùng trong Update
        /// Đối với đối tượng mà có 1 trans ràng buộc vị trí, thì nên ràng buộc cha con
        /// Đối với đ tượng nhiều ràng buộc vị trí thì nên ràng buộc trong update
        /// </summary>

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

        /// <summary>
        /// Phải chạy, do các class nắm giữ không thể destroy
        /// </summary>
        public void Destroy()
        {
            if (!((UnityEngine.Object)this._rawObject != (UnityEngine.Object)null))
                return;
            UnityEngine.Object.Destroy((UnityEngine.Object)this._rawObject);
        }
    }
}