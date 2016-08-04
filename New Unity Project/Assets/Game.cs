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
    MEIHUA,
    FANGKUA,
    HONGXIN,
    HEITAO,
    DAWANG,
    XIAOWANG,
}
public class poker :IComparable
{

    public int Value;
    public STYLE Style;
    public poker(STYLE s, int v)
    {
        Style = s;
        Value = v;
    }
    public static bool operator ==(poker p1,poker p2)
    {
        return p1.Value == p2.Value;

    }

    public static bool operator !=(poker p1, poker p2)
    {
        return p1.Value != p2.Value;
    }
    public static bool operator >(poker p1, poker p2)
    {
        if(p1.Value == p2.Value)
        {
            return p1.Style > p2.Style;
        }
        if (p1.Value == 0) return true;
        if (p2.Value == 0) return false;
        if (p1.Value < 3 && p2.Value > 2)
        {
            return true;
        }
        if (p1.Value > 2 && p2.Value < 3)
        {
            return false;
        }
        else
        {
            return p1.Value > p2.Value;
        }
        //return p1.Value > p2.Value;
    }
    public static bool operator <(poker p1, poker p2)
    {
        if (p1.Value == p2.Value)
        {
            return p1.Style < p2.Style;
        }
        return p2>p1;
    }
    public int CompareTo(object o)
    {
        poker p = o as poker;
        if (this > p) return 1;
        if (this < p) return -1;
        return 0;
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

public enum POKERCOMBINATION
{
    SHUNZI,
    SANDAIYI,
    SANDAIER,
    SAN,
    DUIZI,
    SINGLE,
    FEIJI,
    ZHADAN,

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
        direction = di;
    }
    public void AddPoker(poker p)
    {
        pokers.Add(p);
        pokers.Sort();
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




