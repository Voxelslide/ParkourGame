using UnityEditor.Experimental.GraphView;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

/*
 * ThirdPersonController.cs
 * 
 * A modified StarterAssets script for character movement, camera control, and player actions.
 * 
 * Key Features:
 * - Movement (walk/sprint)
 * - Jumping and falling with custom gravity
 * - Camera rotation with Cinemachine
 * - Ground check system
 * - Audio events for footsteps/landing
 * - Placeholder ledge grabbing and ledge movement
 * 
 * Notes:
 * - Uses CharacterController for collision/movement
 * - Uses Animator for animation triggers
 * - References StarterAssetsInputs for input mapping
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour
	{
		// ======================================================================================
		// PLAYER PARAMETERS
		// ======================================================================================

		[Header("Player Movement")]
		[Tooltip("Normal walking speed (m/s)")]
		public float MoveSpeed = 2.0f;

		[Tooltip("Sprinting speed (m/s)")]
		public float SprintSpeed = 5.335f;

		[Tooltip("How quickly the character rotates to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;

		[Tooltip("Acceleration/deceleration rate")]
		public float SpeedChangeRate = 10.0f;

		[Header("Player Jump & Gravity")]
		[Tooltip("Jump height (in meters)")]
		public float JumpHeight = 1.2f;

		[Tooltip("Custom gravity value (engine default is -9.81f)")]
		public float Gravity = -15.0f;

		[Tooltip("Cooldown between jumps (set 0 to allow spamming)")]
		public float JumpTimeout = 0.50f;

		[Tooltip("Delay before entering falling state (useful for stairs/ramps)")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded Check")]
		[Tooltip("Current grounded state")]
		public bool Grounded = true;

		[Tooltip("Vertical offset for ground check sphere")]
		public float GroundedOffset = -0.14f;

		[Tooltip("Radius of ground check sphere (should match CharacterController radius)")]
		public float GroundedRadius = 0.28f;

		[Tooltip("What layers count as ground?")]
		public LayerMask GroundLayers;

		[Header("Ledge Grab")]
		[Tooltip("Reference to a box collider used for ledge detection")]
		public GameObject LedgeGrabBox;

		// Ledge state variables
		public RaycastHit LedgeHit;
		public RaycastHit LedgeCheck;
		public bool Hanging = false;

		[Header("Camera")]
		[Tooltip("Cinemachine follow target (camera anchor)")]
		public GameObject CinemachineCameraTarget;

		[Tooltip("Max upward look angle")]
		public float TopClamp = 70.0f;

		[Tooltip("Max downward look angle")]
		public float BottomClamp = -30.0f;

		[Tooltip("Additional camera angle offset")]
		public float CameraAngleOverride = 0.0f;

		[Tooltip("Lock camera rotation (useful for cutscenes or fixed cams)")]
		public bool LockCameraPosition = false;

		[Header("Audio Clips")]
		public AudioClip LandingAudioClip;
		public AudioClip[] FootstepAudioClips;
		[Range(0, 1)] public float FootstepAudioVolume = 0.5f;

		// ======================================================================================
		// PRIVATE VARIABLES
		// ======================================================================================


		//State Machine
		private StateMachine playerStateMachine;




		// Cinemachine target rotation
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

		// Movement state
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// Timers
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// Animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

		// Cached references
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private Animator _animator;
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private bool _hasAnimator;
		private const float _threshold = 0.01f;

		// ======================================================================================
		// INITIALIZATION
		// ======================================================================================

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
        return false;
#endif
			}
		}

		private void Awake()
		{
			// Cache main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			// Set initial yaw from Cinemachine camera target
			_cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

			// Cache references
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
      Debug.LogError("Missing Input System dependencies. Use Tools/Starter Assets/Reinstall Dependencies to fix.");
#endif

			AssignAnimationIDs();

			// Initialize timers
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

			//Initialize State Machine
			playerStateMachine = new StateMachine();
			playerStateMachine.Initialize(new GroundedState(this, playerStateMachine));

		}

		// ======================================================================================
		// UPDATE LOOPS
		// ======================================================================================

		private void Update()
		{
			_hasAnimator = TryGetComponent(out _animator);

			playerStateMachine.HandleInput();
			playerStateMachine.LogicUpdate();


			// Main player actions
			//LedgeGrab();
			//JumpAndGravity();
			//GroundedCheck();
			//Move();
		}

		private void FixedUpdate()
		{
			playerStateMachine.PhysicsUpdate();
		}



		private void LateUpdate()
		{
			CameraRotation();
		}

		// ======================================================================================
		// ANIMATION ID SETUP
		// ======================================================================================

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		// ======================================================================================
		// PLAYER ACTIONS
		// ======================================================================================

		// Check if player is grounded (sphere check)
		private void GroundedCheck()
		{
			if (!Hanging)
			{
				Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
				Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

				if (_hasAnimator)
				{
					_animator.SetBool(_animIDGrounded, Grounded);
				}
			}
		}

		// Handles camera rotation from input
		private void CameraRotation()
		{
			if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
			{
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
				_cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
			}

			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
				_cinemachineTargetPitch + CameraAngleOverride,
				_cinemachineTargetYaw,
				0.0f
			);
		}

		// Handles walking, sprinting, and ledge movement
		private void Move()
		{
			// STEP 1: Decide movement speed----------------------------------------
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
			// If no movement input, stop movement
			if (_input.move == Vector2.zero)
				targetSpeed = 0.0f;

			// Current horizontal speed (ignoring vertical velocity)
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f; // deadzone to prevent jitter
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// Smooth acceleration and deceleration
			if (currentHorizontalSpeed < targetSpeed - speedOffset ||
					currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// Gradually adjust speed to target
				_speed = Mathf.Lerp(
						currentHorizontalSpeed,
						targetSpeed * inputMagnitude,
						Time.deltaTime * SpeedChangeRate
				);

				// Round speed for precision
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				// Snap directly if within deadzone
				_speed = targetSpeed;
			}

			// Smooth animation blending
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
			if (_animationBlend < 0.01f)
				_animationBlend = 0f;

			// STEP 2: Determine movement direction (relative to camera)------
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			if (_input.move != Vector2.zero && !Hanging)
			{
				// Convert input direction into world rotation relative to camera
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
												+ _mainCamera.transform.eulerAngles.y;

				// Smooth rotation
				float rotation = Mathf.SmoothDampAngle(
						transform.eulerAngles.y,
						_targetRotation,
						ref _rotationVelocity,
						RotationSmoothTime
				);

				// Apply rotation to character
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}

			// Convert target rotation into movement vector
			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// STEP 3: Handle Movement (ledge vs ground)----------------------
			if (Hanging)
			{
				// Special ledge hanging movement
				LedgeMovement(inputDirection);
			}
			else
			{
				// Normal movement with vertical velocity (gravity/jumping)
				_controller.Move(
						targetDirection.normalized * (_speed * Time.deltaTime) +
						new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
				);
			}

			// STEP 4: Update animator parameters------------------------------
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
			}
		}


		// ======================================================================================
		// LEDGE PLACEHOLDERS
		// ======================================================================================

		private void LedgeGrab()
		{
			if (_controller.velocity.y < 0 && !Hanging && !Grounded)
			{
				var ledgeDetector = LedgeGrabBox.GetComponent<LedgeDetection>();
				if (ledgeDetector != null && ledgeDetector.ReturnOnLedge())
				{
					Hanging = true;

					// Grab position from detector
					Vector3 hangPos = ledgeDetector.ReturnGrabLocation();

					// Offset to place player in front of ledge
					Vector3 offset = transform.forward * -0.45f + transform.up * -2.1f;
					hangPos += offset;
					transform.position = hangPos;

					// Raycast toward ledge surface to get rotation
					Vector3 rayStart = hangPos + Vector3.up * 2.0f; // slightly above ledge
					Vector3 rayDir = (ledgeDetector.ReturnGrabLocation() - rayStart).normalized; // toward actual ledge
					float rayLength = 1.0f;

					Debug.DrawRay(rayStart, rayDir * rayLength, Color.blue, 2f);

					if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, rayLength, GroundLayers))
					{
						Vector3 lookDir = -hit.normal;
						lookDir.y = 0f;
						if (lookDir.sqrMagnitude > 0f)
							transform.forward = lookDir.normalized;
					}
				}
			}
		}

		[Header("Ledge Movement Settings")]
		[Tooltip("Distance from ledge center to check for horizontal movement")]
		[SerializeField] private float LedgeSphereOffset = 0.2f;

		[Tooltip("Radius of sphere for horizontal ledge check")]
		[SerializeField] private float LedgeSphereRadius = 0.05f;

		private void LedgeMovement(Vector3 inputDirection)
		{
			if (!Hanging) return;

			if (Mathf.Abs(inputDirection.x) < 0.01f) return;

			var ledgeDetector = LedgeGrabBox.GetComponent<LedgeDetection>();
			Vector3 ledgeEdge = ledgeDetector != null ? ledgeDetector.ReturnGrabLocation() : transform.position;

			// Determine left/right sphere positions along ledge
			Vector3 ledgeRight = Vector3.Cross(Vector3.up, (ledgeEdge - transform.position).normalized).normalized;
			Vector3 rightSphere = ledgeEdge + ledgeRight * LedgeSphereOffset;
			Vector3 leftSphere = ledgeEdge - ledgeRight * LedgeSphereOffset;

			// Cast small spheres downward to check if ledge exists
			bool canMoveRight = Physics.SphereCast(rightSphere + Vector3.up * 0.3f, LedgeSphereRadius, Vector3.down, out _, 0.3f, GroundLayers);
			bool canMoveLeft = Physics.SphereCast(leftSphere + Vector3.up * 0.3f, LedgeSphereRadius, Vector3.down, out _, 0.3f, GroundLayers);

			Vector3 moveDir = Vector3.zero;

			if (inputDirection.x > 0 && canMoveRight) moveDir = ledgeRight;
			if (inputDirection.x < 0 && canMoveLeft) moveDir = -ledgeRight;

			Debug.DrawRay(rightSphere + Vector3.up * 0.1f, Vector3.down * 0.3f, canMoveRight ? Color.green : Color.red, 0.1f);
			Debug.DrawRay(leftSphere + Vector3.up * 0.1f, Vector3.down * 0.3f, canMoveLeft ? Color.green : Color.red, 0.1f);

			_controller.Move(moveDir * (_speed * Time.deltaTime));
		}






		// ======================================================================================
		// JUMPING & GRAVITY
		// ======================================================================================

		private void JumpAndGravity()
		{
			if (Grounded || Hanging)
			{
				_fallTimeoutDelta = FallTimeout;

				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, false);
					_animator.SetBool(_animIDFreeFall, false);
				}

				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					if (_hasAnimator) _animator.SetBool(_animIDJump, true);

					if (Hanging) Hanging = false;
				}

				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				_jumpTimeoutDelta = JumpTimeout;

				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				else if (_hasAnimator)
				{
					_animator.SetBool(_animIDFreeFall, true);
				}

				_input.jump = false;
			}

			if (_verticalVelocity < _terminalVelocity && !Hanging)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		// ======================================================================================
		// UTILITIES
		// ======================================================================================

		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f) angle += 360f;
			if (angle > 360f) angle -= 360f;
			return Mathf.Clamp(angle, min, max);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			Gizmos.color = Grounded ? transparentGreen : transparentRed;

			Gizmos.DrawSphere(
				new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
				GroundedRadius
			);
		}

		// ======================================================================================
		// ANIMATION EVENTS (called via Animator)
		// ======================================================================================

		private void OnFootstep(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
			{
				int index = Random.Range(0, FootstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
			}
		}

		private void OnLand(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > 0.5f)
			{
				AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
			}
		}
	}
}
