using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathFinder
{
    private LevelData levelData;
    private int[,] walkableGrid;

    public PathFinder(LevelData level)
    {
        levelData = level;
        InitializeWalkableGrid();
    }

    private void InitializeWalkableGrid()
    {
        walkableGrid = new int[levelData.width, levelData.height];

        // 0, 3 и 4 - проходимые, остальное - стены
        for (int x = 0; x < levelData.width; x++)
        {
            for (int y = 0; y < levelData.height; y++)
            {
                walkableGrid[x, y] = (levelData.grid[x, y] == 0 || levelData.grid[x, y] == 3 || levelData.grid[x, y] == 4 || levelData.grid[x, y] == 7) ? 0 : 1;
            }
        }
    }

    public bool IsWalkable(Vector2Int position)
    {
        if (position.x < 0 || position.x >= levelData.width ||
            position.y < 0 || position.y >= levelData.height)
            return false;

        return walkableGrid[position.x, position.y] == 0;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, List<Vector2Int> obstacles)
    {
        // Создаем временную сетку с учетом всех препятствий
        int[,] tempGrid = (int[,])walkableGrid.Clone();

        // Добавляем все препятствия
        foreach (var obstacle in obstacles)
        {
            if (obstacle.x >= 0 && obstacle.x < levelData.width &&
                obstacle.y >= 0 && obstacle.y < levelData.height)
            {
                tempGrid[obstacle.x, obstacle.y] = 1;
            }
        }

        // Остальная часть метода остается без изменений
        var path = new List<Vector2Int>();
        var openSet = new PriorityQueue();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, target);
        openSet.Enqueue(new Node(start, fScore[start]));

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue().Position;

            if (current == target)
            {
                path = ReconstructPath(cameFrom, current);
                return path;
            }

            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                if (neighbor.x < 0 || neighbor.x >= levelData.width ||
                    neighbor.y < 0 || neighbor.y >= levelData.height ||
                    tempGrid[neighbor.x, neighbor.y] == 1)
                {
                    closedSet.Add(neighbor);
                    continue;
                }

                float tentativeGScore = gScore.ContainsKey(current) ? gScore[current] + 1 : float.MaxValue;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, target);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(new Node(neighbor, fScore[neighbor]));
                    }
                }
            }
        }

        return null;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int>();
        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        // Удаляем стартовую позицию (мы уже там)
        if (path.Count > 0)
            path.RemoveAt(0);

        return path;
    }

    private float HeuristicCostEstimate(Vector2Int a, Vector2Int b)
    {
        // Манхэттенское расстояние
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        return new List<Vector2Int>
    {
        new Vector2Int(position.x, position.y + 1),    // Вверх
        new Vector2Int(position.x + 1, position.y),    // Вправо
        new Vector2Int(position.x, position.y - 1),    // Вниз
        new Vector2Int(position.x - 1, position.y)     // Влево
    };
    }

    private class Node
    {
        public Vector2Int Position { get; }
        public float Priority { get; }

        public Node(Vector2Int position, float priority)
        {
            Position = position;
            Priority = priority;
        }
    }

    private class PriorityQueue
    {
        private List<Node> elements = new List<Node>();

        public int Count => elements.Count;

        public void Enqueue(Node item)
        {
            elements.Add(item);
            // Сортировка по приоритету, затем по координатам
            elements.Sort((a, b) =>
            {
                int priorityCompare = a.Priority.CompareTo(b.Priority);
                if (priorityCompare != 0) return priorityCompare;

                // При одинаковом приоритете сортируем по X и Y
                int xCompare = a.Position.x.CompareTo(b.Position.x);
                return xCompare != 0 ? xCompare : a.Position.y.CompareTo(b.Position.y);
            });
        }

        public Node Dequeue()
        {
            var item = elements[0];
            elements.RemoveAt(0);
            return item;
        }

        public bool Contains(Vector2Int position)
        {
            return elements.Any(node => node.Position == position);
        }
    }
}