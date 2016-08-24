using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerControl : MonoBehaviour 
{
    List<PuKe> list = new List<PuKe>();
    List<PuKe> clickPuke = new List<PuKe>();
    private Player player;
    public UILabel buttonLabel;
    public UILabel messageLabel;
    bool isReady = false;
    int time = 30;
    bool beginTime = false;
    float timeStep = 0;
    
    public UIButton Qiang;
    public UIButton BuQiang;

    public UIButton Chupai;
    public UIButton BuChu;
    
    void Awake()
    {
        Game.Instance.GameFapai += new Game.OnGameFaPai(FaPai);
        Game.Instance.GameGetDiZhu += this.GetDiZhu;
        Game.Instance.GameBeginPlayerQiangDiZhu += this.BegianQiangDiZhu;
        Game.Instance.GameBeginPlayerChuPai += this.BeginPlayerChuPai;
    }

    public void Remove(string name)
    {
         
    }
    void Add(poker p)
    {
        GameObject prefab = ResLoader.Load("Prefab/Puke") as GameObject;
        GameObject puke = NGUITools.AddChild(gameObject, prefab);
        UISprite sprite = puke.GetComponent<UISprite>();
        string name = GetPokerSpriteName(p);
        Debug.Log(name);
        sprite.spriteName = name;
        sprite.MarkAsChanged();
        PuKe pk = puke.GetComponent<PuKe>();
        pk.Poker = p;
        pk.SetBoxClider(0-list.Count);
        list.Add(pk);
        AdjusetPosition();
        puke.SetActive(true);
    }

    IEnumerator IEnumeratorFaPai(List<poker> pokers,Action callback)
    {
        for (int i = 0; i < pokers.Count; i++)
        {
            Add(pokers[i]);
            yield return new WaitForSeconds(0.25f);
        }
        /*Game.Instance.BeginDizhuPoker();*/
        if(callback != null)
        {
            callback();
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
    public void ReadClick()
    {
        buttonLabel.text = isReady ? "取消准备" : "准备";
        isReady = !isReady;
    }

    

    public Player Player
    {
        get { return player; }
        set { player = value; }
    }
    public bool IsRead
    {
        get { return isReady; }
    }

    void FaPai()
    {
        StartCoroutine(IEnumeratorFaPai(player.Pokers, Game.Instance.BeginDizhuPoker));
       //IEnumeratorFaPai();
    }

    void Destroy()
    {
        Game.Instance.GameFapai -= new Game.OnGameFaPai(FaPai);
    }

   public void ClickPoker(PuKe p)
    {
        Debug.Log(p.Poker.ToString() + "       "+p.gameObject.transform.localPosition);
        if (p.IsClick)
        {
            if (!clickPuke.Contains(p))
            {
                clickPuke.Add(p);
            }
        }
        else
        {
            if (clickPuke.Contains(p))
            {
                clickPuke.Remove(p);
            }
        }
   }

   public void ChuPai()
   {
       if (clickPuke.Count == 0) return;
       if (Game.Instance.Chupai(GetPukeListPoker(),player))
       {
           RemovePokers(clickPuke);
           if(list == null || list.Count == 0)
           {
               
           }
           AdjusetPosition();
           BuChuPai();
           MoveCilckPuKe();
           clickPuke.Clear();
       }
   }
   private string GetPokerSpriteName(poker p)
   {
       return (p.Style.ToString() + p.Value.ToString()).ToLower();
   }

   public void TimeDone()
    {
        if (player.Status == STATUS.NONE)
        {
            player.BeiShu = -1;
            Game.Instance.DoGameBeginPlayerQiangDiZhu(player.Next);
            //transform.parent.SendMessage("QiangDizhu", this, SendMessageOptions.DontRequireReceiver);
        }
       else
        {
            BuChuPai();
            
        }
    }

    public  void QiangDiZhu()
    {
        player.BeiShu = Game.Instance.CurBeishu+1;
        Game.Instance.CurBeishu = player.BeiShu;
        //transform.parent.SendMessage("QiangDizhu", this, SendMessageOptions.DontRequireReceiver);
        Qiang.gameObject.SetActive(false);
        BuQiang.gameObject.SetActive(false);
        beginTime = false;
        messageLabel.gameObject.SetActive(false);
        Game.Instance.QiangDiZhu(player);
    }

    public void AddDizhuPoker()
    {
        List<poker> pokers = Game.Instance.GetDizhuPoker();
        Game.Instance.DoGameDiZhuOver();
        StartCoroutine(IEnumeratorFaPai(pokers, SortPokers));
        //SortPokers();
        player.Status = STATUS.LANDLORD;
        player.Next.Status = STATUS.PERSANT1;
        player.Next.Next.Status = STATUS.PERSANT2;
        BeginPlayerChuPai(player);
    }     

    public void BegianQiangDiZhu(Player p)
    {
        if (player == p)
        {
            Qiang.gameObject.SetActive(true);
            BuQiang.gameObject.SetActive(true);
            beginTime = true;
        }
    }


    void Update()
    {
        if(beginTime)
        {
            timeStep += Time.deltaTime;
            if(timeStep>1)
            {
                time -=1;
                messageLabel.text = time.ToString();
                timeStep = 0;
                if(time == 0)
                {
                    TimeDone();
                    beginTime = false;
                }
                
            }
        }
    }

    void GetDiZhu(Player p, bool isGet)
    {
        if(p==player)
        {
            if (!isGet)
            {
                Game.Instance.DoGameBeginPlayerQiangDiZhu(p.Next);
            }
            else
            {
                AddDizhuPoker();
            }
        }
    }

     public void GiveUpDizhu()
    {
        player.BeiShu = -1;
        Qiang.gameObject.SetActive(false);
        BuQiang.gameObject.SetActive(false);
        beginTime = false;
        messageLabel.gameObject.SetActive(false);
        Game.Instance.DoGameBeginPlayerQiangDiZhu(player.Next);
    }

    void SortPokers()
    {
        List<poker> pokes = new List<poker>();
        foreach(PuKe puke in list)
        {
            pokes.Add(puke.Poker);
        }
        pokes.Sort();
        for(int i = 0;i<list.Count;i++)
        {
            if(list[i].Poker!=pokes[i])
            {
                list[i].SetName(pokes[i].ToString());
                list[i].Poker = pokes[i];
            }
        }
    }


    void BeginPlayerChuPai(Player p)
    {
        if(p==player)
        {
            time = 30;
            beginTime = true;
            Chupai.gameObject.SetActive(true);
            BuChu.gameObject.SetActive(true);
            messageLabel.gameObject.SetActive(true);
        }
    }
    public void BuChuPai()
    {
        Game.Instance.DoGameBeginPlayerChuPai(player.Next);
        beginTime = false;
        Chupai.gameObject.SetActive(false);
        BuChu.gameObject.SetActive(false);
        messageLabel.gameObject.SetActive(false);
    }

    void RemovePokers(List<PuKe> pokers)
    {
        foreach(PuKe p in pokers)
        {
            list.Remove(p);
        }
    }

    void MoveCilckPuKe()
    {
        transform.parent.SendMessage("SetAlreadyChuPokers", clickPuke, SendMessageOptions.DontRequireReceiver);
    }

    List<poker> GetPukeListPoker()
    {
        List<poker> pokers = new List<poker>();
        foreach(PuKe p in clickPuke)
        {
            pokers.Add(p.Poker);
        }
        return pokers;
    }

    void PlayerWin()
    {
        
    }
}
