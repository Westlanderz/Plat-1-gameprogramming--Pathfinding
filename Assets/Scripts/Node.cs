using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public bool walkable { get; private set;}
    public Vector3 worldPosition { get; private set;}

    public Node(bool _walkable, Vector3 _worldPosition) {
        walkable = _walkable;
        worldPosition = _worldPosition;
    }
}
