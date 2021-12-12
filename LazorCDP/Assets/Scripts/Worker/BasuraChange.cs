using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasuraChange : MonoBehaviour
{
    [SerializeField] private Sprite[] imageList = new Sprite[2];
    [SerializeField] public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = imageList[0];
    }

    public void changeBasura(int numeroBasura)
    {
        spriteRenderer.sprite = imageList[numeroBasura];
    }
}
