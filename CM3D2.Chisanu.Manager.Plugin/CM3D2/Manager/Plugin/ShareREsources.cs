using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
namespace CMD2.ChisanuManager.Plugin
{
    /// <summary>
    /// Có chức năng load dữ liệu, nhằm hạn chế những hàm nặng gây tốn thời gian
    /// Các plugin khác sẽ yêu cầu dữ liệu gì bằng cách nạp properties cho hàm này trong start
    /// hàm này sẽ đợi 5 fram để lấy hết các yêu cầu rồi chdck những yêu cầu cần gì
    /// trả lại các yêu cầu với các delegate
    /// </summary>
    /// 
    public enum OrderType
    {
        none,
        UpdateListMaid

    }
    public class ManagerResources : MonoBehaviour
    {
        /// <summary>
        /// Các order từ các plugin khác vd (bool)Order["MaidFirst"] = true; là yêu cầu Maid[0];
        /// </summary>
        public Dictionary<OrderType, UnityEngine.Object> Order = new Dictionary<OrderType, UnityEngine.Object>();
        public List<Maid> existMaids = new List<Maid>();

        public static ManagerResources manager { set; get; }
        void Awake()
        {
            if (manager == null) manager = this;
            if (manager != this) Destroy(this.gameObject);
            DontDestroyOnLoad(this.gameObject);

        }


        IEnumerator Start()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            if(Order.ContainsKey( OrderType.UpdateListMaid))
            {
                 UpdateListMaid();
            }
            
        }
        public void UpdateListMaid()
        {
            existMaids = new List<Maid>();
            for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
            {
                Maid existMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
                if (existMaid != null)
                {
                    existMaids.Add(existMaid);
                }
            }
        }

    }
}
