using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

    private GameObject PuKeBG;
    // Use this for initialization
	void Start () 
    {
        GameObject prefab = Resources.Load("PuKeBG") as GameObject;
        GameObject pukeBg = NGUITools.AddChild(gameObject, prefab);
        PuKeBG = NGUITools.AddChild(gameObject, prefab);
	}
	
	void PuKeMove(Vector3 tweenPostion)
    {
        Vector3 postion = PuKeBG.transform.localPosition;
        TweenPosition tween = TweenPosition.Begin(PuKeBG, 0.5f, tweenPostion);
        tween.AddOnFinished(() =>
            {
                PuKeBG.SetActive(false);
                PuKeBG.transform.localPosition = postion;
                PuKeBG.SetActive(true);

            });
    }
	void Update () 
    {
	
	}
}
