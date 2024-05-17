using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameGrid : MonoBehaviour
{
    public int columns = 13;
    public int rows = 11;
    
    public static GameGrid Instance;
    public GameBlock[][] gameGrid;
    public Node[][] gameGraph;
    public Vector3 GridSize;

    private void Awake()
    {
        Instance = this;
        gameGrid = new GameBlock[rows][];
        gameGraph = new Node[rows][];
        for (int i = 0; i < rows; i++)
        {
            gameGrid[i] = new GameBlock[columns];
            gameGraph[i] = new Node[columns];
            for (int j = 0; j < columns; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                if (i % 2 == 1 && j % 2 == 1)
                    gameGrid[i][j] = new GameBlock(GameBlockMaterial.Unpassable, pos);
                else if ((i == 0 || i == rows - 1) && (j < 2 || j >= columns - 2))
                    gameGrid[i][j] = new GameBlock(GameBlockMaterial.Empty, pos);
                else if ((i == 1 || i == rows - 2) && (j == 0 || j == columns - 1))
                    gameGrid[i][j] = new GameBlock(GameBlockMaterial.Empty, pos);
                else
                    gameGrid[i][j] = new GameBlock(GameBlockMaterial.Filled, pos);
            }
        }

        PutNode(0, 0);
        PutNode(0, columns-1);
        PutNode(rows-1, 0);
        PutNode(rows-1, columns-1);

        Grid gridObject = GetComponent<Grid>();
        GridSize = gridObject.cellSize;
    }

    private Node PutNode(int i, int j)
    {
        Node node = new Node();
        node.Value = gameGrid[i][j];
        gameGraph[i][j] = node;
        Vector2Int[] neighbours = new[]
        {
            new Vector2Int(i - 1, j), new Vector2Int(i + 1, j),
            new Vector2Int(i, j + 1), new Vector2Int(i, j - 1)
        };
        for (int k = 0; k < neighbours.Length; k++)
        {
            int i1 = neighbours[k].x;
            int j1 = neighbours[k].y;
            if (0 <= i1 && i1 < rows && 0 <= j1 && j1 < columns)
            {
                if (IsEmpty(i1, j1))
                {
                    if (gameGraph[i1][j1] == null)
                        node.Neighbours.Add(PutNode(i1, j1));
                    else
                        node.Neighbours.Add(gameGraph[i1][j1]);
                }else if (IsFilled(i1, j1))
                {
                    node.WallCount++;
                }
            }
        }

        return node;
    }

    private Node PutNodeUpdateNeighbours(int i, int j)
    {
        Node node = PutNode(i, j);
        UpdateNeighbours(node, i, j, true);

        return node;
    }

    private void UpdateNeighbours(Node node, int i, int j, bool add)
    {
        Vector2Int[] neighbours = new[]
        {
            new Vector2Int(i - 1, j), new Vector2Int(i + 1, j),
            new Vector2Int(i, j + 1), new Vector2Int(i, j - 1)
        };
        for (int k = 0; k < neighbours.Length; k++)
        {
            int i1 = neighbours[k].x;
            int j1 = neighbours[k].y;
            if (0 <= i1 && i1 < rows && 0 <= j1 && j1 < columns && gameGraph[i1][j1] != null)
            {
                if (add)
                {
                    gameGraph[i1][j1].WallCount--;
                    gameGraph[i1][j1].Neighbours.Add(node);
                }
                else
                {
                    if (node.Value.Material == GameBlockMaterial.Filled)
                        gameGraph[i1][j1].WallCount--;
                    gameGraph[i1][j1].Neighbours.Remove(node);
                }
            }
        }
    }

    private void SetMaterial(GameBlock block, GameBlockMaterial material)
    {
        if (material == GameBlockMaterial.Empty)
        {
            PutNodeUpdateNeighbours(block.IPos.x, block.IPos.y);
            block.Material = GameBlockMaterial.Empty;
        } else if (material == GameBlockMaterial.Unpassable)
        {
            Node node;
            if (gameGraph[block.IPos.x][block.IPos.y] == null)
            {
                node = new Node();
                node.Value = block;
            }
            else
            {
                node = gameGraph[block.IPos.x][block.IPos.y];
            }
            UpdateNeighbours(node, block.IPos.x, block.IPos.y, false);
            gameGraph[block.IPos.x][block.IPos.y] = null;

            block.Material = GameBlockMaterial.Unpassable;
        }
    }


    private bool IsEmpty(int i, int j)
    {
        return gameGrid[i][j].Material == GameBlockMaterial.Empty;
    }
    private bool IsFilled(int i, int j)
    {
        return gameGrid[i][j].Material == GameBlockMaterial.Filled;
    }

    private void Update()
    {
        //Debug.Log(GridToString());
    }

    private string GridToString()
    {
        string res = "";
        for (int i = 0; i < gameGrid.Length; i++)
        {
            res += "[";
            for (int j = 0; j < gameGrid[0].Length; j++)
            {
                res += gameGrid[i][j].Danger;
                res += ", ";
            }

            res += "]\n";
        }
        return res;
    }
    
    public static Vector3 CenteredInCell(Vector3 givenPos)
    {
        Vector3 gridSize = GameGrid.Instance.GridSize;
        float origX = givenPos.x;
        float origY = givenPos.y;

        float newX = (float)(Math.Round(origX / gridSize.x) * gridSize.x);
        float newY = (float)(Math.Round(origY / gridSize.y) * gridSize.y);
        
        return new Vector3(newX, newY, givenPos.z);
    }

    public Vector2Int IndexesFromWorldPos(Vector3 pos)
    {
        return new Vector2Int((int)Math.Round(-pos.y, 0), (int)Math.Round(pos.x, 0));
    }
    public Vector3 WorldPosFromIndexes(Vector2Int pos)
    {
        return new Vector3(pos.y, -pos.x);
    }

    public BombExplosionTiles ExplodeBomb(Bomb bomb)
    {
        BombExplosionTiles res = GetBombExplosionTiles(bomb);
        foreach (Vector2Int tilesIndex in res.RemovedTilesIndexes)
        {
            SetMaterial(gameGrid[tilesIndex.x][tilesIndex.y], GameBlockMaterial.Empty);
            res.RemovedTiles.Add(WorldPosFromIndexes(tilesIndex));
        }

        return res;
    }

    public void PlaceBombDanger(Bomb bomb)
    {
        BombExplosionTiles explosionTiles = GetBombExplosionTiles(bomb);
        foreach (Vector2Int tilesIndex in explosionTiles.BlastTilesIndexes)
        {
            gameGrid[tilesIndex.x][tilesIndex.y].Danger = true;
        }
    }
    public void RemoveBombDanger(BombExplosionTiles bombExplosionTiles)
    {
        foreach (Vector2Int tilesIndex in bombExplosionTiles.BlastTilesIndexes)
        {
            gameGrid[tilesIndex.x][tilesIndex.y].Danger = false;
        }
    }

    /**
     * Doesn't change GameGrid, just queries bomb data at moment in time
     */
    public BombExplosionTiles GetBombExplosionTiles(Bomb bomb)
    {
        Vector2Int bombIndexes = IndexesFromWorldPos(bomb.transform.position);
        int bombRowI = bombIndexes.x;
        int bombColumnI = bombIndexes.y;
        BombExplosionTiles res = new BombExplosionTiles();
        res.BlastTilesIndexes.Add(new Vector2Int(bombRowI, bombColumnI));
        
        int startIndex, currIndex, endIndex;
        
        // Down
        startIndex = bombRowI;
        endIndex = bombRowI + bomb.LaserRange;
        for (currIndex = startIndex + 1;; currIndex++)
        {
            if (currIndex >= rows || gameGrid[currIndex][bombColumnI].Material == GameBlockMaterial.Unpassable || currIndex > endIndex)
            {
                currIndex--;
                break;
            }
            
            res.BlastTilesIndexes.Add(new Vector2Int(currIndex, bombColumnI));
            
            if (gameGrid[currIndex][bombColumnI].Material == GameBlockMaterial.Filled)
            {
                res.RemovedTilesIndexes.Add(new Vector2Int(currIndex, bombColumnI));
                break;
            }
        }
        res.Down = currIndex - startIndex;
        
        // Up
        endIndex = bombRowI - bomb.LaserRange;
        for (currIndex = startIndex - 1;; currIndex--)
        {
            if (currIndex < 0 || gameGrid[currIndex][bombColumnI].Material == GameBlockMaterial.Unpassable || currIndex < endIndex)
            {
                currIndex++;
                break;
            }
            res.BlastTilesIndexes.Add(new Vector2Int(currIndex, bombColumnI));
            if (gameGrid[currIndex][bombColumnI].Material == GameBlockMaterial.Filled)
            {
                res.RemovedTilesIndexes.Add(new Vector2Int(currIndex, bombColumnI));
                break;
            }
        }
        res.Top = startIndex - currIndex;
        
        // Left
        startIndex = bombColumnI;
        endIndex = bombColumnI - bomb.LaserRange;
        for (currIndex = startIndex - 1;; currIndex--)
        {
            if (currIndex < 0 || gameGrid[bombRowI][currIndex].Material == GameBlockMaterial.Unpassable ||  currIndex < endIndex)
            {
                currIndex++;
                break;
            }
            res.BlastTilesIndexes.Add(new Vector2Int(bombRowI, currIndex));
            if (gameGrid[bombRowI][currIndex].Material == GameBlockMaterial.Filled)
            {
                res.RemovedTilesIndexes.Add(new Vector2Int(bombRowI, currIndex));
                break;
            }
        }
        res.Left = startIndex - currIndex;
        
        //Right
        endIndex = bombColumnI + bomb.LaserRange;
        for (currIndex = startIndex + 1;; currIndex++)
        {
            if (currIndex >= columns || gameGrid[bombRowI][currIndex].Material == GameBlockMaterial.Unpassable || currIndex > endIndex)
            {
                currIndex--;
                break;
            }             
            res.BlastTilesIndexes.Add(new Vector2Int(bombRowI, currIndex));
            if (gameGrid[bombRowI][currIndex].Material == GameBlockMaterial.Filled)
            {
                res.RemovedTilesIndexes.Add(new Vector2Int(bombRowI, currIndex));
                break;
            }
        }
        res.Right = currIndex - startIndex;

        return res;
    }

    public class GameBlock
    {
        public bool Danger;
        public GameBlockMaterial Material;
        public Vector2Int IPos;
        public bool HasPowerUp;

        public GameBlock(GameBlockMaterial material, Vector2Int pos)
        {
            this.Material = material;
            IPos = pos;
            this.Danger = false;
            HasPowerUp = false;
        }
    }
    
    
    public enum GameBlockMaterial
    {
        Empty = 0,
        Unpassable = 1,
        Filled = 2
    }
    public class BombExplosionTiles
    {
        public readonly List<Vector3> RemovedTiles = new List<Vector3>();
        public readonly List<Vector2Int> RemovedTilesIndexes = new List<Vector2Int>();
        public readonly List<Vector2Int> BlastTilesIndexes = new List<Vector2Int>();
        public int Top;
        public int Left;
        public int Right;
        public int Down;

        public BombExplosionTiles()
        {
        }
    }
    public class Node
    {
        public GameBlock Value;
        public List<Node> Neighbours = new List<Node>();
        public int WallCount = 0;
    }
}
