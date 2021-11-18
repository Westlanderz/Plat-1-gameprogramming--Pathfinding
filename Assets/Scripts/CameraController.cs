using UnityEngine;
using System.Collections;

namespace OperationBlackwell.Core {
	class CameraController : MonoBehaviour {
		private Camera camera_;
		private float cameraSpeed_ = 1f;

		/*
		* Initializes the camera controller
		* and sets the camera in the movement controller.
		*/
		private void Awake() {
			camera_ = Camera.main;
		}

		private void FixedUpdate() {
			HandleCameraMovement();
		}

		private void HandleCameraMovement(float distance = 10f) {
			Vector3 moveDir = new Vector3(0, 0);
			if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
				moveDir.y = +1;
			}
			if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
				moveDir.y = -1;
			}
			if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
				moveDir.x = -1;
			}
			if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
				moveDir.x = +1;
			}
			moveDir.Normalize();

			float moveSpeed = 15f;
			
			camera_.transform.position += moveDir * moveSpeed * Time.deltaTime;
		}
	}
}
