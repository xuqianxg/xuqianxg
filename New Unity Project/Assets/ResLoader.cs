using UnityEngine;
using System.Collections;

public class ResLoader  {

	// Use this for initialization
    public static Object  Load(string name)
    {
        return  Resources.Load(name);
    }
}
