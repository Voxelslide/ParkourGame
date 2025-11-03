using StarterAssets;
using UnityEngine;
using UnityEngine.Windows;

public class FallState : PlayerState
{
  private float animationBlend;
  private Vector3 currentMomentum;

  public FallState(ThirdPersonController controller, StateMachine stateMachine)
        : base(controller, stateMachine) { }




  public override void Enter()
  {
    Debug.Log("Entered Fall State");
    currentMomentum = controller.GetMomentum(); // get stored momentum

    var animator = controller.GetAnimator();
    if (animator != null)
    {
      animator.SetBool("FreeFall", true);
      animator.SetBool("Jump", false);
      animator.SetBool("Grounded", false);
    }
  }


  public override void HandleInput()
  {
  }

  public override void LogicUpdate()
  {
    
    ApplyGravity();
    UpdateAirMovement();

    // Check for ledge grab opportunity
    //if (controller.CheckForLedge())
    //{
    //Coming soon
    //stateMachine.ChangeState(new LedgeHangState(controller, stateMachine));
    //return;
    //}

    UpdateAnimation();

    if (controller.IsGrounded())
    {
      stateMachine.ChangeState(new GroundedState(controller, stateMachine));
      return;
    }
    else if (controller._coyoteTimeCounter > 0)
    {
      controller._coyoteTimeCounter -= Time.deltaTime;
    }
  }

  
  
  public override void PhysicsUpdate() 
  {

  }


  public override void Exit()
  {
    var animator = controller.GetAnimator();
    if (animator != null)
    {
      animator.SetBool("FreeFall", false);
    }
    Debug.Log("Exited Fall State");
  }



  private void ApplyGravity()
  {
    float verticalVelocity = controller.GetVerticalVelocity();

    if (verticalVelocity < controller.GetTerminalVelocity())
    {
      verticalVelocity += controller.Gravity * Time.deltaTime;
      controller.SetVerticalVelocity(verticalVelocity);
    }
  }


  private void UpdateAnimation()
  {
    var animator = controller.GetAnimator();
    if (animator != null)
    {
      animationBlend = Mathf.Lerp(animationBlend, 1f, Time.deltaTime * 2f);
      animator.SetFloat("Speed", animationBlend);
      animator.SetFloat("MotionSpeed", 1f);
    }
  }

  private void UpdateAirMovement()
  {
    var input = controller.GetInput();
    var mainCamera = Camera.main;
    var characterController = controller.GetCharacterController();

    // Read directional input (camera-relative)
    Vector3 inputDir = new Vector3(input.move.x, 0f, input.move.y).normalized;

    // Apply a small influence from player input midair
    if (inputDir.sqrMagnitude > 0.01f)
    {
      float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
      Vector3 inputMove = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

      // Blend existing momentum with new input direction
      currentMomentum = Vector3.Lerp(currentMomentum, inputMove * controller.MoveSpeed, controller.GetAirControl() * Time.deltaTime);
    }

    // Apply slight momentum decay (optional for realism)
    currentMomentum *= controller.GetMomentumDecay();

    // Move horizontally with preserved momentum
    //characterController.Move(currentMomentum * Time.deltaTime);
    var finalMove = new Vector3(currentMomentum.x * Time.deltaTime, controller.GetVerticalVelocity() * Time.deltaTime, currentMomentum.z * Time.deltaTime);

    characterController.Move(finalMove);
    

    // Apply vertical velocity from gravity
    //Vector3 verticalMove = new Vector3(0f, , 0f);
    //characterController.Move(verticalMove * Time.deltaTime);
  }

}
