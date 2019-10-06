using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class delete : MonoBehaviour
{
    Paint p;
    public static GameObject obj;
    public static GameObject obj1;
    // Start is called before the first frame update
    void Start()
    {
        obj = new GameObject("Terrain");
        ThreadStart threadStart = delegate
        {
            p = new Paint(2);
        };
        new Thread(threadStart).Start();
    }
}

public class Paint
{
    int a;
    public Paint(int a)
    {
        this.a = a;
        Debug.Log("Jaya");
        delete.obj.transform.position = Vector3.zero;
        //Drum b = new Drum(5);
        //GameObject obj = new GameObject("Jaay");
        //obj.transform.position = Vector3.zero;
    }
}
public class Drum
{
    int b;

    public Drum(int b)
    {
        this.b = b;
        Debug.Log("Success");
    }
}
