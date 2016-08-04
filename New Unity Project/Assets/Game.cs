using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Game
{
    private static Game instance;
    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Game();

            }
            return instance;
        }
    }
    public static float PuKeSpacing = 20;
    public  Player Player1;
    public  Player Player2;
    public  Player Player3;
    public  List<poker> pokers = new List<poker>();
    public  bool AllReady()
    {
        if( true)
        {
            XiPai();
            return true;
        }
        return false;
    }

    public  void Init()
    {
        for(int i=1;i<=13;i++)
        {
            for (int j=0;j<4;j++)
            {
                poker p = new poker((STYLE)j, i);
                pokers.Add(p);
            }
        }
        pokers.Add(new poker(STYLE.DAWANG, 0));
        pokers.Add(new poker(STYLE.XIAOWANG, 0));
        Player1 = new Player(DIRECTION.SOUTH);
        Player2 = new Player(DIRECTION.WEST);
        Player3 = new Player(DIRECTION.EAST);
       
    }
    public void XiPai()
    {
        AddPoker(Player1);
        AddPoker(Player2);
        AddPoker(Player3);
    }
    private  void AddPoker(Player p)
    {
        
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        for (int i=0;i<17;i++)
        {
            int value = UnityEngine.Random.Range(0, pokers.Count);
            p.AddPoker(pokers[value]);
            pokers.RemoveAt(value);
        }
    }
    public  delegate void OnGameFaPai();
    public event OnGameFaPai GameFapai;

    public void DoGameFapai()
    {
        GameFapai();
    }
}
public enum STYLE
{
    HEITAO,
    HONGXIN,
    MEIHUA,
    FANGKUA,
    DAWANG,
    XIAOWANG,
}
public class poker
{

    public int Value;
    public STYLE Style;
    public poker(STYLE s, int v)
    {
        Style = s;
        Value = v;
    }

}

public enum DIRECTION
{
    EAST,
    WEST,
    SOUTH,
}

public enum STATUS
{
    PERSANT1,
    PERSANT2,
    LANDLORD,
    NONE,
}
public class Player
{
    List<poker> pokers = new List<poker>();
    
    STATUS status;

    DIRECTION direction;
    public DIRECTION Direction
    {
        get { return direction; }
    }

    public List<poker>  Pokers
    {
        get { return pokers; }
    }
     public Player(DIRECTION di)
    {

    }

//     public Player(DIRECTION di, GameObject go)
//     {
//         Vector3 centrePos = Vector4.zero;
// 
//         Quaternion quaternion = new Quaternion();
//         if (di == DIRECTION.EAST)
//         {
//             centrePos = new Vector3(-390, 100, 0);
//             quaternion = new Quaternion(0, 0, 90, 0);
//         }
//         if (di == DIRECTION.WEST)
//         {
//             centrePos = new Vector3(390,100, 0);
//             quaternion = new Quaternion(0, 0, 90, 0);
//         }
//         if (di == DIRECTION.SOUTH)
//         {
//             centrePos = new Vector3(0, -330, 0);
//             quaternion = new Quaternion(0, 0, 0, 0);
//         }
//         GameObject prefab = ResLoader.Load("Prefab/Player") as GameObject;
//         GameObject player = NGUITools.AddChild(go, prefab);
//         player.transform.localRotation = quaternion;
//         player.transform.localPosition = centrePos;
//         playerControl = player.GetComponent<PlayerControl>();
//         status = STATUS.NONE;
//     }

//     public PlayerControl PlayerCtr
//     {
//         get { return playerControl; }
//     }

    public void AddPoker(poker p)
    {
        pokers.Add(p);
    }
    public void RemovePoker()
    {

    }
    public string GetPokerSpriteName(int index)
    {
        poker p = pokers[index];
        return (p.Style.ToString() + p.Value.ToString()).ToLower();
    }
}

public class PukeSingle
{
    string name;
}




