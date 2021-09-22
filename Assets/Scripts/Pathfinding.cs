using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {
    [SerializeField] private Transform start, end;
    private Grid grid;
    void Awake() {
        grid = GetComponent<Grid>();
    }

    void Update() {
        FindPath(start.position, end.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos) {
        //Find the path from startPos to targetPos
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Node current;
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        openList.Add(startNode);

        while(openList.Count() > 0) {
            current = openList[0];
            for(int i = 1; i < openList.Count; i++) {
                if(openList[i].fCost < current.fCost || openList[i].fCost == current.fCost && openList[i].hCost < current.hCost) {
                    if(openList[i].hCost < current.hCost)
                        current = openList[i];
                }
            }
            
            openList.Remove(current);
            closedList.Add(current);

            if(current == targetNode) {
                Retrace(startNode, targetNode);
                return;
            }

            foreach(Node neighbour in grid.GetNeighbours(current)) {
                if(!neighbour.walkable || closedList.Contains(neighbour))
                    continue;

                int newCostToNeighbour = current.gCost + GetDistance(current, neighbour);
                if(newCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour)) {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = current;   
                    if(!openList.Contains(neighbour))
                        openList.Add(neighbour);                
                }
            }
        }
    }

    void Retrace(Node start, Node end) {
        List<Node> path = new List<Node>();
        Node current = end;
        while(current != start) {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX-dstY);
		return 14 * dstX + 10 * (dstY-dstX);
	}
}
