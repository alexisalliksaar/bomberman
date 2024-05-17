using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

/*[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(BombDropper))]*/
public class AIController : MonoBehaviour
{
    public float AttackWeight = 50;
    public float WallWeight = 50;
    public float PowerUpWeight = 0;
    public float WaitTime = 0.5f;


    private float _waitTimer;
    private Vector2? _moveTarget;
    private State? _activeState;
    private Movement _movement;
    private BombDropper _bombDropper;
    private Stack<Vector2Int> _pathIndexes = new Stack<Vector2Int>();
    void Start()
    {
        _movement = GetComponent<Movement>();
        _bombDropper = GetComponent<BombDropper>();
        _activeState = null;
        GameController.Players.Add(_bombDropper);
        GameController.ControllerScripts.Add(this);
    }

    
    void Update()
    {
        if (_activeState == State.Wait)
        {
            if (_waitTimer < Time.time)
            {
                ChooseState();
            }
            else
            {
                return;
            }
        }
        if (_activeState == null || _moveTarget == null)
        {
            ChooseState();
        }

        if (_moveTarget != null)
        {
            if (_activeState != State.Attack)
            {
                if (Vector2.Distance(_moveTarget.Value, transform.position) > 0.1)
                    _movement.MoveTowards(_moveTarget.Value);
                else
                {
                    SetMoveTarget();
                    if (_moveTarget == null)
                    {
                        if (_activeState == State.BlockMining)
                        {
                            _bombDropper.DropBomb();
                        }
                        _activeState = null;
                    }
                }
            }
            else if (_activeState == State.Attack)
            {
                if (Vector2.Distance(_moveTarget.Value, transform.position) > 0.5)
                    _movement.MoveTowards(_moveTarget.Value);
                else
                {
                    SetMoveTarget();
                    if (_moveTarget == null)
                    {
                        _bombDropper.DropBomb();
                        _activeState = null;
                    }
                }
            }
        }
        

        
    }

    private void ChooseState()
    {
        Vector2Int? dest = null;
        Vector2Int pos = GameGrid.Instance.IndexesFromWorldPos(transform.position);
        if (GameGrid.Instance.gameGrid[pos.x][pos.y].Danger)
        {
            _activeState = State.BombEvasion;
            dest = NearestStateTarget(State.BombEvasion, false);
        }
        else
        {
            List<Tuple<State, float>> states = new List<Tuple<State, float>>()
            {
                new Tuple<State, float>(State.BlockMining, WallWeight),
                new Tuple<State, float>(State.Attack, AttackWeight),
                new Tuple<State, float>(State.PowerUp, PowerUpWeight)
            };
            State[] stateOrder = new State[3];
            for (int i = 0; i < stateOrder.Length; i++)
            {
                float maxSize = states.Select(t => t.Item2).Sum();
                float random = Random.Range(0.0f, maxSize);
                float curr = 0f;
                for (int j = 0; j < states.Count; j++)
                {
                    curr += states[j].Item2;
                    if (curr >= random)
                    {
                        stateOrder[i] = states[j].Item1;
                        states.RemoveAt(j);
                        break;
                    }
                }
            }
            
            for (int i = 0; i < stateOrder.Length; i++)
            {
                State selectedState = stateOrder[i];
                if (selectedState == State.BlockMining)
                {
                    _activeState = State.BlockMining;
                    dest = NearestStateTarget(State.BlockMining, true);
                }
                else if (selectedState == State.Attack)
                {
                    _activeState = State.Attack;
                    dest = NearestStateTarget(State.Attack, true);
                }
                else
                {
                    _activeState = State.PowerUp;
                    dest = NearestStateTarget(State.PowerUp, true);
                }
                if (dest != null) break;
            }
        }
        if (dest != null)
        {
            _pathIndexes = ShortestPathTo(dest.Value);
        }
        else
        {
            _activeState = State.Wait;
            _waitTimer = Time.time + WaitTime + Random.Range(0.0f, WaitTime);
        }
        SetMoveTarget();
        
    }

