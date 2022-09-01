using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billboard : MonoBehaviour
{
    public Transform cam;
    private void Start() {
        cam = Camera.main.transform;
    }
    void LateUpdate() {
        transform.LookAt(transform.position + cam.forward);
    }
}