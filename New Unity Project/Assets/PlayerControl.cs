using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour 
{
    List<PuKe> list = new List<PuKe>();
    public void Remove(string name)
    {

    }
    void Add(string name)
    {
        GameObject prefab = ResLoader.Load("Prefab/Puke") as GameObject;
        GameObject puke = NGUITools.AddChild(gameObject, prefab);
        UISprite sprite = puke.GetComponent<UISprite>();
        sprite.spriteName = name;
        sprite.MarkAsChanged();
        list.Add(puke.GetComponent<PuKe>());
        AdjusetPosition();
        puke.SetActive(true);
       
    }

    public IEnumerator FaPai()
    {
        for (int i = 0; i < 17; i++)
        {
            Add("heitao1");
            yield return new WaitForSeconds(0.25f);
        }
    }
    void AdjusetPosition()
    {
        int count = list.Count;
        float lenght = count * Game.PuKeSpacing + 105;
        Vector3 leftPosition = new Vector3(0 - lenght / 2, 0, 0);
        for (int i = 0; i < count; i++)
        {
            //Vector3 position = list[i].transform.localPosition;
            list[i].transform.localPosition = new Vector3(leftPosition.x + 105 / 2 + Game.PuKeSpacing * i, leftPosition.y, leftPosition.z);
        }
    }


}
