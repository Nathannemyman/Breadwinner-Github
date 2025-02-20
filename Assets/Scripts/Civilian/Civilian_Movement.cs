using System.Collections;
using UnityEngine;

public class Civilian_Movement : MonoBehaviour
{
    private float speed;
    private bool isMoving = true;
    private const float check_movement_interval = 3f;

    void Start()
    {
        speed = Random.Range(1f, 4f);
        StartCoroutine(Movement_Stops_Routine());
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
    }
    IEnumerator Movement_Stops_Routine()
    {
        while (true)
        {
            // 50% chance to stop for 3 seconds, checked every 3 seconds
            yield return new WaitForSeconds(check_movement_interval);
            if (Random.Range(0f, 1f) < 0.5f)
            {
                isMoving = false;
                yield return new WaitForSeconds(3f);
                isMoving = true;
            }
        }
    }
}
