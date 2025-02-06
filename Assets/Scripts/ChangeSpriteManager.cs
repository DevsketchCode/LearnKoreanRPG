using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSpriteManager : MonoBehaviour
{
    // Not used at this time, See Player.cs and move it from there.
    public Sprite[] spriteArray;

    public void ChangeSprite(SpriteRenderer spriteRenderer, int spriteArrayValue)
    {
        spriteRenderer.sprite = spriteArray[spriteArrayValue];
    }
}
