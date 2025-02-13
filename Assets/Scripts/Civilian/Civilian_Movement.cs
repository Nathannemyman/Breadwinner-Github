using System.Threading;
using UnityEngine;

public class Civilian_Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.left * 5 * Time.deltaTime;
    }
}
