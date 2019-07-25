using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rigid;
    [SerializeField]
    private float speed;
    
    public void Initialize(Vector3 pos, Vector3 dir)
    {
        transform.position = pos;
        rigid.velocity = dir.normalized * speed;
    }

    public void OnTriggerEnter(Collider other)
    {
        
    }
}
