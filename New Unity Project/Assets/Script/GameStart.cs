using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameStart : MonoBehaviour {

    private DizhuPai dizhuPai;
    private GameObject alreadyChuPokersGo;
    void Awake()
    {
       // Game.Instance.Init();
       /// Debug.Log(transform.localEulerAngles);
       Client.Instance.Init();
       ; Client.NetWork.Connect("127.0.0.1", 9999);
        Client.Instance.Create += new Client.OnCreate(CreatePlayer);
    }

    void Start()
    {
//         Game.Instance.GameBeginDizhuPoker += this.DizhuPoker;
//         GameObject prefab = ResLoader.Load("Prefab/Player") as GameObject;
//         GameObject player = NGUITools.AddChild(gameObject, prefab);
//         player.transform.localPosition = new Vector3(-390, 100, 0);
//         player.transform.localEulerAngles = new Vector3(0, 0,-90);
//         PlayerControl playerControl1 = player.GetComponent<PlayerControl>();
//         playerControl1.Player = (Game.Instance.Player1);
// 
//         player = NGUITools.AddChild(gameObject, prefab);
//         player.transform.localPosition = new Vector3(0, -330, 0);
//         PlayerControl playerControl3 = player.GetComponent<PlayerControl>();
//         playerControl3.Player = (Game.Instance.Player3);
// 
//         player = NGUITools.AddChild(gameObject, prefab);
//         player.transform.localPosition = new Vector3(390, 100, 0);
//         player.transform.localEulerAngles = new Vector3(0, 0, 90);
//         PlayerControl playerControl2 = player.GetComponent<PlayerControl>();
//         playerControl2.Player = (Game.Instance.Player2);
//         Game.Instance.XiPai();
    }
    
    void CreatePlayer(int value)
    {
        DIRECTION ditrection = (DIRECTION)value;
        GameObject playerGo = null;
        GameObject prefab = ResLoader.Load("Prefab/Player") as GameObject;
        playerGo = NGUITools.AddChild(gameObject, prefab);
        if(ditrection == DIRECTION.EAST)
        {

            playerGo.transform.localPosition = new Vector3(-390, 100, 0);
            playerGo.transform.localEulerAngles = new Vector3(0, 0, -90);
            
            
        }
        else if(ditrection == DIRECTION.EAST)
        {
            playerGo.transform.localPosition = new Vector3(0, -330, 0);
        }
        else
        {
            playerGo.transform.localPosition = new Vector3(390, 100, 0);
            playerGo.transform.localEulerAngles = new Vector3(0, 0, 90);
        }
        Player player = new Player(ditrection);
        Game.Instance.listPlayer[(int)(ditrection)] = player;
        PlayerControl playerControl = playerGo.GetComponent<PlayerControl>();
        playerControl.Player = player;
    } 

    void Update()
    {
        Client.NetWork.Update();
    }


    public void GameBegin(GameObject go)
    {
      //  go.SetActive(false);
       // Game.Instance.DoGameFapai();
 
        Client.NetWork.SendNetEmptyMessage(NetOpcodes_C2SEnmu.C2S_CREATERE);
    }

    void DizhuPoker()
    {
        GameObject prefab = ResLoader.Load("Prefab/DiZhuPai") as GameObject;
        GameObject dizhuPaiGo = NGUITools.AddChildNotLoseAnyThing(gameObject, prefab);
        dizhuPai = dizhuPaiGo.GetComponent<DizhuPai>();
        Game.Instance.WhoQiangDizhu();

    }

    void OnDestroy()
    {
         Game.Instance.GameBeginDizhuPoker -= this.DizhuPoker;
         Client.Instance.Create -= new Client.OnCreate(CreatePlayer);
    }
   
    public void SetAlreadyChuPokers( List<PuKe> list)
    {
        if (alreadyChuPokersGo != null)
        {
            Destroy(alreadyChuPokersGo);
            alreadyChuPokersGo = null;
        }
        GameObject prefab = ResLoader.Load("Prefab/GameObject") as GameObject;
        prefab.name = "AlreadyChuPokers";
        alreadyChuPokersGo = NGUITools.AddChildNotLoseAnyThing(gameObject, prefab);
        alreadyChuPokersGo.transform.localPosition = new Vector3(0, -33, 0);
        int count = list.Count;
        float lenght = count * Game.PuKeSpacing + 105;
        Vector3 leftPosition = new Vector3(0 - lenght / 2, 0, 0);
        for (int i = 0; i < count; i++)
        {
            //Vector3 position = list[i].transform.localPosition;
            list[i].transform.parent = alreadyChuPokersGo.transform;
            list[i].transform.localPosition = new Vector3(leftPosition.x + 105 / 2 + Game.PuKeSpacing * i, leftPosition.y, leftPosition.z);
            list[i].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
}
