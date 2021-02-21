using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputController : MonoSingleton<InputController>
{
    #region Events

    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;

    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;

    #endregion

    private PlayerControls playerControls;
    private Camera mainCamera;
    private bool send = false;
    private InputAction.CallbackContext context;
    public static void Enable() => Instance.gameObject.SetActive(true);
    public static void Disable() => Instance.gameObject.SetActive(false);

    override protected void Awake()
    {
        base.Awake();
        playerControls = new PlayerControls();
        mainCamera = Camera.main;
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        playerControls.Touch.PrimaryContact.started += StartTouchPrimary;
        playerControls.Touch.PrimaryContact.canceled += EndTouchPrimary;
    }

    private void OnDestroy()
    {
        playerControls.Touch.PrimaryContact.started -= StartTouchPrimary;
        playerControls.Touch.PrimaryContact.canceled -= EndTouchPrimary;
    }


    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        if (playerControls.Touch.PrimaryPosition.ReadValue<Vector2>() != Vector2.zero)
            SendStartPosition(context);
        else
        {
            this.context = context;
            send = true;
        }
    }

    private void SendStartPosition(InputAction.CallbackContext context)
    {
        send = false;
        OnStartTouch?.Invoke(
                    InputUtils.ScreenToWorld(mainCamera, playerControls.Touch.PrimaryPosition.ReadValue<Vector2>()),
                    (float)context.startTime);
    }

    private void LateUpdate()
    {
        if(send) SendStartPosition(context);
    }

    private void FixedUpdate()
    {
        if(send) SendStartPosition(context);
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        OnEndTouch?.Invoke(
            InputUtils.ScreenToWorld(mainCamera, playerControls.Touch.PrimaryPosition.ReadValue<Vector2>()),
            (float)context.time);
    }

    public Vector2 PrimaryPosition()=> InputUtils.ScreenToWorld(mainCamera,playerControls.Touch.PrimaryPosition.ReadValue<Vector2>());

}
