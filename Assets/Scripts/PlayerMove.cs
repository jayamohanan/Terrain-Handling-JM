using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0,0,1)* speed;
    }


    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    rb += new Vector3(0,0,1) * speed;
        //}
        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    transform.position += new Vector3(0, 0, -1) * speed;
        //}
        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    transform.position += new Vector3(-1, 0, 0) * speed;
        //}
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    transform.position += new Vector3(1, 0, 0) * speed;
        //}
    }
}
