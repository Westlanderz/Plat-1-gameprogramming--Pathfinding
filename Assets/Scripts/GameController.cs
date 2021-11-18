using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OperationBlackwell.Core {
	public class GameController : MonoBehaviour {
		private const bool DebugMovement = false;

		public static GameController Instance { get; private set; }

		[SerializeField] private Vector3 gridWorldSize_;
		[SerializeField] private float cellSize_;

		[SerializeField] private bool drawGridLines_;
		[SerializeField] private Unit heroPrefab_;

		[SerializeField] private MovementTilemapVisual arrowTilemapVisual_;
		[SerializeField] private MovementTilemapVisual selectorTilemapVisual_;
		[SerializeField] private MovementTilemapVisual unitTilemapVisual_;
		[SerializeField] private MovementTilemapVisual moveTilemapVisual_;

		private MovementTilemap arrowTilemap_;
		private MovementTilemap selectorTilemap_;
		private MovementTilemap unitTilemap_;
		private MovementTilemap moveTilemap_;

		public Grid<Tilemap.Node> grid { get; private set; }

		public GridPathfinding gridPathfinding { get; private set; }
		public Tilemap tilemap { get; private set; }

		[SerializeField] private TilemapVisual tilemapVisual_;
		private Tilemap.Node.NodeSprite nodeSprite_;

		private GameState state_;
		private List<Unit> heroes_;
		private Unit currentHero_;

		public event EventHandler<GameState> OnGameStateChanged;

		public enum GameState {
			None,
			Heroes,
			Objectives,
			Obstacles,
			Executing,
			Waiting,
			Finished
		}

		private void Awake() {
			state_ = GameState.None;
			grid = new Grid<Tilemap.Node>((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0), 
				(Grid<Tilemap.Node> g, Vector3 worldPos, int x, int y) => new Tilemap.Node(worldPos, x, y, g, true), drawGridLines_);
			tilemap = new Tilemap(grid);
			Instance = this;
			Vector3 origin = new Vector3(0, 0);
			heroes_ = new List<Unit>();

			gridPathfinding = new GridPathfinding(origin + new Vector3(1, 1) * cellSize_ * .5f, new Vector3(gridWorldSize_.x, gridWorldSize_.y) * cellSize_, cellSize_);
			arrowTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
			selectorTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
			unitTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
			moveTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
		}

		private void Start() {
			tilemap.SetTilemapVisual(tilemapVisual_);
			arrowTilemap_.SetTilemapVisual(arrowTilemapVisual_);
			selectorTilemap_.SetTilemapVisual(selectorTilemapVisual_);
			unitTilemap_.SetTilemapVisual(unitTilemapVisual_);
			moveTilemap_.SetTilemapVisual(moveTilemapVisual_);
			PaintMap();
			OnGameStateChanged?.Invoke(this, state_);
		}

		private void Update() {
			PaintSelectorTool();
			switch(state_) {
				case GameState.None:
					break;
				case GameState.Obstacles:
					PaintingController.Instance.HandlePainting();
					break;
				case GameState.Heroes:
					UpdateValidMovePositions();
					PlaceHeroes();
					break;
				case GameState.Objectives:
					UpdateValidMovePositions();
					SelectHero();
					UpdateHeroArrows();
					if(currentHero_ != null) {
						Tilemap.Node gridObject = grid.GetGridObject(Utils.GetMouseWorldPosition());
						if(gridObject == null) {
							break;
						}
						ResetArrowTool();
						if(gridObject.GetIsValidMovePosition()) {
							PaintArrowTool(currentHero_.GetPosition(), Utils.GetMouseWorldPosition());
							if(Input.GetMouseButtonDown(0)) {
								currentHero_.MoveTo(Utils.GetMouseWorldPosition());
								currentHero_ = null;
								ResetHeroVisual();
							}
						}
					}
					break;
				case GameState.Executing:
					UpdateValidMovePosition();
					ResetArrowTool();
					UpdateHeroArrows();
					ResetHeroVisual();
					break;
				case GameState.Waiting:
					break;
				case GameState.Finished:
					break;
				default:
					break;
			}

			if(Input.GetKeyDown(KeyCode.Return)) {
				ChangeState();
			}
			if(Input.GetKeyDown(KeyCode.Backspace)) {
				RevertState();
			}
			if(Input.GetKeyDown(KeyCode.R)) {
				ResetLevel();
			}
		}

		public Grid<Tilemap.Node> GetGrid() {
			return grid;
		}

		public MovementTilemap GetArrowTilemap() {
			return arrowTilemap_;
		}
		
		public MovementTilemap GetSelectorTilemap() {
			return selectorTilemap_;
		}

		public MovementTilemap GetUnitTilemap() {
			return unitTilemap_;
		}

		private void ChangeState() {
			if(state_ == GameState.None) {
				state_ = GameState.Obstacles;
				ResetArrowTool();
			} else if(state_ == GameState.Obstacles) {
				state_ = GameState.Heroes;
				ResetArrowTool();
			} else if(state_ == GameState.Heroes) {
				state_ = GameState.Objectives;
				ResetArrowTool();
			} else if(state_ == GameState.Objectives) {
				state_ = GameState.Executing;
				currentHero_ = null;
				
			} else if(state_ == GameState.Executing) {
				state_ = GameState.Finished;
			}
			OnGameStateChanged?.Invoke(this, state_);
		}

		private void RevertState() {
			if(state_ == GameState.Obstacles) {
				state_ = GameState.None;
				ResetArrowTool();
			} else if(state_ == GameState.Heroes) {
				state_ = GameState.Obstacles;
				ResetArrowTool();
			} else if(state_ == GameState.Objectives) {
				state_ = GameState.Heroes;
				ResetArrowTool();
			} else if(state_ == GameState.Executing) {
				state_ = GameState.Objectives;
				currentHero_ = null;
			} else if(state_ == GameState.Finished) {
				state_ = GameState.Executing;
			}
			OnGameStateChanged?.Invoke(this, state_);
		}

		private void ResetLevel() {
			state_ = GameState.None;
			currentHero_ = null;
			OnGameStateChanged?.Invoke(this, state_);
			ResetArrowTool();
			ResetHeroVisual();
		}

		private void PaintMap() {
			foreach(Tilemap.Node node in grid.GetAllGridObjects()) {
				nodeSprite_ = Tilemap.Node.NodeSprite.FLOOR;
				node.SetNodeSprite(nodeSprite_);
				grid.TriggerGridObjectChanged(node.gridX, node.gridY);
			}
		}

		private void PaintSelectorTool() {
			ResetSelectorTool();
			Tilemap.Node gridObject = grid.GetGridObject(Utils.GetMouseWorldPosition());
			if(gridObject == null) {
				return;
			}
			selectorTilemap_.SetTilemapSprite(
				gridObject.gridX, gridObject.gridY, MovementTilemap.TilemapObject.TilemapSprite.Move
			);
		}

		private void ResetSelectorTool() {
			selectorTilemap_.SetAllTilemapSprite(
				MovementTilemap.TilemapObject.TilemapSprite.None
			);
		}

		private void PaintArrowTool(Vector3 start, Vector3 end) {
			List<PathNode> currentPathUnit_ = GameController.Instance.gridPathfinding.GetPath(start, end);
			int x = 0, y = 0;
			foreach(PathNode node in currentPathUnit_) {
				x = node.xPos;
				y = node.yPos;

				if(node.parent == null) {
					continue;
				}

				if(grid.GetGridObject(x, y).GetUnitGridCombat() != currentHero_ && grid.GetGridObject(x, y) != grid.GetGridObject(start)) {
					if(grid.GetGridObject(x, y) != grid.GetGridObject(end)) {
						if((node.parent.xPos > x || node.parent.xPos < x) && node.parent.yPos == y) {
							arrowTilemap_.SetRotation(x, y, 90f);
							arrowTilemap_.SetTilemapSprite(x, y, MovementTilemap.TilemapObject.TilemapSprite.ArrowStraight);
						} 
						if((node.parent.yPos > y || node.parent.yPos < y) && node.parent.xPos == x) {
							arrowTilemap_.SetRotation(x, y, 0f);
							arrowTilemap_.SetTilemapSprite(x, y, MovementTilemap.TilemapObject.TilemapSprite.ArrowStraight);
						}
					} else {
						if(node.parent.xPos > x && node.parent.yPos == y) {
							arrowTilemap_.SetRotation(x, y, 90f);
						} else if(node.parent.xPos < x && node.parent.yPos == y) {
							arrowTilemap_.SetRotation(x, y, -90f);
						} else if(node.parent.xPos == x && node.parent.yPos > y) {
							arrowTilemap_.SetRotation(x, y, 180f);
						} else if(node.parent.xPos == x && node.parent.yPos < y) {
							arrowTilemap_.SetRotation(x, y, 0f);
						}
						arrowTilemap_.SetTilemapSprite(x, y, MovementTilemap.TilemapObject.TilemapSprite.ArrowEnd);
					}

					if(node.parent.parent == null) {
						continue;
					}

					if((node.parent.parent.xPos == node.parent.xPos && node.parent.xPos < x 
						&& node.parent.parent.yPos > node.parent.yPos && node.parent.yPos == y)
						|| (node.parent.parent.xPos > node.parent.xPos && node.parent.xPos == x 
						&& node.parent.parent.yPos == node.parent.yPos && node.parent.yPos < y)) {
						arrowTilemap_.SetRotation(node.parent.xPos, node.parent.yPos, 90f);
						arrowTilemap_.SetTilemapSprite(node.parent.xPos, node.parent.yPos, MovementTilemap.TilemapObject.TilemapSprite.ArrowCorner);
					}
					if((node.parent.parent.xPos == node.parent.xPos && node.parent.xPos > x 
						&& node.parent.parent.yPos > node.parent.yPos && node.parent.yPos == y)
						|| (node.parent.parent.xPos < node.parent.xPos && node.parent.xPos == x 
						&& node.parent.parent.yPos == node.parent.yPos && node.parent.yPos < y)) {
						arrowTilemap_.SetRotation(node.parent.xPos, node.parent.yPos, 180f);
						arrowTilemap_.SetTilemapSprite(node.parent.xPos, node.parent.yPos, MovementTilemap.TilemapObject.TilemapSprite.ArrowCorner);
					}
					if((node.parent.parent.xPos == node.parent.xPos && node.parent.xPos < x 
						&& node.parent.parent.yPos < node.parent.yPos && node.parent.yPos == y)
						|| (node.parent.parent.xPos > node.parent.xPos && node.parent.xPos == x 
						&& node.parent.parent.yPos == node.parent.yPos && node.parent.yPos > y)) {
						arrowTilemap_.SetRotation(node.parent.xPos, node.parent.yPos, 0f);
						arrowTilemap_.SetTilemapSprite(node.parent.xPos, node.parent.yPos, MovementTilemap.TilemapObject.TilemapSprite.ArrowCorner);
					}
					if((node.parent.parent.xPos == node.parent.xPos && node.parent.xPos > x 
						&& node.parent.parent.yPos < node.parent.yPos && node.parent.yPos == y)
						|| (node.parent.parent.xPos < node.parent.xPos && node.parent.xPos == x 
						&& node.parent.parent.yPos == node.parent.yPos && node.parent.yPos > y)) {
						arrowTilemap_.SetRotation(node.parent.xPos, node.parent.yPos, -90f);
						arrowTilemap_.SetTilemapSprite(node.parent.xPos, node.parent.yPos, MovementTilemap.TilemapObject.TilemapSprite.ArrowCorner);
					}
				}
			}
		}

		private void ResetArrowTool() {
			arrowTilemap_.SetAllTilemapSprite(
				MovementTilemap.TilemapObject.TilemapSprite.None
			);
		}

		private void UpdateValidMovePositions() {
			moveTilemap_.SetAllTilemapSprite(
				MovementTilemap.TilemapObject.TilemapSprite.None
			);
			foreach(Tilemap.Node node in grid.GetAllGridObjects()) {
				if(node.walkable) {
					node.SetIsValidMovePosition(true);
					gridPathfinding.SetWalkable(node.gridX, node.gridY, true);
					moveTilemap_.SetTilemapSprite(node.gridX, node.gridY, MovementTilemap.TilemapObject.TilemapSprite.Move);
				} else {
					node.SetIsValidMovePosition(false);
					gridPathfinding.SetWalkable(node.gridX, node.gridY, false);
				}
			}
			if(currentHero_ != null) {
				Tilemap.Node gridObject = grid.GetGridObject(currentHero_.GetPosition());
				gridObject.SetIsValidMovePosition(true);
				gridPathfinding.SetWalkable(gridObject.gridX, gridObject.gridY, true);
			}
		}

		private void UpdateValidMovePosition() {
			moveTilemap_.SetAllTilemapSprite(
				MovementTilemap.TilemapObject.TilemapSprite.None
			);
			foreach(Tilemap.Node node in grid.GetAllGridObjects()) {
				if(node.walkable) {
					node.SetIsValidMovePosition(true);
					gridPathfinding.SetWalkable(node.gridX, node.gridY, true);
					moveTilemap_.SetTilemapSprite(node.gridX, node.gridY, MovementTilemap.TilemapObject.TilemapSprite.Move);
				} else {
					node.SetIsValidMovePosition(false);
					gridPathfinding.SetWalkable(node.gridX, node.gridY, false);
				}
			}
			foreach(Unit hero in heroes_) {
				Tilemap.Node gridObject = grid.GetGridObject(hero.GetPosition());
				gridObject.SetIsValidMovePosition(true);
				gridPathfinding.SetWalkable(gridObject.gridX, gridObject.gridY, true);
			}
		}

		private void PlaceHeroes() {
			Tilemap.Node gridObject = grid.GetGridObject(Utils.GetMouseWorldPosition());
			if(gridObject == null || !gridObject.GetIsValidMovePosition() || gridObject.GetUnitGridCombat() != null) {
				return;
			}
			if(Input.GetMouseButtonDown(0)) {
				// The Rounding to int with Mathf.RoundToInt is to prevent the hero from being placed on the tilemap border.
				// With the addition of cellSize / 2 the unit will spawn in the middle of the tile.
				Unit unit = Instantiate(heroPrefab_, 
					new Vector3(Mathf.RoundToInt(gridObject.worldPosition.x) + cellSize_ / 2, Mathf.RoundToInt(gridObject.worldPosition.y) + cellSize_ / 2), 
					Quaternion.identity);
				gridObject.SetUnitGridCombat(unit);
				heroes_.Add(unit);
				UpdateValidMovePositions();
			}
		}

		private void SelectHero() {
			ResetHeroVisual();
			Tilemap.Node gridObject = grid.GetGridObject(Utils.GetMouseWorldPosition());
			if(gridObject == null || gridObject.GetUnitGridCombat() == null || gridObject.GetUnitGridCombat().IsRouteSet()) {
				return;
			}
			if(Input.GetMouseButtonDown(0)) {
				currentHero_ = gridObject.GetUnitGridCombat();
			}
			if(currentHero_ != null){
				gridObject = grid.GetGridObject(currentHero_.GetPosition());
				unitTilemap_.SetTilemapSprite(
					gridObject.gridX, gridObject.gridY, MovementTilemap.TilemapObject.TilemapSprite.Move
				);
			}
		}

		private void ResetHeroVisual() {
			unitTilemap_.SetAllTilemapSprite(
				MovementTilemap.TilemapObject.TilemapSprite.None
			);
			if(currentHero_ != null){
				Tilemap.Node gridObject = grid.GetGridObject(currentHero_.GetPosition());
				unitTilemap_.SetTilemapSprite(
					gridObject.gridX, gridObject.gridY, MovementTilemap.TilemapObject.TilemapSprite.Move
				);
			}
		}

		private void UpdateHeroArrows() {
			foreach(Unit hero in heroes_) {
				if(hero.pathRoute_.Count > 0) {
					PaintArrowTool(hero.GetPosition(), hero.pathRoute_[hero.pathRoute_.Count - 1]);
				}
			}
		}
	}
}
