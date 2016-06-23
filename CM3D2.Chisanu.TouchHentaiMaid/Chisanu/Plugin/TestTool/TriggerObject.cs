using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CM3D2.Chisanu.TouchHentaiMaid.Plugin.TestTool
{
    public class TriggerObject : MonoBehaviour
    {
        public Renderer rend;
        void OnMouseDown()
        {
            Debug.LogWarning("OnmouseDown\t" + Input.mousePosition);
        }
        void OnMouseDrag()
        {
            // rend.material.color -= Color.white * Time.deltaTime;
        }
    }
}
