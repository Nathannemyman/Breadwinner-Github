using UnityEngine;

public class DeathCollision : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Body" &&
            (collision.GetComponent<CapsuleCollider2D>() != null || collision.GetComponent<Rigidbody2D>() != null))
        {  
            // Turn sprite black if it gets entered
            spriteRenderer.color = Color.black;
        }
    }
}