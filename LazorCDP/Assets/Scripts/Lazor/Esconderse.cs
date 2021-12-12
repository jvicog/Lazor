using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Esconderse : MonoBehaviour
{
    private GameObject playerController;
    private bool dentro;
    private bool vibracion = true;
    private Transform pos;
    private Quaternion rot;

    private Tween tween;
    // Start is called before the first frame update
    void Start()
    {
        pos = transform.parent;
        rot = transform.parent.rotation;
        playerController = FindObjectOfType<PlayerController>().gameObject;

        //transform.parent.DOMoveY(pos.position.y + 0.2f, 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        tween = transform.parent.DOScale(pos.localScale * 1.1f, 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        tween.Play();

    }

    // Update is called once per frame
    void Update()
    {
        if (dentro && playerController.GetComponent<PlayerController>().escondido)
        {
            vibrar();
            transform.parent.position = pos.position;

            playerController.GetComponent<PlayerController>().enabled = false;
            playerController.GetComponent<CColider>().capsuleColliders[0].enabled = false;
            playerController.GetComponent<CColider>().capsuleColliders[1].enabled = false;
            playerController.GetComponent<Rigidbody>().useGravity = false;
            playerController.transform.GetChild(0).gameObject.SetActive(false);
            playerController.transform.GetChild(4).gameObject.SetActive(false);
            transform.DOScale(pos.localScale, 0.1f);
            tween.Pause();
        }
        else if (dentro)
        {
            playerController.GetComponent<PlayerController>().enabled = true;
            playerController.GetComponent<CColider>().capsuleColliders[0].enabled = true;
            playerController.GetComponent<CColider>().capsuleColliders[1].enabled = true;
            playerController.GetComponent<Rigidbody>().useGravity = true;
            playerController.transform.GetChild(0).gameObject.SetActive(true);

            playerController.transform.GetChild(4).gameObject.SetActive(true);
            tween.Play();
        }
        else
        {
            tween.Play();
        }
    }


    private void vibrar()
    {
        if (!vibracion) return;
        transform.parent.rotation = rot;
        vibracion = false;
        StartCoroutine(vibrarAlRato());
    }

    IEnumerator vibrarAlRato()
    {
        yield return new WaitForSeconds(3);
        vibracion = true;
        if (dentro && playerController.GetComponent<PlayerController>().escondido)
        {
            transform.parent.DORotate(new Vector3(pos.rotation.eulerAngles.x + 2, pos.rotation.eulerAngles.y, pos.rotation.eulerAngles.z + 2), 0.1f);
            yield return new WaitForSeconds(0.1f);
            transform.parent.DORotate(new Vector3(-pos.rotation.eulerAngles.x + 2, pos.rotation.eulerAngles.y, -pos.rotation.eulerAngles.z + 2), 0.1f);
            yield return new WaitForSeconds(0.1f);
            transform.parent.DORotate(new Vector3(pos.rotation.eulerAngles.x + 2, pos.rotation.eulerAngles.y, pos.rotation.eulerAngles.z + 2), 0.1f);
            yield return new WaitForSeconds(0.1f);
            transform.parent.DORotate(new Vector3(-pos.rotation.eulerAngles.x + 2, pos.rotation.eulerAngles.y, -pos.rotation.eulerAngles.z + 2), 0.1f);
            yield return new WaitForSeconds(0.1f);
            transform.parent.rotation = rot;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            dentro = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            dentro = false;
        }
    }
}