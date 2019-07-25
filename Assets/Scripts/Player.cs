using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
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
        
    public Vector2 MoveDir { get; private set; }
    public Vector2 ViewDir { get; private set; }

    public BaseWeapon CurrentWeapon;
    private Coroutine ShotRoutine;

    private void Awake()
    {
        SetWeapon();

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

    private void PlayerInput_OnClick(bool isPressed)
    {
        if (isPressed && ShotRoutine == null)
        {
            ShotRoutine = StartCoroutine(CurrentWeapon.Fire(HandPosition.position, ViewDir, () => ShotRoutine = null));
        }
    }


    private void PlayerInput_OnViewDirection(Vector2 obj)
    {
        ViewDir = obj;
        var angle = Mathf.Atan2(obj.y, obj.x) * Mathf.Rad2Deg;

        HandPivot.localRotation = Quaternion.AngleAxis(-angle, Vector3.up);
    }

    private void PlayerInput_OnMoveDirection(Vector2 obj)
    {
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
