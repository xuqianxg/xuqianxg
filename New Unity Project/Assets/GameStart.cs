using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {



    void Awake()
    {
        Game.Instance.Init();
        Debug.Log(transform.localEulerAngles);
    }

    void Start()
    {

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
        for (int i = 0; i < 17; i++)
        {
            Debug.Log("player1 "+ Game.Instance.Player1.GetPokerSpriteName(i));
            Debug.Log("player2 " + Game.Instance.Player2.GetPokerSpriteName(i));
            Debug.Log("player3 " + Game.Instance.Player3.GetPokerSpriteName(i));
        }

    }
    

    public void GameBegin(GameObject go)
    {
        go.SetActive(false);
        Game.Instance.DoGameFapai();
    }


	void Update () 
    {
	        
	}
}
