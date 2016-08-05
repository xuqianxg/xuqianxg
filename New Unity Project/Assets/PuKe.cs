using UnityEngine;
using System.Collections;

public class PuKe : MonoBehaviour {

    private string name;
    private bool isClick = false;
    private poker p;

    public poker Poker
    {
        get { return p; }
        set { p = value; }
    }
    public bool IsClick
    {
        get { return isClick; }
    }
    void SetName(string name)
    {

    }

    void SetPosition(Vector3 postion)
    {
        transform.localPosition = postion;
    }

    public void SetBoxClider(int depth)
    {
        GetComponent<BoxCollider>().center = new Vector3(0, 0, depth);
    }

    public void OnClick()
    {
        Vector3 curPos = transform.localPosition;
        if (!isClick)
        {
            transform.localPosition = new Vector3(curPos.x, curPos.y + 40, curPos.z);
        }
        else
        {
            transform.localPosition = new Vector3(curPos.x, curPos.y - 40, curPos.z);
        }
        isClick = !isClick;
        transform.parent.SendMessage("ClickPoker", this, SendMessageOptions.DontRequireReceiver);
    }
}
