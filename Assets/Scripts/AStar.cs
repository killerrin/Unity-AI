using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
    public MazeGenerator Grid;
    public int BaseMovementCost = 10;

    public GameObject AStarCubePrefab;
    public Material PathCurrentNodeMaterial;
    public Material PathOpenedMaterial;
    public Material PathFoundMaterial;

    public bool debugMode = false;
    private bool setup = false;
    public bool includeDiagnols = false;
    // Slow down A* Speed
    private float delay = 0.0f;
    public float DELAY = 0.1f;

    // A* Variables
    public bool FoundTarget { get; private set; }

    private PriorityQueue<PriorityQueueNode<AStarNode>> m_openedList;
    private List<AStarNode> m_closedList;

    private AStarNode m_checkingNode;
    private AStarNode m_startNode;
    private AStarNode m_endNode;

	// Use this for initialization
	void Start () {
        FoundTarget = false;
	}

    void SetupAStar()
    {
        if (Grid == null) return;

        m_openedList = new PriorityQueue<PriorityQueueNode<AStarNode>>();
        m_closedList = new List<AStarNode>();

        // Place the End Position at a random spot on the map
        Vector2 endGridCell = Grid.GetRandomGridCell(0);
        m_endNode = new AStarNode(endGridCell);
        m_endNode.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, endGridCell, new Vector3(0.0f, -0.0f, 0.0f));

        // Place the Moveable block at the start in the first available position
        Vector2 StartGridCell = Grid.GetRandomGridCell(0);
        m_startNode = CreateNodeAtGridSpace(StartGridCell, null);
        FindAdjacentNodes(m_startNode);
        AddToOpenedList(m_startNode);

        m_startNode.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, StartGridCell);
        m_startNode.debugDraw.renderer.material = PathCurrentNodeMaterial;
        m_checkingNode = m_startNode;

        setup = true;
    }

    void ResetAStar()
    {
        setup = false;

        GameObject[] aStarVisiblePaths = GameObject.FindGameObjectsWithTag("CurrentSearch");
        for (int i = 0; i < aStarVisiblePaths.Length; i++) {
            Destroy(aStarVisiblePaths[i]);
        }

        m_checkingNode = null;
        m_startNode = null;
        m_endNode = null;

        FoundTarget = false;

        SetupAStar();
    }

	// Update is called once per frame
	void Update () {
        if (!setup) {
            SetupAStar();
        }

        // If space is pressed, reload the level
        if (Input.GetKeyDown(KeyCode.Space)) {
            ResetAStar();
        }

        // Delay Timer
        if (delay <= DELAY) {
            delay += Time.deltaTime;
            return;
        }
        else {
            delay -= DELAY;
        }

        // Write A* Code Here
        if (!FoundTarget) FindPath();
        else TraceBackPath();
	}

    void FindPath()
    {
        if (FoundTarget) return;
        if (m_openedList.Count == 0) return;

        for (int i = 0; i < m_checkingNode.Connections.Count; i++) {
            if (m_checkingNode.Connections[i] != null) {
                DetermineNodeValues(m_checkingNode, m_checkingNode.Connections[i]);
            }
        }

        if (!FoundTarget) {
            AddToClosedList(m_checkingNode);

            m_checkingNode = m_openedList.Dequeue().Data;
            FindAdjacentNodes(m_checkingNode);
        }
    }

    void TraceBackPath()
    {
        Debug.Log("Path Found");
        AStarNode node = m_endNode;

        do {
            if (node.debugDraw != null) {
                node.debugDraw.renderer.material = PathFoundMaterial;
            }

            node = node.Parent;
        }
        while (node != null);

    }

    void DetermineNodeValues(AStarNode currentNode, AStarNode targetNode)
    {
        if (targetNode == null) return;
        if (m_endNode.GridPosition == targetNode.GridPosition) {
            m_endNode.Parent = currentNode;
            FoundTarget = true;
            return;
        }

        // Check the Grid Cell to see if it is passable
        GridCell targetGridCell = Grid.GetGridCellAtGridPosition(targetNode.GridPosition);
        if (targetGridCell.ID == 1)
            return;

        if (!ClosedListContains(targetNode)) {
            targetNode.Parent = currentNode;

            if (OpenedListContains(targetNode)) {
                int newGCost = currentNode.GValue + BaseMovementCost;

                if (newGCost < targetNode.GValue) {
                    targetNode.GValue = newGCost;
                    targetNode.CalculateFValue();
                }
            }
            else {
                targetNode.GValue = currentNode.GValue + BaseMovementCost;
                targetNode.CalculateFValue();
                AddToOpenedList(targetNode);
            }
        }
    }


    void AddToOpenedList(AStarNode node) {
        m_openedList.Enqueue(new PriorityQueueNode<AStarNode>(node, node.HValue));
    }
    void AddToClosedList(AStarNode currentNode) {
        if (!ClosedListContains(currentNode))
            m_closedList.Add(currentNode);
    }

    bool OpenedListContains(AStarNode n)
    {
        for (int i = 0; i < m_openedList.Count; i++) {
            if (m_openedList[i].Data.GridPosition == n.GridPosition)
                return true;
        }
        return false;
    }
    bool OpenedListContains(Vector2 n)
    {
        for (int i = 0; i < m_openedList.Count; i++) {
            if (m_openedList[i].Data.GridPosition == n)
                return true;
        }
        return false;
    }
    bool ClosedListContains(AStarNode n)
    {
        for (int i = 0; i < m_closedList.Count; i++) {
            if (m_closedList[i].GridPosition == n.GridPosition)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Creates and calculates the node values required for A*
    /// </summary>
    /// <param name="gridSpace">Grid Location to which we create the AStarNode</param>
    /// <param name="parent">The Parent Node to our newly created Node</param>
    /// <returns>Node representing the Grid Position for use in A*</returns>
    AStarNode CreateNodeAtGridSpace(Vector2 gridSpace, AStarNode parent)
    {
        AStarNode temp = new AStarNode(gridSpace, parent);
        temp.MovementCost = BaseMovementCost;
        temp.HeuristicValue = CalculateManhattanDistance(temp.GridPosition, m_endNode.GridPosition);
        temp.CalculateFValue();
        return temp;
    }

    /// <summary>
    /// Finds and assigns the adjacent nodes to our inputted AStarNode.
    /// </summary>
    /// <param name="node">Node we will get the adjacent nodes for</param>
    /// <param name="debugMode">If True, it will instantiate a cube to the screen with the position of the node</param>
    void FindAdjacentNodes (AStarNode node)
    {
        Vector2 parentPos = new Vector2(-1,-1);
        if (node.Parent != null)
            parentPos = node.Parent.GridPosition;

        Vector2 northPos = node.GridPosition + new Vector2(0, 1);
        Vector2 southPos = node.GridPosition - new Vector2(0, 1);

        Vector2 eastPos = node.GridPosition + new Vector2(1, 0);
        Vector2 westPos = node.GridPosition - new Vector2(1, 0);

        // Up Down Left Right
        if (Grid.InGrid(northPos)) {
            if (parentPos != northPos && 
                Grid.GetGridCellAtGridPosition(northPos).ID != 1) {

                AStarNode north = CreateNodeAtGridSpace(northPos, node);
                Debug.Log("Found North: " + north.GridPosition.ToString());
                if (debugMode) {
                    north.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, northPos, new Vector3(0.0f, -0.5f, 0.0f));
                    north.debugDraw.renderer.material = PathOpenedMaterial;
                }
                node.Connections.Add(north);
            }
        }
        if (Grid.InGrid(southPos)) {
            if (parentPos != southPos &&
                Grid.GetGridCellAtGridPosition(southPos).ID != 1) {

                AStarNode south = CreateNodeAtGridSpace(southPos, node);
                Debug.Log("Found South: " + south.GridPosition.ToString());
                if (debugMode) {
                    south.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, southPos, new Vector3(0.0f, -0.5f, 0.0f));
                    south.debugDraw.renderer.material = PathOpenedMaterial;
                }
                node.Connections.Add(south);
            }
        }

        if (Grid.InGrid(eastPos)) {
            if (parentPos != eastPos &&
                Grid.GetGridCellAtGridPosition(eastPos).ID != 1) {

                AStarNode east = CreateNodeAtGridSpace(eastPos, node);
                Debug.Log("Found East: " + east.GridPosition.ToString());
                if (debugMode) {
                    east.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, eastPos, new Vector3(0.0f, -0.5f, 0.0f));
                    east.debugDraw.renderer.material = PathOpenedMaterial;
                }
                node.Connections.Add(east);
            }
        }
        if (Grid.InGrid(westPos)) {
            if (parentPos != westPos &&
                Grid.GetGridCellAtGridPosition(westPos).ID != 1) {

                AStarNode west = CreateNodeAtGridSpace(westPos, node);
                Debug.Log("Found West: " + west.GridPosition.ToString());
                if (debugMode) {
                    west.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, westPos, new Vector3(0.0f, -0.5f, 0.0f));
                    west.debugDraw.renderer.material = PathOpenedMaterial;
                }
                node.Connections.Add(west);
            }
        }

        // Diagnols
        if (includeDiagnols) {
            Vector2 northEastPos = node.GridPosition + new Vector2(1, 1);
            Vector2 northWestPos = node.GridPosition - new Vector2(1, 1);

            Vector2 southEastPos = node.GridPosition + new Vector2(1, 1);
            Vector2 southWestPos = node.GridPosition - new Vector2(1, 1);
            if (Grid.InGrid(northEastPos)) {
                if (parentPos != northEastPos &&
                    Grid.GetGridCellAtGridPosition(northEastPos).ID != 1) {

                    AStarNode north = CreateNodeAtGridSpace(northEastPos, node);
                    Debug.Log("Found northEastPos: " + north.GridPosition.ToString());
                    if (debugMode) {
                        north.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, northEastPos, new Vector3(0.0f, -0.5f, 0.0f));
                        north.debugDraw.renderer.material = PathOpenedMaterial;
                    }
                    node.Connections.Add(north);
                }
            }
            if (Grid.InGrid(northWestPos)) {
                if (parentPos != northWestPos &&
                    Grid.GetGridCellAtGridPosition(northWestPos).ID != 1) {

                    AStarNode south = CreateNodeAtGridSpace(northWestPos, node);
                    Debug.Log("Found northWestPos: " + south.GridPosition.ToString());
                    if (debugMode) {
                        south.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, northWestPos, new Vector3(0.0f, -0.5f, 0.0f));
                        south.debugDraw.renderer.material = PathOpenedMaterial;
                    }
                    node.Connections.Add(south);
                }
            }

            if (Grid.InGrid(southEastPos)) {
                if (parentPos != southEastPos &&
                    Grid.GetGridCellAtGridPosition(southEastPos).ID != 1) {

                    AStarNode east = CreateNodeAtGridSpace(southEastPos, node);
                    Debug.Log("Found southEastPos: " + east.GridPosition.ToString());
                    if (debugMode) {
                        east.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, southEastPos, new Vector3(0.0f, -0.5f, 0.0f));
                        east.debugDraw.renderer.material = PathOpenedMaterial;
                    }
                    node.Connections.Add(east);
                }
            }
            if (Grid.InGrid(southWestPos)) {
                if (parentPos != southWestPos &&
                    Grid.GetGridCellAtGridPosition(southWestPos).ID != 1) {

                    AStarNode west = CreateNodeAtGridSpace(southWestPos, node);
                    Debug.Log("Found southWestPos: " + west.GridPosition.ToString());
                    if (debugMode) {
                        west.debugDraw = Grid.InstantiateAtGridPos(AStarCubePrefab, southWestPos, new Vector3(0.0f, -0.5f, 0.0f));
                        west.debugDraw.renderer.material = PathOpenedMaterial;
                    }
                    node.Connections.Add(west);
                }
            }
        }
    }

    int CalculateManhattanDistance(Vector2 currentPos, Vector2 endPos)
    {
        return (int)(Mathf.Abs(currentPos.x - endPos.x) + Mathf.Abs(currentPos.y - endPos.y));
    }
}
