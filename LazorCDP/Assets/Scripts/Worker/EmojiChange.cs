using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiChange : MonoBehaviour
{
    [SerializeField] private Sprite[] imageList = new Sprite[5];
    [SerializeField] public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = imageList[0];
    }

    public void changeEmoji(int numeroEmoji)
    {
        spriteRenderer.sprite = imageList[numeroEmoji];
    }
}
