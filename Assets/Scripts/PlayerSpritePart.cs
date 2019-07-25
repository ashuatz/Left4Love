using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpritePart : MonoBehaviour
{
    public enum PartType
    {
        Forward = 0,
        Back = 1
    }

    [SerializeField]
    private SpriteRenderer renderer;

    [SerializeField]
    private List<Sprite> sprites;

    private bool flipX;

    private void Awake()
    {
        flipX = renderer.flipX;
    }

    public void Setpart(PartType type)
    {
        renderer.sprite = sprites[(int)type];
    }

    public void SetFlip(bool isInverse)
    {
        if (isInverse)
            renderer.flipX = !flipX;
        else
            renderer.flipX = flipX;
    }

    public void setColor(Color color)
    {
        renderer.color = color;
    }
}
