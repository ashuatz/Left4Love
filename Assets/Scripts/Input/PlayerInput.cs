using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zombi;

public class PlayerInput : BaseInput
{

    public Player player;
    public GameObject particleRoot;
    public ParticleSystem particle;

    public override event Action<Vector2> OnMoveDirection;
    public override event Action<Vector2> OnViewDirection;

    public override event Action<bool> OnClick;
    public override event Action OnSpectialClick;

    void Update()
    {
        Vector2 moveDir = Vector2.zero;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKeyDown(KeyCode.A))
        {
            moveDir += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += Vector2.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += Vector2.down;
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveDir += Vector2.up;
        }
        if(Input.GetKey(KeyCode.Space))
        {
            ZombiManager zombiManager = ZombiManager.Instance;
            List<GameObject> playerList = zombiManager.GetPlayerOwnerList();

            GameObject near = null;
            float nearDist = float.MaxValue;
            for (int i = 0; i < playerList.Count; ++i)
            {
                if (playerList[i] != gameObject)
                {
                    float dist = Vector3.Distance(transform.position, playerList[i].transform.position);
                    if (dist < nearDist && dist <= 10.0f)
                    {
                        near = playerList[i];
                        nearDist = dist;
                    }
                }
            }

            if (near)
            {
                particleRoot.transform.SetParent(near.transform);
                particleRoot.transform.localPosition = Vector3.zero;
                particle.Clear();
                particle.Play();

                for (int i = 0; i < playerList.Count; ++i)
                {
                    if (playerList[i] == gameObject)
                    {
                        List<ZombiCharacter> zombiList = zombiManager.GetSpawnedZombiList(playerList[i]);
                        for(int j=0;j<zombiList.Count;++j)
                            zombiList[j].SetTargetPlayer(near);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            OnSpectialClick?.Invoke();
        }

        var dir = Input.mousePosition - Camera.main.WorldToScreenPoint(player.transform.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f, 1 << LayerMask.NameToLayer("Ground")))
        {
            var temp = (hit.point + Vector3.up * 0.5f) - player.transform.position;
            dir = new Vector2(temp.x, temp.z);
        }

        OnViewDirection?.Invoke(new Vector2(dir.x, dir.y));
        OnMoveDirection?.Invoke(moveDir);

        OnClick?.Invoke(Input.GetMouseButton(0));
    }
}
