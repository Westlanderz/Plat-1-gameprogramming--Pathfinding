using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperationBlackwell.Core {
	public class PaintingController : MonoBehaviour {
		public static PaintingController Instance { get; private set; }

		private Tilemap.Node.NodeSprite nodeSprite_;

		private void Awake() {
			Instance = this;
		}

		public void HandlePainting() {
			if(Input.GetMouseButtonDown(0)) {
				nodeSprite_ = Tilemap.Node.NodeSprite.FLOOR;
				Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
				Tilemap.Node node = GameController.Instance.grid.GetGridObject(mouseWorldPosition);
				node.SetNodeSprite(nodeSprite_);
				GameController.Instance.grid.TriggerGridObjectChanged(node.gridX, node.gridY);
			}
			if(Input.GetMouseButtonDown(1)) {
				nodeSprite_ = Tilemap.Node.NodeSprite.WALL;
				Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
				Tilemap.Node node = GameController.Instance.grid.GetGridObject(mouseWorldPosition);
				node.SetNodeSprite(nodeSprite_);
				GameController.Instance.grid.TriggerGridObjectChanged(node.gridX, node.gridY);
			}
		}

	}
}
