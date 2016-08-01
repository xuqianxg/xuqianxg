using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {

    public UILabel buttonLabel;
    public UILabel messageLabel;
    bool isReady = false;
	void Start () 
    {
        Player player1 = new Player(DIRECTION.SOUTH, gameObject);
        StartCoroutine(player1.PlayerCtr.FaPai());
	}
	

	void Update () 
    {
	
	}

    void ReadClick()
    {
        //isReady == true ? buttonLabel.text = "取消准备" : buttonLabel.text = "准备";
        buttonLabel.text = isReady ? "取消准备" : "准备";
        isReady = !isReady;
    }
}
