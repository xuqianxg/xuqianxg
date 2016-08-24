using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DizhuPai : MonoBehaviour
{

    public UISprite[] sprites;

    void Start()
    {
        Game.Instance.GameDiZhuOver += this.SetSprites;
    }
    private string GetPokerSpriteName(poker p)
    {
        return (p.Style.ToString() + p.Value.ToString()).ToLower();
    }
     void SetSprites()
    {
        List<poker> list = Game.Instance.GetDizhuPoker();
        for (int i = 0; i < list.Count; i++)
        {
            sprites[i].spriteName = GetPokerSpriteName(list[i]);
            sprites[i].MarkAsChanged();
        }
    }

     void Destroy()
     {
         Game.Instance.GameDiZhuOver -= this.SetSprites;
     }
}
