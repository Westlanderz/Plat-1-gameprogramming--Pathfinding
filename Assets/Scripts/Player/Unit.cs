using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OperationBlackwell.Core {
    public class Unit : MonoBehaviour {
		[SerializeField] private float moveSpeed_;

		public List<Vector3> pathRoute_ { get; private set; }
		private int pathIndex_;
		private bool routeSet_;
		private Vector3 targetPosition_;

		private void Awake() {
			pathRoute_ = new List<Vector3>();
			pathIndex_ = -1;
			routeSet_ = false;
			targetPosition_ = Vector3.zero;
		}

		private void Update() {
			if(pathIndex_ != -1 && GameController.Instance.GetState() == GameController.GameState.Executing) {
				// Move to next path position
				Vector3 nextPathPosition;
				if(pathRoute_.Count == 1) {
					nextPathPosition = pathRoute_[pathIndex_];
				} else {
					nextPathPosition = pathRoute_[pathIndex_ + 1];
				}
				Vector3 moveVelocity = (nextPathPosition - transform.position).normalized;
				GetComponent<IMoveVelocity>().SetVelocity(moveVelocity);
				if(Vector3.Distance(transform.position, nextPathPosition) < 0.1f) {
					GameController.Instance.ResetHeroTile(this);
				}
			} else {
				// Stop moving
				GetComponent<IMoveVelocity>().SetVelocity(Vector3.zero);
				pathIndex_ = -1;
			}
		}
	
		public void MoveTo(Vector3 targetPosition) {
			if(targetPosition_ == Vector3.zero) {
				targetPosition_ = targetPosition;
			}
			PathRoute route = GridPathfinding.Instance.GetPathRoute(transform.position, targetPosition);
			pathRoute_ = route.pathVectorList;
			if(pathRoute_.Count > 0) {
				pathIndex_ = 0;
				routeSet_ = true;
			} else {
				pathIndex_ = -1;
			}
		}

		public Vector3 GetPosition() {
			return transform.position;
        }

		public bool IsRouteSet() {
			return routeSet_;
		}

		public Vector3 GetTargetPosition() {
			return targetPosition_;
		}
	}
}
