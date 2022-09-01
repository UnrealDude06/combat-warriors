using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectileScr : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("OOF",1f);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * speed *Time.deltaTime, ForceMode.Impulse);

    }

    void OOF()
    {
        Destroy(this.gameObject);
    }
}
