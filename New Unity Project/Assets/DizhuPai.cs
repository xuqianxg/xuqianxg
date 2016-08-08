using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DizhuPai : MonoBehaviour
{

    public UISprite[] sprites;
    private string GetPokerSpriteName(poker p)
    {
        return (p.Style.ToString() + p.Value.ToString()).ToLower();
    }
    public void SetSprites(List<poker> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            sprites[i].spriteName = GetPokerSpriteName(list[i]);
            sprites[i].MarkAsChanged();
        }
    }
}
