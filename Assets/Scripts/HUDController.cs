using UnityEngine;
using UnityEngine.UI;
using OperationBlackwell.Core;

namespace OperationBlackwell.UI {
	public class HUDController : MonoBehaviour {

		[SerializeField] private Text gamePhaseText_;

		private void Awake() {
			GameController.Instance.OnGameStateChanged += UpdateGamePhase;
		}

		private void UpdateGamePhase(object sender, GameController.GameState args) {
			gamePhaseText_.text = "Game phase: " + args;
		}
	}
}
