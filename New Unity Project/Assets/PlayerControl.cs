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

    IEnumerator IEnumeratorFaPai()
    {
        for (int i = 0; i < 17; i++)
        {
            Add(player.Pokers[i]);
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

    

    public void SetPlayer (Player p)
    {
        player = p;
    }
    public bool IsRead
    {
        get { return isReady; }
    }

    void FaPai()
    {
        StartCoroutine(IEnumeratorFaPai());
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

   public void QiangDiZhu()
   {

   }
}
