using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CM3D2.Chisanu.Plugin.Touch
{
   public class ControlSignal
    {
        /// <summary>
        /// Cho phép ra trong Hscene
        /// </summary>
        public static bool _isAllowIkumInHScene = false;
        /// <summary>
        /// Kích hoạt đồng thời mở rộng X, và tăng tiếng nói khi chạm vào vùng X
        /// </summary>
        public static OneParamMethod<string> delegateLeftClick_X = null;
        /// <summary>
        /// target_mune target_hip target_vagina là 3 key
        /// </summary>
        public static OneParamMethod<string> delegateRemoveLeftClick_X = null;
        public static OneParamMethod<string> delegateLeftClick_X_Update = null;

        /// <summary>
        /// Dùng để giữ tốc độ khi H
        /// </summary>
        public static OneParamMethod<KeyCode> delegateRightClickAnyWhere = null;

        /// <summary>
        /// Tăng tốc độ khi H
        /// </summary>
        public static TwoParamMethod<KeyCode, KeyCode> delegate_LeftRight_ClickAnyWhere = null;
    }
}
