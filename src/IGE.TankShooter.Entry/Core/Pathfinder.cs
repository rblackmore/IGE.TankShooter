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

  public TiledMap Map => _map;

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

  public int TileCoordsToVertex(Vector2 worldValues)
  {
    return (int)worldValues.X + (int)worldValues.Y * _map.Width;
  }

  public Vector2 WorldToTileCoords(Vector2 worldValues)
  {
    return worldValues / _tileWidthWorldUnits;
  }

  public NavigationPath FindPath(Vector2 target, Vector2 dest)
  {
    // Only available after calling LoadContent()
    if (_pathfindingGraph == null)
    {
      return null;
    }
  
    var targetTile = WorldToTileCoords(target);
    var targetVertex = TileCoordsToVertex(targetTile);
    var destTile = WorldToTileCoords(dest);
    var destVertex = TileCoordsToVertex(destTile);

    // No point in trying to pathfind to points which don't exist. This will always result in having to traverse
    // the entire graph to no avail, which will have a performance impact.
    // Instead, choose another point to navigate to that is close to the destination vertex.
    if (!_pathfindingGraph.ContainsVertex(destVertex))
    {
      var closestVertex = ClosestVertex(destVertex);
      Console.WriteLine($"No vertex at {destVertex}, will pathfind to the closest point {closestVertex} instead.");
      destVertex = closestVertex;
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
    algo.ExamineEdge += edge => examinedEdges.Add(edge);
      
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
      Console.WriteLine($"Pathfinding from {targetVertex} to {destVertex}");
      algo.Compute(targetVertex);
    }

    IEnumerable<Edge<int>> result;
    if (!predecessors.TryGetPath(destVertex, out result))
    {
      return new NavigationPath(this, targetVertex, destVertex, null, examinedEdges);
    }
    else
    {
      return new NavigationPath(this, targetVertex, destVertex, result, examinedEdges);
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

  public Vector2 PathfindingGridVertexToWorld(int vertex)
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
  
  public int ClosestVertex(int vertex)
  {
    if (_pathfindingGraph.ContainsVertex(vertex))
    {
      return vertex;
    }

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
    if (_pathfindingGraph == null || !Debug.DrawDebugLines.Pathfinding.Grid)
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
  }
}

public class NavigationPath
{

  private readonly Pathfinder _pathfinder;  
  private readonly int _sourceVertex;
  private readonly int _destVertex;
  private readonly IEnumerable<Edge<int>> _path;
  private readonly IEnumerable<Edge<int>> _examinedEdges;

  public NavigationPath(Pathfinder pathfinder, int sourceVertex, int destVertex, IEnumerable<Edge<int>> path, IEnumerable<Edge<int>> examinedEdges)
  {
    this._pathfinder = pathfinder;
    this._sourceVertex = sourceVertex;
    this._destVertex = destVertex;
    this._path = path;
    this._examinedEdges = examinedEdges;
  }

  public IEnumerable<Edge<int>> Path => _path;

  public IEnumerable<Edge<int>> ExaminedEdges => _examinedEdges;
  
  public bool Matches(Vector2 source, Vector2 dest)
  {
    var sourceTileCoords = _pathfinder.WorldToTileCoords(source);
    var sourceVertex = _pathfinder.TileCoordsToVertex(sourceTileCoords);
    
    var destTileCoords = _pathfinder.WorldToTileCoords(dest);
    var destVertex = _pathfinder.TileCoordsToVertex(destTileCoords);

    return Matches(sourceVertex, destVertex);
  }

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
      sourceInPath |= edge.Source == sourceVertex || edge.Target == sourceVertex;
      destInPath |= edge.Source == destVertex || edge.Target == destVertex;

      if (sourceInPath && destInPath)
      {
        break;
      }
    }

    if (sourceInPath && destInPath)
    {
      return true;
    }

    Console.WriteLine(
      $"Path from {_sourceVertex} to {_destVertex} does not match request path from {sourceVertex} to {destVertex}");
    return false;
  }

  public Vector2? NextPosition(Vector2 currentPosition)
  {
    if (_path == null)
    {
      return null;
    }

    var currentTileCoords = _pathfinder.WorldToTileCoords(currentPosition);
    var currentVertex = _pathfinder.TileCoordsToVertex(currentTileCoords);
    var nextVertex = ClosestSourceVertex(currentVertex);

    return _pathfinder.PathfindingGridVertexToWorld(nextVertex);
  }

  private int ClosestSourceVertex(int currentVertex)
  {
    // This is a cheaper search then the next one which requires math to
    // figure out the closest. If we are exactly on top of a vertex, then
    // advance to the next one and return that.

    foreach (var edge in _path)
    {
      if (edge.Target == currentVertex)
      {
        return edge.Source;
      }
    }

    var vertexX = currentVertex % _pathfinder.Map.Width;
    var vertexY = currentVertex / _pathfinder.Map.Width;
    
    return _path.MinBy(edge =>
    {
      var x = edge.Source % _pathfinder.Map.Width;
      var y = edge.Source / _pathfinder.Map.Width;
      return Math.Sqrt(
        (x - vertexX) * (x - vertexX) +
        (y - vertexY) * (y - vertexY)
      );
    }).Source;
  }

  public void Draw(SpriteBatch spriteBatch)
  {
    if (Debug.DrawDebugLines.Pathfinding.Grid && Debug.DrawDebugLines.Pathfinding.Results)
    { 
      foreach (var edge in ExaminedEdges) {
        var source = _pathfinder.PathfindingGridVertexToWorld(edge.Source);
        var target = _pathfinder.PathfindingGridVertexToWorld(edge.Target);
        
        spriteBatch.DrawLine(
        source.X,
        source.Y,
        target.X,
        target.Y,
          Color.LightGray,
          0.1f
        );
      }
    }

    if (Debug.DrawDebugLines.Pathfinding.Results)
    { 
      if (Path != null)
      {
        foreach (var edge in Path) {
          var source = _pathfinder.PathfindingGridVertexToWorld(edge.Source);
          var target = _pathfinder.PathfindingGridVertexToWorld(edge.Target);
          
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

  public bool AlmostMatches(int sourceVertex, int destVertex, int gridCellsApart)
  {
    // TODO: Use this instead of "Matches" for two reasons:
    //       1) The tank is constantly moving, so the fact it is a few tiles away from the
    //          destination last time we performed pathfinding doesn't negate the need for
    //          an enemy to continue following their path. If they are several edges away
    //          from the tank, we probably don't need to recalculate until the tank has moved
    //          a certain distance from our path.
    //       2) The enemy doesn't follow their own path exactly. The physics engine pushes them
    //          along their path, which means sometimes they are closer to a corner of a
    //          tile that is in the path, but that corner is not part of it. Be more forgiving
    //          when our curent position is close to the path, regardless of how far the tank is. 
    throw new Exception("Not implemented");
  }
  
}
