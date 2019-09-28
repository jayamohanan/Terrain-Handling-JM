using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DisableOnPlay : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
