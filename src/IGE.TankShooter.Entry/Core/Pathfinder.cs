namespace IGE.TankShooter.Entry.Core;

using System;
using System.Collections.Generic;
using System.Linq;

using Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;

using QuikGraph;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.ShortestPath;

/// <summary>
/// Finds the shortest path between two points on the map.
///
/// Uses the A* algorithm from the QuikGraph library. To do this, it builds a graph from the
/// tile map, where each tile without any objects represents a vertex (at the centre of the tile)
/// and there is an edge from each vertex to its north/south/east/west neighbour.
///
/// The QuikGraph library stores vertices as an individual integer index into a 2 dimensional array.
/// Thus, there is helper functions such as PathfindingGridVertexToWorld() and WorldToPathfindingGridVertex()
/// to help convert these to world coordinates and vice-verca.
/// </summary>
public class Pathfinder
{
  private readonly TiledMap _map;
  private readonly Vector2 _mapScale;

  private AdjacencyGraph<int, Edge<int>> _pathfindingGraph;
  private NavigationPath _pathfindingResults;
  private readonly float _tileWidthWorldUnits;

  public Pathfinder(TiledMap map, float tileWidthWorldUnits)
  {
    this._map = map;
    this._tileWidthWorldUnits = tileWidthWorldUnits;
    this._mapScale = new Vector2(tileWidthWorldUnits / map.TileWidth, tileWidthWorldUnits / map.TileHeight);
  }

  public void LoadContent()
  {
    this._pathfindingGraph = BuildRectangularGraphWithDiagonals();
  }

  private Vector2 WorldToTileCoords(Vector2 worldValues)
  {
    return worldValues / _tileWidthWorldUnits;
  }

  public void Update(Vector2 target, Vector2 dest)
  {
    // Only available after calling LoadContent()
    if (_pathfindingGraph == null)
    {
      return;
    }
  
    var targetTile = WorldToTileCoords(target);
    var targetVertex = (int)targetTile.X + (int)targetTile.Y * _map.Width;
    var destTile = WorldToTileCoords(dest);
    var destVertex = (int)destTile.X + (int)destTile.Y * _map.Width;

    // No point in trying to pathfind to points which don't exist. This will always result in having to traverse
    // the entire graph to no avail, which will have a performance impact.
    // Instead, choose another point to navigate to that is close to the destination vertex.
    if (!_pathfindingGraph.ContainsVertex(destVertex))
    {
      var closestVertex = ClosestVertex(destVertex);
      Console.WriteLine($"No vertex at {destVertex}, will pathfind to the closest point {closestVertex} instead.");
      destVertex = closestVertex;
    }

    if (_pathfindingResults != null && _pathfindingResults.Matches(targetVertex, destVertex))
    {
      Console.WriteLine($"No need to perform pathfinding to {destVertex}, we are up to date.");
      return;
    }

    var edgeWeights = new Func<Edge<int>, double>(edge =>
    {
      var source = PathfindingGridVertexToWorld(edge.Source);
      var target = PathfindingGridVertexToWorld(edge.Target);
      return (source - target).Length();
    });

    // Grid-based movement including diagonals.
    // http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html#heuristics-for-grid-maps
    const int D = 1;
    double D2 = Math.Sqrt(2);
    var costHeuristic = new Func<int, double>(currentVertex =>
    {
      var currentX = currentVertex % _map.Width;
      var currentY = currentVertex / _map.Width;
      
      var destX = destVertex % _map.Width;
      var destY = destVertex / _map.Width;

     
      // The page linked above explains that the commented out formula below is the correct way to do A* on a grid with diagonal movement.
      // However, the search space still encompasses a huge array of edges, many more than is required to find the
      // shorted path in most cases. After experimenting myself with completely un-principled calculations with
      // no-mathimatical-basis-at-all, this seems to do a great job:
      return (new Vector2(currentX, currentY) - new Vector2(destX, destY)).Length() * 4;
      
      // var dy = Math.Abs(destY - currentY);
      // var dx = Math.Abs(destX - currentX);

      // return D * (dx + dy) + (D2 - 2 * D) * Math.Min(dx, dy);
    });

    var algo = new AStarShortestPathAlgorithm<int, Edge<int>>(_pathfindingGraph, edgeWeights, costHeuristic);

    // For debugging purposes, record which edges were visited as part of the AStar search.
    // Will be helpful when finding out whether it faithfully follows our heuristic to narrow the search space.
    var examinedEdges = new List<Edge<int>>();
    algo.ExamineEdge += edge =>
    {
      examinedEdges.Add(edge);
    };
      
    algo.FinishVertex += vertex =>
    {
      if (vertex == destVertex)
      {
        Console.WriteLine($"Terminating pathfinding because path to vertex {destVertex} was found.");
        algo.Abort();
      }
    };

    var predecessors = new VertexPredecessorRecorderObserver<int, Edge<int>>();
    using (predecessors.Attach(algo))
    {
      // Run the algorithm with A set to be the source
      Console.WriteLine($"Pathfinding from {targetVertex} to {destVertex}");
      algo.Compute(targetVertex);
    }

    IEnumerable<Edge<int>> result;
    if (!predecessors.TryGetPath(destVertex, out result))
    {
      _pathfindingResults = new NavigationPath(targetVertex, destVertex, null, examinedEdges);
    }
    else
    {
      _pathfindingResults = new NavigationPath(targetVertex, destVertex, result, examinedEdges);
    }
  }

