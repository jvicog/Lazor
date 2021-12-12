using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Blood : MonoBehaviour
{
    private void Start() {
        StartCoroutine(Dissapear());
    }

    IEnumerator Dissapear() {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}
