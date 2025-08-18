using UnityEngine;

public class IdleState : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



/*
 public class IdleState : PlayerState
{
    public IdleState(ThirdPersonController controller, StateMachine stateMachine)
        : base(controller, stateMachine) { }

    public override void Enter()
    {
        controller.Animator.Play("Idle");
    }

    public override void HandleInput()
    {
        if (controller.MoveInput.magnitude > 0.1f)
            stateMachine.ChangeState(new WalkState(controller, stateMachine));
    }
}

 
 */