  private AdjacencyGraph<int, Edge<int>> BuildRectangularGraphWithDiagonals()
  {
    var hasObjectGraph = new bool[_map.Width, _map.Height];
    for (int y = 0; y < _map.Height; y ++)
    {
      for (int x = 0; x < _map.Width; x ++)
      {
        if (_map.HasObjectAt(x, y))
        {
          hasObjectGraph[x, y] = true;
        }
      }
    }

    List<Edge<int>> pathfindingEdges = new List<Edge<int>>();
    for (int y = 0; y < _map.Height - 1; y ++)
    {
      for (int x = 0; x < _map.Width - 1; x ++)
      {
        if (hasObjectGraph[x, y])
        {
          continue;
        }

        var source = y * _map.Width + x;
            
        // Above
        if (y > 0 && !hasObjectGraph[x, y - 1])
        {
          pathfindingEdges.Add(new Edge<int>(source, (y - 1) * _map.Width + x));
        }

        // Above-right
        if (y > 0 && !hasObjectGraph[x + 1, y - 1])
        {
          pathfindingEdges.Add(new Edge<int>(source, (y - 1) * _map.Width + x + 1));
        }
         
        // Above-left
        if (y > 0 && x > 0 && !hasObjectGraph[x - 1, y - 1])
        {
          pathfindingEdges.Add(new Edge<int>(source, (y - 1) * _map.Width + x - 1));
        }
          
        // Right
        if (!hasObjectGraph[x + 1, y])
        {
          pathfindingEdges.Add(new Edge<int>(source, y * _map.Width + x + 1));
        }
          
        // Below
        if (!hasObjectGraph[x, y + 1])
        {
          pathfindingEdges.Add(new Edge<int>(source, (y + 1) * _map.Width + x));
        }

        // Below-right
        if (!hasObjectGraph[x + 1, y + 1])
        {
          pathfindingEdges.Add(new Edge<int>(source, (y + 1) * _map.Width + x + 1));
        }
         
        // Below-left
        if (x > 0 && !hasObjectGraph[x - 1, y + 1])
        {
          pathfindingEdges.Add(new Edge<int>(source, (y + 1) * _map.Width + x - 1));
        }
          
        // Left
        if (x > 0 && !hasObjectGraph[x - 1, y])
        {
          pathfindingEdges.Add(new Edge<int>(source, y * _map.Width + x - 1));
        }
      }
    }
    
    return pathfindingEdges.ToAdjacencyGraph<int, Edge<int>>();
  }

