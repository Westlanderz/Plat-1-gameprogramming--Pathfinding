using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperationBlackwell.Core {
	public class MoveVelocity : MonoBehaviour, IMoveVelocity {
		[SerializeField] private float moveSpeed_;

		private Vector3 velocityVector_;
		private Rigidbody2D rigidbody2D_;

		private void Awake() {
			rigidbody2D_ = GetComponent<Rigidbody2D>();
		}

		public void SetVelocity(Vector3 velocityVector) {
			this.velocityVector_ = velocityVector;
		}

		private void FixedUpdate() {
			rigidbody2D_.velocity = velocityVector_ * moveSpeed_;
		}

		public void Disable() {
			this.enabled = false;
			rigidbody2D_.velocity = Vector3.zero;
		}

		public void Enable() {
			this.enabled = true;
		}
	}
}
