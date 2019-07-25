﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Zombi;

public class Player : MonoBehaviour, IDamage
{
    public int ID { get; private set; }
    
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
    private Animator animator;

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
    private Coroutine HitGradientRoutine;

    private Player LastAttacker = null;

    [SerializeField]
    private Gradient HitGradient;

    [SerializeField]
    private List<PlayerSpritePart> PlayerParts = new List<PlayerSpritePart>();

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
            SetSpriteColor();
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
        CurrentWeapon.Initialize(gameObject);
    }
    
    private IEnumerator Timer(float t, Action onComplete)
    {
        yield return new WaitForSeconds(t);
        onComplete?.Invoke();
    }

    //Interaction
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
        SetAnimation();
    }

    //animator
    private void SetSpritePart(Vector2 lastView)
    {
        //X flip
        if (ViewDir.x < 0 && lastView.x >= 0)
        {
            CurrentWeapon.ReverseSprite(true);
            foreach(var part in PlayerParts)
            {
                part.SetFlip(true);
            }
        }
        else if (ViewDir.x >= 0 && lastView.x < 0)
        {
            CurrentWeapon.ReverseSprite(false);
            foreach (var part in PlayerParts)
            {
                part.SetFlip(false);
            }
        }

        if(ViewDir.y < 0 && lastView.y >= 0)
        {
            foreach (var part in PlayerParts)
            {
                part.Setpart(PlayerSpritePart.PartType.Forward);
            }
        }
        else if(ViewDir.y >= 0 && lastView.y < 0)
        {
            foreach (var part in PlayerParts)
            {
                part.Setpart(PlayerSpritePart.PartType.Back);
            }
        }
    }

    private void SetAnimation()
    {
        if (MoveDir.magnitude > 0)
        {
            animator.Play("PlayerMovement");
        }
        else
        {
            animator.Play("PlayerIdle");
        }
    }

    private void SetSpriteColor()
    {
        if (HitGradientRoutine == null)
            HitGradientRoutine = StartCoroutine(SetSpriteColorInternal(1f));
    }

    private IEnumerator SetSpriteColorInternal(float time)
    {
        float t = 0;
        while (t < time)
        {
            foreach (var part in PlayerParts)
            {
                part.setColor(HitGradient.Evaluate(t / time));
            }
            t += Time.deltaTime;
            yield return null;
        }

        foreach (var part in PlayerParts)
        {
            part.setColor(HitGradient.Evaluate(1));
        }
        HitGradientRoutine = null;
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

        var lastView = ViewDir;

        ViewDir = obj;
        var angle = Mathf.Atan2(obj.y, obj.x) * Mathf.Rad2Deg;

        HandPivot.localRotation = Quaternion.AngleAxis(-angle, Vector3.up);

        SetSpritePart(lastView);
        CurrentWeapon?.Rotate(ViewDir);
    }

    private void PlayerInput_OnMoveDirection(Vector2 obj)
    {
        if (!isControllable)
            return;

        MoveDir = obj;
    }
    
}
