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

	private bool paused = false;

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

	public void SetPaused(bool newPaused)
	{
		bool wasPaused = paused;
		paused = newPaused;

		SetCursorState(!paused);

		Time.timeScale = paused ? 0 : 1;

		// Show/hide pause menu - using name search to avoid circular assembly dependency
		if (wasPaused != paused) 
		{
			// Find the PauseMenu Canvas by name to avoid direct type reference
			var pauseMenuGO = GameObject.Find("PauseMenuCanvas");
			if (pauseMenuGO == null)
			{
				// Try inactive objects
				var allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
				foreach (var canvas in allCanvases)
				{
					if (canvas.gameObject.name == "PauseMenuCanvas")
					{
						pauseMenuGO = canvas.gameObject;
						break;
					}
				}
			}
			if (pauseMenuGO != null)
			{
				pauseMenuGO.SetActive(paused);
			}
		}
	}

	private void Start()
	{
		SetCursorState(!paused);
	}

	private void Update()
	{
		// Escape key is intercepted by the browser to free the cursor:
		// https://docs.unity3d.com/2023.1/Documentation/Manual/webgl-input.html
		// so we detect this and act accordingly
		if (!CursorLocked && !paused)
		{
			// cursor unlocked but not paused so we must correct that
			SetPaused(true);
		}
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}
}