using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


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
    ERROR,

}

public class PokerData
{
    public POKERCOMBINATION PokerCombination;
    public List<int> list = new List<int>();
}
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
    private  List<poker> pokers = new List<poker>();
    private int BeiShu = 0;
    private PokerData beforPoker;
    public int CurBeishu = 0;

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
        Player1.Next = Player3;
        Player2.Next = Player1;
        Player3.Next = Player2;
       
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
        if (GameFapai != null)
        {
            GameFapai();
        }
    }

    public delegate void OnGameChuPai(List<poker> list);
    public event OnGameChuPai GameChuPai;

    public void DoGameChuPai(List<poker> list)
    {
        if (GameChuPai != null)
        {
            GameChuPai(list);
        }
    }

    public delegate void OnGameSetDizhuPoker();
    public event OnGameSetDizhuPoker GameSetDizhuiPoker;
    private bool isFirst = true;
    public void DoGameSetDizhuPoker()
    {
        if (isFirst && GameSetDizhuiPoker!=null)
        {
            GameSetDizhuiPoker();
            isFirst = false;
        }
        
    }

    public delegate void OnGameDiZhuOver();
    public event OnGameDiZhuOver GameDiZhuOver;
    public void DoGameDiZhuOver()
    {
        if (GameDiZhuOver != null)
        {
            GameDiZhuOver();
            isFirst = false;
        }
    }

    public List<poker> GetDizhuPoker()
    {
        return pokers;
    }

    public bool Chupai(List<poker> list)
    {
        PokerData pokerData = GetPokers(list);
        if(beforPoker == null)
        {
            beforPoker = pokerData;
            
        }

        //Debug.Log(pokerData.PokerCombination.ToString() + "   " + pokerData.list[0]);
    }


    public PokerData GetPokers(List<poker> list)
    {
        PokerData pokerData = new PokerData();
        list.Sort();
        if (list.Count == 1) pokerData.PokerCombination = POKERCOMBINATION.SINGLE;
        if(list.Count == 2)
        {
            if (list[0].Value == 0 && list[0].Value == list[1].Value)
            {
                pokerData.list.Add(0);
                pokerData.PokerCombination = POKERCOMBINATION.ZHADAN;
            }
            else
            {
                pokerData.list.Add(list[0].Value);
                pokerData.PokerCombination = list[0].Value == list[1].Value ? POKERCOMBINATION.DUIZI : POKERCOMBINATION.ERROR;
            }
            //return list[0].Value == list[1].Value ?  POKERCOMBINATION.DUIZI : POKERCOMBINATION.ERROR;
           
        }
        if(list.Count==3)
        {
            pokerData.list.Add(list[0].Value);
            pokerData.PokerCombination = (list[0].Value == list[1].Value && list[2].Value == list[0].Value) ? POKERCOMBINATION.SAN : POKERCOMBINATION.ERROR;
        }
        if(list.Count==4)
        {
            if(list[0].Value == list[1].Value && list[1].Value == list[2].Value && list[2].Value == list[3].Value)
            {
                pokerData.list.Add(list[0].Value);
                pokerData.PokerCombination = POKERCOMBINATION.ZHADAN;
            }
            else
            {
                int flag = list[0].Value;
                if(flag==list[1].Value)
                {
                    if(flag == list[2].Value || flag == list[3].Value)
                    {
                        pokerData.PokerCombination = POKERCOMBINATION.SANDAIYI;
                    }
                    else pokerData.PokerCombination = POKERCOMBINATION.ERROR;
                }
                else
                {
                    if(flag==list[2].Value && flag==list[3].Value)
                    {
                        pokerData.PokerCombination = POKERCOMBINATION.SANDAIYI;
                    }
                    else
                    {
                        flag = list[1].Value;
                        if(flag==list[2].Value && flag== list[3].Value)
                            pokerData.PokerCombination = POKERCOMBINATION.SANDAIYI;
                        else pokerData.PokerCombination = POKERCOMBINATION.ERROR;
                    }
                }
                pokerData.list.Add(flag);
            }
        }
        if(list.Count==5)
        {
            List<int> l = GetPokersEqual(list, 3);
            if(l.Count>0)
            {
                pokerData.list.Add(l[0]);
                l = GetPokersEqual(list, 2);
                if(l.Count>0)
                {
                    pokerData.list.Add(l[0]);
                    pokerData.PokerCombination = POKERCOMBINATION.SANDAIER;
                }
                else
                {
                    pokerData.PokerCombination = POKERCOMBINATION.ERROR;
                }
            }
            else if (IsDanShunzi(list))
            {
                pokerData.PokerCombination = POKERCOMBINATION.SHUNZI;
            }
            else
            {
                pokerData.PokerCombination = POKERCOMBINATION.ERROR;
            }
        }

        return pokerData;
    }


    bool IsDanShunzi(List<poker> pokers) //单顺子
    {
        pokers.Sort();
        int count = pokers.Count;
        if(pokers[count-1].Value==0 || pokers[count-1].Value==2) return false;
        for(int i = 1;i<count-1;i++)
        {
            if (pokers[i].Value - pokers[i - 1].Value == 1) continue;
            else return false;
        }
        if (pokers[count - 1].Value - pokers[count - 2].Value == 1) return true;
        else if (pokers[count - 1].Value == 1 && pokers[count - 2].Value == 13) return true;
        return false;
    }

    bool IsShuangShunzi(List<poker> pokers)//双顺子  445566
    {
        return true;
    }

    bool IsFeiji(List<poker> pokers)
    {
        return true;
    }

    private List<int> GetPokersEqual(List<poker> pokers,int count)
    {
        List<int> list = new List<int>();
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach(poker p  in pokers)
        {
            if(dict.ContainsKey(p.Value))
            {
                dict[p.Value] += 1;
            }
            else
            {
                dict.Add(p.Value, 1);
            }
        }
        foreach(KeyValuePair<int,int> key in dict)
        {
            if(key.Value==count)
            {
                list.Add(key.Key);
            }
        }
        return list;
    }

    public List<poker> GetShangjiaPokers()
    {
        return null;
    }

    public void  QiangDizhu(Player player)
    {
//         Player dizhu = null;
//         if(IsDiZhu(Player1,Player2,Player3))
//         {
//             dizhu = Player1;
//         }
//         else if (IsDiZhu(Player2, Player1, Player3))
//         {
//             dizhu = Player2;
//         }
//         else if (IsDiZhu(Player3, Player2, Player1))
//         {
//             dizhu = Player3;
//         }
//         if(dizhu==null)
//         {
//             return false;
//         }
//         else
//         {
//             return true;
//         }

        if(player.BeiShu == 3 || player.Next.GiveUpDiZhu)
        {
            foreach(poker p in pokers)
            {
                player.AddPoker(p);
            }
        }
        else
        {

        }
    }

    bool IsDiZhu(Player p1 ,Player p2 ,Player p3)
    {
        if (p1.BeiShu == 3) return true;
        if (p1.BeiShu > p2.BeiShu && p1.BeiShu > p3.BeiShu && p2.GiveUpDiZhu && p3.GiveUpDiZhu) return true;
        return false;
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


public class Player
{
    List<poker> pokers = new List<poker>();
    int beishu = 0;
    STATUS status;
    DIRECTION direction;
    bool giveUpDizhu = false;
    Player next;

    public Player Next
    {
        get { return next; }
        set { next = value; }
    }

    public bool GiveUpDiZhu
    {
        get { return giveUpDizhu;}
    }
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
    public int BeiShu
    {
        get { return beishu; }
        set { BeiShu = value; }
    }
    public STATUS Status
    {
        get { return status; }
        set { status = value; }
    }
    public void AddPoker(poker p)
    {
        pokers.Add(p);
        pokers.Sort();
    }
    public void RemovePoker()
    {

    }

    public void  GiveUpDizhu()
    {
        giveUpDizhu = true;
    }

}

public class PukeSingle
{
    string name;
}




