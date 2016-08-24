using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameStart : MonoBehaviour {

    private DizhuPai dizhuPai;
    private GameObject alreadyChuPokersGo;
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
        PlayerControl playerControl1 = player.GetComponent<PlayerControl>();
        playerControl1.Player = (Game.Instance.Player1);

        player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(0, -330, 0);
        PlayerControl playerControl3 = player.GetComponent<PlayerControl>();
        playerControl3.Player = (Game.Instance.Player3);

        player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(390, 100, 0);
        player.transform.localEulerAngles = new Vector3(0, 0, 90);
        PlayerControl playerControl2 = player.GetComponent<PlayerControl>();
        playerControl2.Player = (Game.Instance.Player2);
        Game.Instance.XiPai();
    }
    

    public void GameBegin(GameObject go)
    {
        go.SetActive(false);
        Game.Instance.DoGameFapai();

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
