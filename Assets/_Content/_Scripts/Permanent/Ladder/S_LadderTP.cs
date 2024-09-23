using UnityEngine;

public class LadderTP : MonoBehaviour
{
    private Vector3 _teleportTo;
    private bool _playerIsIn = false;
    private NewCharacterMotor _character;
    private bool _isBottomTP;

    private void Update()
    {
		// temp
		HandleInputs();
	}

	private void HandleInputs()
	{
		if (!_playerIsIn) return;

		if (_isBottomTP)
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				_character.transform.position = _teleportTo;
			}
		}
		else
		{
			if (Input.GetKey(KeyCode.DownArrow))
			{
				_character.transform.position = _teleportTo;
			}
		}
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NewCharacterMotor>(out var character))
        {
            _playerIsIn = true;
            _character = character;
            Debug.Log("Player is in");
        }
    }

    public void SetVariables(Vector3 origin, Vector3 destination, bool isBottomTP)
    {
        transform.position = origin;
        _teleportTo = destination;
        _isBottomTP = isBottomTP;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NewCharacterMotor>(out var character))
        {
            _playerIsIn = false;
            Debug.Log("Player is out");
        }
    }
}
