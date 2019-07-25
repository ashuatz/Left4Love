using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Zombi;

public class Player : MonoBehaviour, IDamage
{

    [SerializeField]
    private Rigidbody rigidbody;
    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private float MovementSpeed;

    [SerializeField]
    private Transform HandPivot;
    [SerializeField]
    private Transform HandPosition;

    [SerializeField]
    private Bullet BulletOrigin;
    
    [SerializeField]
    private float DefaultShotCoolDown;
    private float ShotCoolDown;

    //State
    private bool Attackable;
    private bool isInvincibility;
    private bool isControllable;
    
    private SubjectValue<float> HP = new SubjectValue<float>(1000);
    
    private SubjectValue<float> LoveGauge = new SubjectValue<float>(0);

    public Vector2 MoveDir { get; private set; }
    public Vector2 ViewDir { get; private set; }

    public bool IsDied => HP.value <= 0;

    public bool IsDamageEnable => !isInvincibility;
    public bool IsHealEnable => true;

    public BaseWeapon CurrentWeapon;
    private Coroutine ShotRoutine;

    private Player LastAttacker = null;

    private void Awake()
    {
        SetWeapon();
        Attackable = true;
        isInvincibility = false;
        isControllable = true;

        playerInput.OnMoveDirection += PlayerInput_OnMoveDirection;
        playerInput.OnViewDirection += PlayerInput_OnViewDirection;
        playerInput.OnClick += PlayerInput_OnClick;

        HP.onNotifyDelta += HP_onNotifyDelta;
        LoveGauge.onNotifyDelta += LoveGauge_onNotifyDelta;
    }

    private void LoveGauge_onNotifyDelta(float last, float current)
    {
        var delta = current - last;
        if(current >= 10)
        {
            //cc
            isControllable = false;

            var dir = (transform.position - LastAttacker.transform.position);
            MoveDir = new Vector2(dir.x, dir.z);
            StartCoroutine(Timer(2f, () => isControllable = true));
        }
    }

    private void HP_onNotifyDelta(float last, float current)
    {
        var delta = current - last;
        if(delta < 0)
        {
            isInvincibility = true;
            StartCoroutine(Timer(1f, () => isInvincibility = false));
        }
        if(current <= 0)
        {
            //dead
            Debug.Log("TODO : DEAD");
        }
    }

    public void SetWeapon()
    {
        CurrentWeapon.transform.SetParent(HandPosition, false);
        CurrentWeapon.transform.localPosition = Vector3.zero;
        CurrentWeapon.transform.localRotation = Quaternion.identity;
    }
    
    private IEnumerator Timer(float t, Action onComplete)
    {
        yield return new WaitForSeconds(t);
        onComplete?.Invoke();
    }

    public void Damage(int damage, GameObject attacker)
    {
        //Player Attack
        var player = attacker.GetComponent<Player>();
        if (player != null)
        {
            LoveGauge.value += damage;
            LastAttacker = player;
        }
        var zombi = attacker.GetComponent<ZombiCharacter>();
        if (zombi != null)
        {
            HP.value -= damage;
        }
    }

    public void Heal(int amount)
    {
        HP.value += amount;
    }

    private void Update()
    {
        move();
    }

    private void move()
    {
        rigidbody.velocity = new Vector3(MoveDir.x, 0, MoveDir.y) * MovementSpeed;
    }

    //input
    private void PlayerInput_OnClick(bool isPressed)
    {
        if (!isControllable || !Attackable)
            return;
        
        if (isPressed && ShotRoutine == null)
        {
            ShotRoutine = StartCoroutine(CurrentWeapon.Fire(ViewDir, () => ShotRoutine = null));
        }
    }
    
    private void PlayerInput_OnViewDirection(Vector2 obj)
    {
        if(!isControllable)
            return;

        ViewDir = obj;
        var angle = Mathf.Atan2(obj.y, obj.x) * Mathf.Rad2Deg;

        HandPivot.localRotation = Quaternion.AngleAxis(-angle, Vector3.up);
        CurrentWeapon?.Rotate(ViewDir);
    }

    private void PlayerInput_OnMoveDirection(Vector2 obj)
    {
        if (!isControllable)
            return;

        MoveDir = obj;
    }
    
}
