using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OperationBlackwell.Core {
    public class Unit : MonoBehaviour {
		private State state_;

		public List<Vector3> pathRoute_ { get; private set; }
		private int pathIndex_;
		private bool routeSet_;

		private enum State {
			Normal,
			Moving
		}

		private void Awake() {
			state_ = State.Normal;
			pathRoute_ = new List<Vector3>();
			routeSet_ = false;
		}

		private void Update() {
			switch(state_) {
				case State.Normal:
					break;
				case State.Moving:
					break;
				default: 
					break;
			}
		}
	
		public void MoveTo(Vector3 targetPosition) {
			state_ = State.Moving;
			pathRoute_ = GridPathfinding.Instance.GetPathRoute(transform.position, targetPosition).pathVectorList;
			if(pathRoute_.Count > 0) {
				pathIndex_ = 0;
				routeSet_ = true;
			} else {
				pathIndex_ = -1;
				state_ = State.Normal;
			}
		}

		public Vector3 GetPosition() {
			return transform.position;
        }

		public bool IsRouteSet() {
			return routeSet_;
		}
	}
}
