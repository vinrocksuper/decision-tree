using UnityEngine;

public class StateAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite closed;
    public Sprite open;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Open()
    {
        spriteRenderer.sprite = open;
    }

    public void Close()
    {
        spriteRenderer.sprite = closed;
    }
}
