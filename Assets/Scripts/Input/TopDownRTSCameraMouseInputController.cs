﻿using UnityEngine;

public class TopDownRTSCameraMouseInputController : MonoBehaviour
{
	// Our Camera Object
	private Camera _cameraObject;

	// Movement variables
	[Range(0,40)]
	public int
		PanThreshold = 20;
	[Range(0,50)]
	public float
		MovementSpeed = 10.0f;
	private Vector3 _mInitialPosition;
	private Vector3 _mTargetPosition = Vector3.zero;
	private Vector3 _mCCameraMoveVel = Vector3.zero;
	private float _rotationY;
	private bool _rotating;
	
	// Zoom variables
	[Range(0,10)]
	public float
		MaxZoom = 5.0f;
	[Range(-10,0)]
	public float
		MinZoom = -2.0f;
	[Range(0,50)]
	public float
		ZoomSpeed = 10.0f;
	private float _initialZoom;
	private float _mTargetZoom;
	private Vector3 _mCCameraZoomVel = Vector3.zero;
	
	// Rotation variables
	public float MaxRotation = 25;
	[Range(0,50)]
	public float
		RotationSpeed = 10.0f;
	private float _initialRotation;
	private float _mCCameraRotateVel;


	// General update movement
	[Range(0.01f,2.0f)]
	public float
		CameraMoveSpeed = 0.3f;

	private void Start ()
	{
		// Assign our camera object.
		_cameraObject = GetComponentInChildren<Camera> ();

		// Set initial target position
		_mInitialPosition = transform.position;
		_mTargetPosition = _mInitialPosition;
		
		// Get initial Camera Zoom
		_initialZoom = Mathf.Clamp(_cameraObject.transform.localPosition.z,MinZoom,MaxZoom);
		_mTargetZoom = _initialZoom;
		
		// Fix the camera systemheight (this never changes)
		_mTargetPosition.y = transform.position.y;
		
		// Force initial camera position
		transform.position = _mTargetPosition;

	}
	
	////////////////////////////////////////////////////
	/// GAMEPLAY									  //
	///////////////////////////////////////////////////
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	private void LateUpdate ()
	{
		// Update the camera position
		UpdatePosition ();

		// Update the camera rotation
		UpdateCameraRotation ();
		
		// Execute Camera Zoom
		UpdateZoom ();
	}

	/// <summary>
	/// Updates the camera rotation.
	/// </summary>
	private void UpdateCameraRotation ()
	{
		_rotating = false;
		if (Input.GetMouseButton (1)) {
			_rotating = true;
			_rotationY += Input.GetAxis ("Mouse X") * RotationSpeed;
			transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, _rotationY, 0);
		}
	}
	
	/// <summary>
	/// Updates the zoom.
	/// </summary>
	private void UpdateZoom ()
	{
		// Zoom function
		var deltaZoom = Input.GetAxis ("Mouse ScrollWheel");
		if (!(Mathf.Approximately (deltaZoom, 0f))) {
			if (deltaZoom < 0) {
				_mTargetZoom = Mathf.Clamp (_mTargetZoom - Time.deltaTime * ZoomSpeed, MinZoom, MaxZoom);
			} else {
				_mTargetZoom = Mathf.Clamp (_mTargetZoom + Time.deltaTime * ZoomSpeed, MinZoom, MaxZoom);
			}
		}
		
		// Execute zoom
		var targetLocalPosition = _cameraObject.transform.localPosition;
		targetLocalPosition.z = _mTargetZoom;
		_cameraObject.transform.localPosition = Vector3.SmoothDamp (_cameraObject.transform.localPosition, targetLocalPosition,
		                                                          ref _mCCameraZoomVel, CameraMoveSpeed);
		
		// Get normalized required rotation
		var normRotation = Mathf.Clamp (_mTargetZoom / MaxZoom, 0, 1);
		var targetEulerAngles = normRotation * (-MaxRotation);
		var currentEulerAngles = _cameraObject.transform.localEulerAngles.x;
		currentEulerAngles = Mathf.SmoothDampAngle (currentEulerAngles, targetEulerAngles, ref _mCCameraRotateVel, CameraMoveSpeed);
		
		// Execute rotation
		var finalRotation = _cameraObject.transform.localEulerAngles;
		finalRotation.x = currentEulerAngles;
		_cameraObject.transform.localEulerAngles = finalRotation;
	}

	/// <summary>
	/// Updates the camera movement.
	/// </summary>
	private void UpdatePosition ()
	{
		// Get camera edge
		var mouseEdge = MouseScreenEdge (PanThreshold);
		
		if (!(Mathf.Approximately (mouseEdge.x, 0f))) {
			//Move your camera depending on the sign of mouse.Edge.x
			if (mouseEdge.x < 0) {
				_mTargetPosition -= transform.right * Time.deltaTime * MovementSpeed;
			} else {
				_mTargetPosition += transform.right * Time.deltaTime * MovementSpeed;
			}
		}
		if (!(Mathf.Approximately (mouseEdge.y, 0f))) {
			//Move your camera depending on the sign of mouse.Edge.y
			if (mouseEdge.y < 0) {
				_mTargetPosition -= transform.forward * Time.deltaTime * MovementSpeed;
			} else {
				_mTargetPosition += transform.forward * Time.deltaTime * MovementSpeed;
			}
		}
		
		// Fix the camera height (this never changes)
		_mTargetPosition.y = transform.position.y;
		
		// Update the position
		transform.position = Vector3.SmoothDamp (transform.position, _mTargetPosition, ref _mCCameraMoveVel, CameraMoveSpeed);
	}

	////////////////////////////////////////////////////
	/// HELPERS										  //
	///////////////////////////////////////////////////
	/// <summary>
	/// Gets if the mouse is hitting the edge of the screen
	/// </summary>
	/// <returns>The screen edge.</returns>
	/// <param name="margin">Margin.</param>
	private Vector2 MouseScreenEdge (int margin)
	{
		if (_rotating)
			return Vector2.zero;
		
		//Margin is calculated in px from the edge of the screen
		var half = new Vector2 (Screen.width / 2, Screen.height / 2);
		
		//If mouse is dead center, (x,y) would be (0,0)
		var x = Input.mousePosition.x - half.x;
		var y = Input.mousePosition.y - half.y;   
		
		//If x is not within the edge margin, then x is 0;
		//In another word, not close to the edge
		if (Mathf.Abs (x) > half.x - margin) {
			x += (half.x - margin) * ((x < 0) ? 1 : -1);
		} else {
			x = 0f;
		}
		
		if (Mathf.Abs (y) > half.y - margin) {
			y += (half.y - margin) * ((y < 0) ? 1 : -1);
		} else {
			y = 0f;
		}
		return new Vector2 (x, y);
	}
}
