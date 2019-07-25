using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    private CollisionRiser detector;

    [SerializeField]
    private float DefaultShotCoolDown;
    private float ShotCoolDown;

    //State
    private bool Attackable;
    private bool isInvincibility;
    private bool isControllable;
    private bool isMoveable;

    private float HP;
    private float LoveGauge;
    
    public Vector2 MoveDir { get; private set; }
    public Vector2 ViewDir { get; private set; }

    public bool IsDied => HP <= 0;

    public bool IsDamageEnable => !isInvincibility;
    public bool IsHealEnable => true;

    public BaseWeapon CurrentWeapon;
    private Coroutine ShotRoutine;

    private void Awake()
    {
        SetWeapon();
        Attackable = true;
        isInvincibility = false;
        isControllable = true;
        isMoveable = true;

        playerInput.OnMoveDirection += PlayerInput_OnMoveDirection;
        playerInput.OnViewDirection += PlayerInput_OnViewDirection;
        playerInput.OnClick += PlayerInput_OnClick;

        detector.OnTriggerEnterRiser += Detector_OnTriggerEnterRiser;
    }

    public void SetWeapon()
    {
        CurrentWeapon.transform.SetParent(HandPosition, false);
        CurrentWeapon.transform.localPosition = Vector3.zero;
        CurrentWeapon.transform.localRotation = Quaternion.identity;
    }

    private void Detector_OnTriggerEnterRiser(Collider obj)
    {
        var bullet = obj.GetComponent<Bullet>();
        if (bullet != null)
        {
            //reduce HP;
        }
    }

    public void Damage(int damage)
    {
        HP -= damage;
    }

    public void Heal(int amount)
    {
        HP += amount;
    }

    private void PlayerInput_OnClick(bool isPressed)
    {
        if (!isControllable || !Attackable)
            return;
        
        if (isPressed && ShotRoutine == null)
        {
            ShotRoutine = StartCoroutine(CurrentWeapon.Fire(HandPosition.position, ViewDir, () => ShotRoutine = null));
        }
    }
    
    private void PlayerInput_OnViewDirection(Vector2 obj)
    {
        if(!isControllable)
            return;

        ViewDir = obj;
        var angle = Mathf.Atan2(obj.y, obj.x) * Mathf.Rad2Deg;

        HandPivot.localRotation = Quaternion.AngleAxis(-angle, Vector3.up);
    }

    private void PlayerInput_OnMoveDirection(Vector2 obj)
    {
        if (!isMoveable || !isControllable)
        {
            MoveDir = Vector2.zero;
            return;
        }

        MoveDir = obj;
    }

    private void Update()
    {
        move();
    }

    private void move()
    {
        rigidbody.velocity = new Vector3(MoveDir.x, 0, MoveDir.y) * MovementSpeed;
    }

}
