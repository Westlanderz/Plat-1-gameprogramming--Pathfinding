using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperationBlackwell.Core {
	public class Tilemap {
		public event System.EventHandler OnLoaded;

		private Grid<Node> grid_;

		public Tilemap(Grid<Node> grid) {
			this.grid_ = grid;
		}

		public Grid<Node> GetGrid() {
			return grid_;
		}

		public void SetNodeSprite(Vector3 worldPosition, Node.NodeSprite nodeSprite) {
			Node node = grid_.GetGridObject(worldPosition);
			if(node != null) {
				node.SetNodeSprite(nodeSprite);
			}
		}

		public void SetTilemapVisual(TilemapVisual tilemapVisual) {
			tilemapVisual.SetGrid(this, grid_);
		}

		public class SaveObject {
			public Node.SaveObject[] nodeSaveObjectArray;
		}
	
		public void Save(System.String name) {
			List<Node.SaveObject> nodeSaveObjectList = new List<Node.SaveObject>();
			for(int x = 0; x < grid_.gridSizeX; x++) {
				for(int y = 0; y < grid_.gridSizeY; y++) {
					Node node = grid_.GetGridObject(x, y);
					nodeSaveObjectList.Add(node.Save());
				}
			}

			SaveObject saveObject = new SaveObject { nodeSaveObjectArray = nodeSaveObjectList.ToArray() };
			SaveSystem.SaveObject(saveObject, name);
		}

		public void Load(System.String name) {
			SaveObject saveObject = SaveSystem.LoadObject<SaveObject>(name);
			foreach(Node.SaveObject nodeSaveObject in saveObject.nodeSaveObjectArray) {				
				Node node = grid_.GetGridObject(nodeSaveObject.x, nodeSaveObject.y);
				node.Load(nodeSaveObject);
			}
			OnLoaded?.Invoke(this, System.EventArgs.Empty);
		}

		public class Node {
			public enum NodeSprite {
				// Default sprite.
				NONE,
				FLOOR,
				WALL
			}
			// Holds if the tile can be walked over.
			public bool walkable;
			public Vector3 worldPosition { get; private set; }
			public int gridX { get; private set; }
			public int gridY { get; private set; }
			private NodeSprite nodeSprite_;

			private bool isValidMovePosition_;
			private Unit unitGridCombat_;

			private Grid<Node> grid_;

			public Node(Vector3 worldPosition, int gridX, int gridY, Grid<Node> grid, bool walkable) {
				this.worldPosition = worldPosition;
				this.gridX = gridX;
				this.gridY = gridY;
				this.grid_ = grid;
				this.walkable = walkable;
			}

			[System.Serializable]
			public class SaveObject {
				public NodeSprite nodeSprite;
				public int x;
				public int y;
				public bool walkable;
			}

			/*
			* Save - Load
			* */
			public SaveObject Save() {
				return new SaveObject { 
					nodeSprite = this.nodeSprite_,
					x = this.gridX,
					y = this.gridY,
					walkable = this.walkable
				};
			}

			public void Load(SaveObject saveObject) {
				this.nodeSprite_ = saveObject.nodeSprite;
				this.walkable = saveObject.walkable;
			}

			public NodeSprite GetNodeSprite() {
				return nodeSprite_;
			}

			public override string ToString() {
				return nodeSprite_.ToString();
			}

			public void SetNodeSprite(NodeSprite nodeSprite) {
				this.nodeSprite_ = nodeSprite;
				if(nodeSprite == NodeSprite.FLOOR) {
					this.walkable = true;
				} else if(nodeSprite == NodeSprite.WALL) {
					this.walkable = false;
				}
				grid_.TriggerGridObjectChanged(gridX, gridY);
			}

			public void SetIsValidMovePosition(bool set) {
				isValidMovePosition_ = set;
			}

			public bool GetIsValidMovePosition() {
				return isValidMovePosition_;
			}

			public void SetUnitGridCombat(Unit unitGridCombat) {
				this.unitGridCombat_ = unitGridCombat;
				this.walkable = false;
			}

			public void ClearUnitGridCombat() {
				SetUnitGridCombat(null);
				this.walkable = true;
			}

			public Unit GetUnitGridCombat() {
				return unitGridCombat_;
			}
		}
	}
}