    private void SetMoveTarget()
    {
        if (_pathIndexes.Count == 0)
        {
            _moveTarget = null;
            return;
        }
        _moveTarget = GameGrid.Instance.WorldPosFromIndexes(_pathIndexes.Pop());
    }
    
    
    private Vector2Int? NearestStateTarget(State state, bool denyDanger)
    {
        List<Vector2Int> enemyPositions = null;
        if (state == State.Attack)
        {
            enemyPositions = GameController.Players
                .Where(el => el.TeamData.TeamColor != _bombDropper.TeamData.TeamColor)
                .Select(el => GameGrid.Instance.IndexesFromWorldPos(el.transform.position))
                .ToList();
        }
        
        Vector2Int pos = GameGrid.Instance.IndexesFromWorldPos(transform.position);
        GameGrid.Node root = GameGrid.Instance.gameGraph[pos.x][pos.y];
        HashSet<GameGrid.Node> visited = new HashSet<GameGrid.Node>();

        HashSet<GameGrid.Node> previousLevel = null;
        
        while (true)
        {
            HashSet<GameGrid.Node> currLevel = new HashSet<GameGrid.Node>();
            if (previousLevel == null)
            {
                currLevel.Add(root);
                visited.Add(root);
            } else {
                foreach (GameGrid.Node node in previousLevel)
                {
                    List<GameGrid.Node> unmet = node.Neighbours.Where(el => !visited.Contains(el)).ToList();
                    if (denyDanger)
                    {
                        unmet = unmet.Where(el => el.Value.Danger == false).ToList();
                    }
                    currLevel.UnionWith(unmet);
                    visited.UnionWith(unmet);
                }
            }
            previousLevel = currLevel;
            
            if (currLevel.Count == 0)
                return null;
            if (state == State.BlockMining)
            {
                if (visited.Count == 1) continue;
                foreach (GameGrid.Node node in currLevel)
                {
                    if (node.WallCount > 0)
                    {
                        return node.Value.IPos;
                    }
                }
            }
            else if (state == State.BombEvasion)
            {
                foreach (GameGrid.Node node in currLevel)
                {
                    if (node.Value.Danger == false)
                    {
                        return node.Value.IPos;
                    }
                }
            }
            else if (state == State.Attack)
            {
                if (visited.Count == 1) continue;
                foreach (GameGrid.Node node in currLevel)
                {
                    if (enemyPositions.Contains(node.Value.IPos))
                    {
                        return node.Value.IPos;
                    }
                }
            }
            else if (state == State.PowerUp)
            {
                foreach (GameGrid.Node node in currLevel)
                {
                    if (node.Value.HasPowerUp)
                    {
                        return node.Value.IPos;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
    
    private Stack<Vector2Int> ShortestPathTo(Vector2Int dest)
    {
        Vector2Int pos = GameGrid.Instance.IndexesFromWorldPos(transform.position);
        Stack<Vector2Int> res = new Stack<Vector2Int>();
        res.Push(dest);

        if (pos.Equals(dest))
        {
            return res;
        }
        GameGrid.Node[][] gameGraph = GameGrid.Instance.gameGraph;

        HashSet<GameGrid.Node> visited = new HashSet<GameGrid.Node>();
        List<HashSet<GameGrid.Node>> visitOrder = new List<HashSet<GameGrid.Node>>();

        HashSet<GameGrid.Node> previousLevel = new HashSet<GameGrid.Node>();
        previousLevel.Add(gameGraph[pos.x][pos.y]);
        visitOrder.Add(previousLevel);
        visited.Add(gameGraph[pos.x][pos.y]);
        while (true)
        {
            HashSet<GameGrid.Node> currLevel = new HashSet<GameGrid.Node>();
            foreach (GameGrid.Node node in previousLevel)
            {
                List<GameGrid.Node> unmet = node.Neighbours.Where(el => !visited.Contains(el)).ToList();
                currLevel.UnionWith(unmet);
                visited.UnionWith(unmet);
            }
            visitOrder.Add(currLevel);
            if (currLevel.Count == 0)
                return null;
            if (currLevel.Where(node => node.Value.IPos.Equals(dest)).ToList().Count > 0)
                break;
            previousLevel = currLevel;
        }

        for (int i = visitOrder.Count - 2; i >= 0; i--)
        {
            foreach (GameGrid.Node node in visitOrder[i])
            {
                if (node.Neighbours.Select(el => el.Value.IPos).Contains(res.Peek()))
                {
                    res.Push(node.Value.IPos);
                    break;
                }
            }
        }

        return res;
    }
    

    private enum State

    {
        BombEvasion = 0,
        BlockMining = 1,
        Attack = 2,
        PowerUp = 3,
        Wait = 4
    }
}
