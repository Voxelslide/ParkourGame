using StarterAssets;
using UnityEngine;

public abstract class PlayerState
{
	protected ThirdPersonController controller;
	protected StateMachine stateMachine;

	protected PlayerState(ThirdPersonController controller, StateMachine stateMachine)
	{
		this.controller = controller;
		this.stateMachine = stateMachine;
	}

	public virtual void Enter() { }            // Run once when entering state
	public virtual void Exit() { }             // Run once when exiting state
	public virtual void HandleInput() { }      // Process StarterAssetsInputs here
	public virtual void LogicUpdate() { }      // Update timers, animations, etc.
	public virtual void PhysicsUpdate() { }    // Update CharacterController.Move()
}
