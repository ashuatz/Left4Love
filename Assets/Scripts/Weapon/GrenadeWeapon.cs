using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zombi;

public class GrenadeWeapon : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rigid;

    [SerializeField]
    private Collider collider;

    [SerializeField]
    private ParticleSystem effect;

    [SerializeField]
    private GameObject Model;

    private GameObject Owner;

    public void Initialize(Vector3 dir, GameObject owner)
    {
        Owner = owner;
        collider.enabled = false;

        rigid.isKinematic = false;

        dir.Normalize();
        rigid.AddForce(new Vector3(dir.x, 0.1f, dir.y) * 500f);
        Model.SetActive(true);
        StartCoroutine(TickAndBoom());
    }


    private IEnumerator TickAndBoom()
    {
        yield return new WaitForSeconds(2f);

        collider.enabled = true;

        effect.transform.rotation = Quaternion.identity;

        effect.Play();

        yield return null;

        Model.SetActive(false);
        rigid.isKinematic = true;

        collider.enabled = false;

        yield return new WaitForSeconds(1f);

        PoolManager.ReleaseObject(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.Damage(100, Owner);
            }

            ZombiCharacter zombi = other.attachedRigidbody.GetComponent<ZombiCharacter>();
            if (zombi != null)
            {
                zombi.Damage(100, Owner);
            }
        }
    }
}