  private Vector2 PathfindingGridVertexToWorld(int vertex)
  {
    var vertexX = vertex % _map.Width;
    var vertexY = vertex / _map.Width;
    
    // Add half a tile size to centre the vertex in a tile, rather than use the top left, which
    // would be kind of arbitrary (some tiles top lefts map be good for navigating, others may not).
    return new Vector2(
      (vertexX * _map.TileWidth + _map.TileWidth / 2f) * _mapScale.X,
      (vertexY * _map.TileHeight + _map.TileHeight / 2f) * _mapScale.Y
    );
  }
  
  private int ClosestVertex(int vertex)
  {
    var vertexX = vertex % _map.Width;
    var vertexY = vertex / _map.Width;
    
    return _pathfindingGraph.Vertices.MinBy(v =>
    {
      var x = v % _map.Width;
      var y = v / _map.Width;
      return Math.Sqrt(
        (x - vertexX) * (x - vertexX) +
        (y - vertexY) * (y - vertexY)
      );
    });
  }

  public void Draw(SpriteBatch spriteBatch)
  {
    if (_pathfindingGraph == null || !Debug.DrawDebugLines)
    {
      return;
    }

    foreach (var vertexIdx in _pathfindingGraph.Vertices)
    {
      var vertex = PathfindingGridVertexToWorld(vertexIdx);
      spriteBatch.DrawCircle(
        vertex.X, 
        vertex.Y, 
        0.2f, 
        10, 
        Color.Black,
        0.1f
      );
    }

    foreach (var edge in _pathfindingGraph.Edges)
    {
      var source = PathfindingGridVertexToWorld(edge.Source);
      var target = PathfindingGridVertexToWorld(edge.Target);
      
      spriteBatch.DrawLine(
        source.X,
        source.Y,
        target.X,
        target.Y,
        Color.Black,
        0.1f
      );
    }
    
    if (_pathfindingResults != null )
    {
      foreach (var edge in _pathfindingResults.ExaminedEdges) {
        var source = PathfindingGridVertexToWorld(edge.Source);
        var target = PathfindingGridVertexToWorld(edge.Target);
        
        spriteBatch.DrawLine(
        source.X,
        source.Y,
        target.X,
        target.Y,
          Color.LightGray,
          0.1f
        );
      }
      
      if (_pathfindingResults.Path != null)
      {
        foreach (var edge in _pathfindingResults.Path) {
          var source = PathfindingGridVertexToWorld(edge.Source);
          var target = PathfindingGridVertexToWorld(edge.Target);
          
          spriteBatch.DrawLine(
          source.X,
          source.Y,
          target.X,
          target.Y,
            Color.White,
            0.3f
          );
        }
      }
    }
  }
}

public class NavigationPath
{
  
  private readonly int _sourceVertex;
  private readonly int _destVertex;
  private readonly IEnumerable<Edge<int>> _path;
  private readonly IEnumerable<Edge<int>> _examinedEdges;

  public NavigationPath(int sourceVertex, int destVertex, IEnumerable<Edge<int>> path, IEnumerable<Edge<int>> examinedEdges)
  {
    this._sourceVertex = sourceVertex;
    this._destVertex = destVertex;
    this._path = path;
    this._examinedEdges = examinedEdges;
  }

  public IEnumerable<Edge<int>> Path => _path;

  public IEnumerable<Edge<int>> ExaminedEdges => _examinedEdges;

  public bool Matches(int sourceVertex, int destVertex)
  {
    if (this._sourceVertex == sourceVertex && this._destVertex == destVertex)
    {
      return true;
    }

    if (_path == null)
    {
      return false;
    }

    var sourceInPath = false;
    var destInPath = false;
    foreach (var edge in _path)
    {
      sourceInPath |= (edge.Source == sourceVertex);
      destInPath |= (edge.Target == destVertex);
      if (sourceInPath && destInPath)
      {
        return true;
      }
    }

    Console.WriteLine(
      $"Path from {_sourceVertex} to {_destVertex} does not match request path fro {sourceVertex} to {destVertex}");
    return false;
  }

  public bool AlmostMatches(int sourceVertex, int destVertex, int gridCellsApart)
  {
    throw new Exception("Not implemented");
  }
  
}
