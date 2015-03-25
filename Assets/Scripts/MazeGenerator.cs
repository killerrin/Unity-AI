using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour {
	private System.Random random;

	public List<List<GridCell>> Grid;
	public Vector2 Dimensions;
	public GameObject CellPrefab;

	public int randomMazeValue = 43;

	private Vector3 m_min;
	private Vector3 m_max;

	private bool setup = false;

	// Use this for initialization
	void Start () {
		GenerateGrid();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void GenerateGrid()
	{
		if (Dimensions.x == 0 || Dimensions.y == 0) return;
		random = new System.Random();

		m_min = new Vector3(
			Dimensions.x + transform.position.x,
			0.0f,
			Dimensions.y + transform.position.z
			);

		m_max = new Vector3(
			(Dimensions.x * CellPrefab.transform.localScale.x) + transform.position.x,
			0.0f,
			(Dimensions.y * CellPrefab.transform.localScale.z) + transform.position.z
			);

		Grid = new List<List<GridCell>>() { };
		for (int x = 0; x < Dimensions.x; x++)
		{
			List<GridCell> gridRow = new List<GridCell>() { };
			for (int y = 0; y < Dimensions.y; y++)
			{
				// ToDo: MazeGeneration Code Here
				if (x == 0 ||
					y == 0 ||
					x == (Dimensions.x - 1) ||
					y == (Dimensions.y - 1)) 
				{
					gridRow.Add(new GridCell(1));
				}
				else 
				{
					int i = random.Next(0, 50);
					if (i < randomMazeValue)
						gridRow.Add(new GridCell(0));
					else
						gridRow.Add(new GridCell(1));
				}

			}
			Grid.Add(gridRow);
		}

		setup = true;
		InstantiateMazePieces();
	}

	void InstantiateMazePieces()
	{
		for (int x = 0; x < Grid.Count; x++)
		{
			for (int y = 0; y < Grid[x].Count; y++)
			{
				//Debug.Log("" + x + ", " + y);
				if (Grid[x][y].ID == 1) 
				{
					InstantiateAtGridPos(CellPrefab, new Vector2(x, y));
				}
			}
		}
	}

	public GameObject InstantiateAtGridPos(GameObject gameObjectPrefab, Vector2 gridPos, Vector3 offset = new Vector3())
	{
		Vector3 cellPosition = new Vector3(gridPos.x, 0.0f, gridPos.y);
		if (gridPos.x != 0) { cellPosition.x *= gameObjectPrefab.transform.localScale.x; }
		if (gridPos.y != 0) { cellPosition.z *= gameObjectPrefab.transform.localScale.z; }
		cellPosition.y *= gameObjectPrefab.transform.localScale.y;
		cellPosition += offset;
		cellPosition += transform.position;

		GameObject temp = (GameObject)Instantiate(gameObjectPrefab, cellPosition, gameObjectPrefab.transform.rotation);
		return temp;
	}

	public GridCell GetGridCellAtPosition(Vector3 position)
	{
		if (!InGrid(position) || !setup) { return new GridCell(1); }

		for (int x = 0; x < Grid.Count; x++)
		{
			for (int y = 0; y < Grid[x].Count; y++)
			{
				Vector3 cellPosition = new Vector3(x, 0.0f, y);
				if (x != 0) { cellPosition.x *= CellPrefab.transform.localScale.x; }
				if (y != 0) { cellPosition.z *= CellPrefab.transform.localScale.z; }
				cellPosition.y *= CellPrefab.transform.localScale.y;
				cellPosition += transform.position;

				Vector3 nextCellPosition = new Vector3(x+1, 0.0f, y+1);
				if (x != 0) { nextCellPosition.x *= CellPrefab.transform.localScale.x; }
				if (y != 0) { nextCellPosition.z *= CellPrefab.transform.localScale.z; }
				nextCellPosition.y *= CellPrefab.transform.localScale.y;
				nextCellPosition += transform.position;

				if (position.x > cellPosition.x &&
					position.x < nextCellPosition.x)
				{
					if (position.z > cellPosition.z &&
						position.z < nextCellPosition.z)
					{
						return Grid[x][y];
					}
				}
			}
		}

		return new GridCell(1);
	}

	public GridCell GetGridCellAtGridPosition(Vector2 gridPos)
	{
		if (!InGrid(gridPos)) return new GridCell(1);
		return Grid[(int)gridPos.x][(int)gridPos.y];
	}

	public Vector2 GetRandomGridCell(int id = 0)
	{
		while (true) {
			Vector2 gridSpace = new Vector2(random.Next(0, (int)Dimensions.x),
											random.Next(0, (int)Dimensions.y));
			GridCell temp = Grid[(int)gridSpace.x][(int)gridSpace.y];
			if (temp.ID == id) return gridSpace;
		}
	}

	public bool InGrid(Vector3 position)
	{
		if (!setup) return false;

		if (position.x > m_min.x &&
			position.z > m_min.z)
		{
			if (position.x < m_max.x &&
				position.z < m_max.z)
			{
				return true;
			}
		}

		return false;
	}

	public bool InGrid(Vector2 gridPos)
	{
		if (!setup) return false;

		if (gridPos.x > 0 &&
			gridPos.x < Grid.Count)
		{
			if (gridPos.y > 0 &&
				gridPos.y < Grid[0].Count)
			{
				return true;
			}
		}

		return false;
	}
}
