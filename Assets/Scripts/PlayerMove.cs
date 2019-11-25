using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    Rigidbody rb;
    Vector3 translation;
    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        //rb.velocity = new Vector3(0,0,1)* speed;
        translation = new Vector3(0, 0, 1) * speed;
    }
    private void Update()
    {
        transform.Translate(translation * Time.deltaTime, Space.World);
    }
}
