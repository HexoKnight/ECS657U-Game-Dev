using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool sprint;
	public bool magnetise;

	[Header("Movement Settings")]
	public bool analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool cursorInputForLook = true;

	private bool CursorLocked => Cursor.lockState == CursorLockMode.Locked;

	public void OnMove(InputValue value)
	{
		move = value.Get<Vector2>();
	}

	public void OnLook(InputValue value)
	{
		if (CursorLocked)
		{
			look = value.Get<Vector2>();
		}
		else
		{
			look = Vector2.zero;
		}
	}

	public void OnJump(InputValue value)
	{
		jump = value.isPressed;
	}

	public void OnSprint(InputValue value)
	{
		sprint = value.isPressed;
	}

	public void OnMagnetise(InputValue value)
	{
		magnetise = value.isPressed;
	}

	public void OnPause(InputValue value)
	{
		// only allow pausing with the keybind (ie. need to use Resume button to unpause)
		// this ensures we have focus and thus can always grab the cursor
		if (value.isPressed) SetPaused(true);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		// pause on focus loss
		if (!hasFocus) SetPaused(true);
	}

	public void SetPaused(bool paused)
	{
		SetCursorState(!paused);

		Time.timeScale = paused ? 0 : 1;

		// TODO: improve
		FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include).gameObject.SetActive(paused);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}
}