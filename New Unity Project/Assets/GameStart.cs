using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameStart : MonoBehaviour {


    private DizhuPai dizhuPai;
    void Awake()
    {
        Game.Instance.Init();
        Debug.Log(transform.localEulerAngles);
    }

    void Start()
    {
        Game.Instance.GameSetDizhuiPoker +=  this.DizhuPoker;
        GameObject prefab = ResLoader.Load("Prefab/Player") as GameObject;
        GameObject player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(-390, 100, 0);
        player.transform.localEulerAngles = new Vector3(0, 0,-90);
        player.GetComponent<PlayerControl>().SetPlayer(Game.Instance.Player1);
        player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(390, 100, 0);
        player.transform.localEulerAngles = new Vector3(0, 0, 90);
        player.GetComponent<PlayerControl>().SetPlayer(Game.Instance.Player2);
        player = NGUITools.AddChild(gameObject, prefab);
        player.transform.localPosition = new Vector3(0, -330, 0);
        player.GetComponent<PlayerControl>().SetPlayer(Game.Instance.Player3);
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
    }

    void SetDizhuPoker(List<poker> list)
    {
      
    }
    void OnDestroy()
    {
        Game.Instance.GameSetDizhuiPoker -= this.DizhuPoker;
    }
}
