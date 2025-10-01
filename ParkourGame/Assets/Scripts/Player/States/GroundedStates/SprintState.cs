using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

public class SprintState : PlayerState
{

  private float speed;
  private float animationBlend;
  private float targetRotation;
  private float rotationVelocity;



  //Constructor
  public SprintState(ThirdPersonController controller, StateMachine stateMachine)
 : base(controller, stateMachine) { }


  //Enter
  public override void Enter()
  {
    Debug.Log("Entered Sprint State");
  }


  //HandleInput
  //Checks for player inputs that could trigger exiting this state
  public override void HandleInput()
  {
    var input = controller.GetInput();

    if (input.move.magnitude < 0.1f)
    {
      stateMachine.ChangeState(new IdleState(controller, stateMachine));
      return;
    }

    if (input.sprint == false)
    {
      stateMachine.ChangeState(new WalkState(controller, stateMachine));
      return;
    }


  }



  //LogicUpdate
  //This is where the sprint logic goes
  public override void LogicUpdate()
  {
    var input = controller.GetInput();
    var animator = controller.GetAnimator();
    var characterController = controller.GetComponent<CharacterController>();
    var mainCamera = Camera.main;



    // STEP 1: Decide movement speed----------------------------------------
    float targetSpeed = controller.SprintSpeed;
    // If no movement input, stop movement
    if (input.move == Vector2.zero)
      targetSpeed = 0.0f;

    // Current horizontal speed (ignoring vertical velocity)
    float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;

    float speedOffset = 0.1f; // deadzone to prevent jitter
    float inputMagnitude = input.analogMovement ? input.move.magnitude : 1f;

    // Smooth acceleration and deceleration
    if (currentHorizontalSpeed < targetSpeed - speedOffset ||
        currentHorizontalSpeed > targetSpeed + speedOffset)
    {
      // Gradually adjust speed to target
      speed = Mathf.Lerp(
          currentHorizontalSpeed,
          targetSpeed * inputMagnitude,
          Time.deltaTime * controller.SpeedChangeRate
      );

      // Round speed for precision
      speed = Mathf.Round(speed * 1000f) / 1000f;
    }
    else
    {
      // Snap directly if within deadzone
      speed = targetSpeed;
    }

    // Smooth animation blending
    animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * controller.SpeedChangeRate);
    if (animationBlend < 0.01f)
      animationBlend = 0f;

    // STEP 2: Determine movement direction (relative to camera)------
    Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

    if (input.move != Vector2.zero)
    {
      // Convert input direction into world rotation relative to camera
      targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                      + mainCamera.transform.eulerAngles.y;

      // Smooth rotation
      float rotation = Mathf.SmoothDampAngle(
          controller.transform.eulerAngles.y,
          targetRotation,
          ref rotationVelocity,
          controller.RotationSmoothTime
      );

      // Apply rotation to character
      controller.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    }

    // Convert target rotation into movement vector
    Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

    // STEP 3: Apply movement-----------------------------------------
    characterController.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                             new Vector3(0.0f, controller.GetVerticalVelocity(), 0.0f) * Time.deltaTime);

    // STEP 4: Animator updates
    if (animator != null)
    {
      if (animator.speed != controller.SprintSpeed)
      {
        float currentBlend = animator.GetFloat("Speed");
        float smoothedBlend = Mathf.Lerp(currentBlend, controller.SprintSpeed, Time.deltaTime * controller.SpeedChangeRate);
        animator.SetFloat("Speed", smoothedBlend);
      }
      else
      {
        animator.SetFloat("Speed", animationBlend);
      }
      animator.SetFloat("MotionSpeed", inputMagnitude);
    }

  }



  //PhysicsUpdate
  public override void PhysicsUpdate()
  {
  }



  //Exit
  public override void Exit()
  {
    //end sprint anim?
    Debug.Log("Exited Sprint State");
  }





}
