using StarterAssets;
using UnityEngine;
using UnityEngine.UI;



public class JumpState : PlayerState
{
  private float _animationBlend;
  private bool _hasJumped = false;

  public JumpState(ThirdPersonController controller, StateMachine stateMachine)
    : base(controller, stateMachine) { }

  public override void Enter()
  {
    var animator = controller.GetAnimator();
    if (animator != null)
    {
      animator.SetBool("Jump", true);
      animator.SetBool("Grounded", false);
      animator.SetBool("FreeFall", false);
    }

    // Prevent residual downward velocity
    float verticalVelocity = controller.GetVerticalVelocity();
    if (verticalVelocity < 0f)
      controller.SetVerticalVelocity(-2f);

    // Apply jump impulse
    float jumpVelocity = Mathf.Sqrt(controller.JumpHeight * -2f * controller.Gravity);
    controller.SetVerticalVelocity(jumpVelocity);
    _hasJumped = true;

    // Consume jump buffer & coyote timers
    controller._jumpBufferCounter = 0f;
    controller._coyoteTimeCounter = 0f;
  }

  public override void HandleInput()
  {
    controller.GetInput().jump = false;
  }

  public override void LogicUpdate()
  {
    ApplyGravity();
    UpdateMovement();
    UpdateAnimation();

    // Transition to fall when moving downward
    if (controller.GetVerticalVelocity() < 0f)
    {
      stateMachine.ChangeState(new FallState(controller, stateMachine));
      return;
    }

    // Ground safety check
    if (controller.IsGrounded())
    {
      stateMachine.ChangeState(new GroundedState(controller, stateMachine));
      return;
    }
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

  private void UpdateMovement()
  {
    var input = controller.GetInput();
    var camera = Camera.main;
    var cc = controller.GetCharacterController();

    // Partial midair control
    Vector3 moveDir = Vector3.zero;
    if (input.move.sqrMagnitude > 0.01f)
    {
      float targetRot = Mathf.Atan2(input.move.x, input.move.y) * Mathf.Rad2Deg + camera.transform.eulerAngles.y;
      moveDir = Quaternion.Euler(0f, targetRot, 0f) * Vector3.forward;
    }

    float airControl = 0.5f;
    Vector3 motion = moveDir * controller.MoveSpeed * airControl * Time.deltaTime;
    motion.y = controller.GetVerticalVelocity() * Time.deltaTime;
    cc.Move(motion);
  }

  private void UpdateAnimation()
  {
    var animator = controller.GetAnimator();
    if (animator == null) return;

    _animationBlend = Mathf.Lerp(_animationBlend, 1f, Time.deltaTime * 4f);
    animator.SetFloat("Speed", _animationBlend);
    animator.SetFloat("MotionSpeed", 1f);
  }

  public override void Exit()
  {
    var animator = controller.GetAnimator();
    if (animator != null)
      animator.SetBool("Jump", false);
  }
}
