using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game 
{
    

}

public enum DIRECTION
{
    EAST,
    WEST,
    SOUTH,
}
public class Player
{
    List<PuKe> list = new List<PuKe>();
    Vector3 centrePos;
    PlayerControl playerControl;
    DIRECTION direction;
    public Player(DIRECTION di)
    {
        if(di==DIRECTION.EAST)
        {
            centrePos = new Vector3(-250, 0, 0);
        }
        if(di==DIRECTION.WEST)
        {
            centrePos = new Vector3(250, 0, 0);
        }
        if(di==DIRECTION.SOUTH)
        {
            centrePos = new Vector3(0, -250, 0);
        }
        direction = di;
    }
    public void Remove(string name)
    {

    }
    public void Add(string name)
    {
        GameObject prefab = ResLoader.Load("Puke") as GameObject;
        GameObject puke =  NGUITools.AddChild(playerControl.gameObject, prefab);
        UISprite sprite = puke.GetComponent<UISprite>();
        sprite.spriteName = name;
        sprite.MarkAsChanged();
        if(direction==DIRECTION.SOUTH)
        {
            puke.transform.localRotation=new Quaternion(0,0,90,0);
        }
        if(list.Count==0)
        {
            puke.transform.localPosition = centrePos;

        }
        puke.SetActive(true);
        Vector3 curPosition = list[list.Count - 1].transform.localPosition;


    }
}

public class PukeSingle
{
    string name;
}


