using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameStart : MonoBehaviour {

    class PlayerControlData
    {
        public PlayerControl playerControl;
        public PlayerControlData next;
    }

    private DizhuPai dizhuPai;
    //private PlayerControlData playerControlList = new PlayerControlData();
    void Awake()
    {
        Game.Instance.Init();
        Debug.Log(transform.localEulerAngles);
    }

    void Start()
    {
        Game.Instance.GameBeginDizhuPoker += this.DizhuPoker;
        GameObject prefab = ResLoader.Load("Prefab/Player") as GameObject;
        GameObject player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(-390, 100, 0);
        player.transform.localEulerAngles = new Vector3(0, 0,-90);
        PlayerControl playerControl = player.GetComponent<PlayerControl>();
        playerControl.Player =  (Game.Instance.Player1);
       // playerControlList.playerControl = playerControl;

        player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(0, -330, 0);
        PlayerControl playerControl3 = player.GetComponent<PlayerControl>();
        playerControl3.Player = (Game.Instance.Player3);
        PlayerControlData pcl = new PlayerControlData();
        pcl.playerControl = playerControl3;
       // playerControlList.next = pcl;

        player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(390, 100, 0);
        player.transform.localEulerAngles = new Vector3(0, 0, 90);
        PlayerControl playerControl2 = player.GetComponent<PlayerControl>();
        playerControl2.Player = (Game.Instance.Player2);
        PlayerControlData pcl1 = new PlayerControlData();
        pcl1.playerControl = playerControl2;
       // playerControlList.next.next = pcl1;

       // playerControlList.next.next.next = playerControlList;
        Game.Instance.XiPai();
    }
    

    public void GameBegin(GameObject go)
    {
        go.SetActive(false);
        Game.Instance.DoGameFapai();
        //WaitUntil()

    }

    void DizhuPoker()
    {
        GameObject prefab = ResLoader.Load("Prefab/DiZhuPai") as GameObject;
        GameObject dizhuPaiGo = NGUITools.AddChildNotLoseAnyThing(gameObject, prefab);
        dizhuPai = dizhuPaiGo.GetComponent<DizhuPai>();
        Game.Instance.WhoQiangDizhu();
        //playerControlList.playerControl.BegianQiangDiZhu();
    }

    void SetDizhuPoker(List<poker> list)
    {
      
    }
    void OnDestroy()
    {
         Game.Instance.GameBeginDizhuPoker -= this.DizhuPoker;
    }

//     PlayerControlData GetPlayerControlData(PlayerControl playerControl)
//     {
//         PlayerControlData p = playerControlList;
//         while(p.next!=playerControlList)
//         {
//             if (p.playerControl == playerControl) return p;
//             else p = p.next;
//         }
//         if (p.playerControl == playerControl) return p;
//         return null;
//     }
//     public void QiangDizhu(PlayerControl playerControl)
//     {
//         PlayerControlData p = GetPlayerControlData(playerControl);
//         if (p.playerControl.Beishu == 3 || p.next.playerControl.Beishu == -1)
//         {
//             p.playerControl.AddDizhuPoker();
//         }
//         else 
//         {
//             p.next.playerControl.BegianQiangDiZhu();
//         }
//     }

    public void QiangDizhu(Player player)
    {

    }


}
