using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour 
{
    List<PuKe> list = new List<PuKe>();
    List<poker> clickPoker = new List<poker>();
    private Player player;
    public UILabel buttonLabel;
    public UILabel messageLabel;
    bool isReady = false;
    int time = 30;
    bool beginTime = false;
    float timeStep = 0;
    public int Beishu = 0;

    public UIButton Qiang;
    public UIButton BuQiang;

    void Awake()
    {
        Game.Instance.GameFapai += new Game.OnGameFaPai(FaPai);
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
        sprite.spriteName = name;
        sprite.MarkAsChanged();
        PuKe pk = puke.GetComponent<PuKe>();
        pk.Poker = p;
        pk.SetBoxClider(0-list.Count);
        list.Add(pk);
        AdjusetPosition();
        puke.SetActive(true);
    }

    IEnumerator IEnumeratorFaPai(List<poker> pokers)
    {
        for (int i = 0; i < pokers.Count; i++)
        {
            Add(pokers[i]);
            yield return new WaitForSeconds(0.25f);
        }
        Game.Instance.DoGameSetDizhuPoker();
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
        StartCoroutine(IEnumeratorFaPai(player.Pokers));
       //IEnumeratorFaPai();
    }

    void Destroy()
    {
        Game.Instance.GameFapai -= new Game.OnGameFaPai(FaPai);
    }

   public void ClickPoker(PuKe p)
    {
        Debug.Log(p.ToString());
        if (p.IsClick)
        {
            if (!clickPoker.Contains(p.Poker))
            {
                clickPoker.Add(p.Poker);
            }
        }
        else
        {
            if (clickPoker.Contains(p.Poker))
            {
                clickPoker.Remove(p.Poker);
            }
        }
   }

   public void ChuPai()
   {
       if (clickPoker.Count == 0) return;
       Game.Instance.Chupai(clickPoker);
   }
   private string GetPokerSpriteName(poker p)
   {
       return (p.Style.ToString() + p.Value.ToString()).ToLower();
   }

   public void GiveUpDiZhu()
    {

    }

    public  void QiangDiZhu()
    {
        Beishu = Game.Instance.CurBeishu+1;
        Game.Instance.CurBeishu = Beishu;
        transform.parent.SendMessage("QiangDizhu", this, SendMessageOptions.DontRequireReceiver);
        Qiang.gameObject.SetActive(false);
        BuQiang.gameObject.SetActive(false);
        beginTime = false;
        messageLabel.gameObject.SetActive(false);
    }

    public void AddDizhuPoker()
    {
        List<poker> pokers = Game.Instance.GetDizhuPoker();
        Game.Instance.DoGameDiZhuOver();
        StartCoroutine(IEnumeratorFaPai(pokers));
        player.Status = STATUS.LANDLORD;
        ReadyChuPoker();
    }

    public void ReadyChuPoker()
    {

    }

    public void BegianQiangDiZhu()
    {
        Qiang.gameObject.SetActive(true);
        BuQiang.gameObject.SetActive(true);
        beginTime = true;
    }


    void Update()
    {
        if(beginTime)
        {
            timeStep += Time.deltaTime;
            Debug.Log(timeStep);
            if(timeStep>1)
            {
                time -=1;
                messageLabel.text = time.ToString();
                timeStep = 0;
                if(time == 0)
                {
                    GiveUpDiZhu();
                    beginTime = false;
                }
                
            }
        }
    }

//    public void QiangDiZhu(bool )
//    {
//        Game.Instance.ComparePlayerBeishu()
//    }
}
