using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game
{
    public static float PuKeSpacing = 20;
    public static Player Pesant1;
    public static Player Pesant2;
    public static Player Landlord;
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
}
public class Player
{
    List<PuKe> list = new List<PuKe>();
    STATUS status;
    PlayerControl playerControl;
    public Player(DIRECTION di, STATUS s ,GameObject go)
    {
        Vector3 centrePos = Vector4.zero;
        status = s;
        Quaternion quaternion = new Quaternion();
        if (di == DIRECTION.EAST)
        {
            centrePos = new Vector3(-390, 100, 0);
            quaternion = new Quaternion(0, 0, 90, 0);
        }
        if (di == DIRECTION.WEST)
        {
            centrePos = new Vector3(390,100, 0);
            quaternion = new Quaternion(0, 0, 90, 0);
        }
        if (di == DIRECTION.SOUTH)
        {
            centrePos = new Vector3(0, -330, 0);
            quaternion = new Quaternion(0, 0, 0, 0);
        }
        GameObject prefab = ResLoader.Load("Prefab/Player") as GameObject;
        GameObject player = NGUITools.AddChild(go, prefab);
        player.transform.localRotation = quaternion;
        player.transform.localPosition = centrePos;
        playerControl = player.GetComponent<PlayerControl>();

    }
    void InstancePlayer(GameObject go)
    {
        GameObject prefab = ResLoader.Load("Player") as GameObject;
        GameObject player = NGUITools.AddChild(go, prefab);
        playerControl = player.GetComponent<PlayerControl>();
    }
    public PlayerControl PlayerCtr
    {
        get { return playerControl; }
    }

}

public class PukeSingle
{
    string name;
}




