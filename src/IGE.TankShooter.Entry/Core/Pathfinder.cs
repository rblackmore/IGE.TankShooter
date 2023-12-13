namespace IGE.TankShooter.Entry.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

using Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Tiled;

using QuikGraph;
using QuikGraph.Algorithms;
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
  private IEnumerable<Edge<int>> _pathfindingResults;
  private readonly float _tileWidthWorldUnits;

  private int extraTargetVertex = -1;
  private int extraDestVertex = -1;

  public Pathfinder(TiledMap map, float tileWidthWorldUnits)
  {
    this._map = map;
    this._tileWidthWorldUnits = tileWidthWorldUnits;
    this._mapScale = new Vector2(tileWidthWorldUnits / map.TileWidth, tileWidthWorldUnits / map.TileHeight);
  }

  public void LoadContent()
  {
    // this._pathfindingGraph = BuildPolygonBasedNavmeshFromTiledLayer();
    // this._pathfindingGraph = BuildRectangularGraph();
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

    var edgeWeights = new Func<Edge<int>, double>(edge =>
    {
      var source = PathfindingGridVertexToWorld(edge.Source);
      var target = PathfindingGridVertexToWorld(edge.Target);
      return (source - target).Length();
    });

    // TODO: Use this heuristic to prioritise vertices for which the direction from the tank to the mouse is similar
    //       to the direction from the tank to the vertex in question.
    /*var vectorToTarget = targetTile - destTile;
    var angleToTarget = vectorToTarget.ToAngle();
    var costHeuristic = new Func<int, double>(vertex =>
    {
      var vertexTile = PathfindingGridVertexToWorld(vertex);
      var vectorToVertexFromTarget = targetTile - vertexTile;
      var angleToVertexFromTarget = vectorToVertexFromTarget.ToAngle();
      return angleToVertexFromTarget - angleToTarget;
    });*/
    var costHeuristic = new Func<int, double>(_ => 1);

    var algo = new AStarShortestPathAlgorithm<int, Edge<int>>(_pathfindingGraph, edgeWeights, costHeuristic);

    Console.WriteLine($"Asking AStar to terminate when {destVertex} found.");
    algo.FinishVertex += vertex =>
    {
      if (vertex == destVertex)
      {
        Console.WriteLine($"Terminating pathfinding because vertex {destVertex} was found.");
        algo.Abort();
      }
    };

    // Add in extra vertices for the items of interest each time we do pathfinding, then
    // remove them when done. Because we are potentially using navigation meshes, it is
    // likely that the tank and enemy do not fall exactly on one of the graph vertices.

    // TODO This is all very messy, doesn't seem to work as expected.
    //   Don't remove and re-add the extra vertex each run if it hasn't changed.
    
    Console.WriteLine($"Removing target vertex {extraTargetVertex} and dest vertex {extraDestVertex}");
   
    // Be defensive here, because otherwise if the tank drives over an existing vertex, that
    // vertex will accidentally be removed from the navigation mesh. Only remove vertexes that
    // we created ourselves.
   
    if (extraDestVertex != destVertex)
    {
      _pathfindingGraph.RemoveVertex(extraDestVertex);
      var nearestVertexToDest = _pathfindingGraph.Vertices.MinBy(v => (PathfindingGridVertexToWorld(v) - dest).Length());
      
      if (!_pathfindingGraph.ContainsVertex(destVertex))
      {
        Console.WriteLine($"Adding dest vertex {destVertex} and edge {destVertex}-{nearestVertexToDest}");
        
        extraDestVertex = destVertex;
        _pathfindingGraph.AddVertex(destVertex);
        _pathfindingGraph.AddEdge(new Edge<int>(destVertex, nearestVertexToDest));
      }
      else
      {
        Console.WriteLine($"Not adding dest vertex {destVertex} because it is already in the graph.");
      }
    }

    if (extraTargetVertex != destVertex)
    {
      _pathfindingGraph.RemoveVertex(extraTargetVertex);
      var nearestVertexToTarget = _pathfindingGraph.Vertices.MinBy(v => (PathfindingGridVertexToWorld(v) - target).Length());
      
      if (!_pathfindingGraph.ContainsVertex(targetVertex))
      {
        Console.WriteLine($"Adding target vertex {targetVertex} and edge {targetVertex}-{nearestVertexToTarget}");
        
        extraTargetVertex = targetVertex;
        _pathfindingGraph.AddVertex(targetVertex);
        _pathfindingGraph.AddEdge(new Edge<int>(targetVertex, nearestVertexToTarget));
      }
      else
      {
        Console.WriteLine($"Not adding target vertex {targetVertex} because it is already in the graph.");
      }
    }
   
    var predecessors = new VertexPredecessorRecorderObserver<int, Edge<int>>();
    using (predecessors.Attach(algo))
    {
      // Run the algorithm with A set to be the source
      Console.WriteLine($"Pathfinding to {targetVertex}");
      algo.Compute(targetVertex);
    }

    if (!predecessors.TryGetPath(destVertex, out _pathfindingResults))
    {
      _pathfindingResults = null;
    }
  }

  private AdjacencyGraph<int, Edge<int>> BuildPolygonBasedNavmeshFromTiledLayer()
  {
    var navmeshLayer = _map.ObjectLayers.FirstOrDefault(l => l.Name == "Navmesh (Polygons)");
    if (navmeshLayer == null)
    {
      return new AdjacencyGraph<int, Edge<int>>();
    }

    var pathfindingEdges = new List<Edge<int>>();
    foreach (var obj in navmeshLayer.Objects)
    {
      if (obj is TiledMapPolygonObject polygon)
      {
        var polygonWithMidpoints = new List<Point2>(polygon.Points.Length * 2);
        for (int i = 0; i < polygon.Points.Length; i++)
        {
          int x = (int)polygon.Position.X / _map.TileWidth;
          int y = (int)polygon.Position.Y / _map.TileHeight;

          var start = polygon.Points[i];
          int startX = x + (int)start.X / _map.TileWidth;
          int startY = y + (int)start.Y / _map.TileHeight;

          var end = polygon.Points[(i + 1) % polygon.Points.Length];
          int endX = x + (int)end.X / _map.TileWidth;
          int endY = y + (int)end.Y / _map.TileHeight;

          int midpointX = (int)Math.Round(MathHelper.Lerp(startX, endX, 0.5f));
          int midpointY = (int)Math.Round(MathHelper.Lerp(startY, endY, 0.5f));

          polygonWithMidpoints.Add(new Point2(startX, startY));
          polygonWithMidpoints.Add(new Point2(midpointX, midpointY));
        }
        
        for (int i = 0; i < polygonWithMidpoints.Count; i ++)
        {

          int startX = (int)polygonWithMidpoints[i].X;
          int startY = (int)polygonWithMidpoints[i].Y;

          // From one point to all other points. Some lines will overlap, but
          // that is probably okay.
          for (int j = 0; j < polygonWithMidpoints.Count; j++)
          {
            if (j == i)
            {
              continue;
            }
            
            int endX = (int)polygonWithMidpoints[j].X;
            int endY = (int)polygonWithMidpoints[j].Y;
          
            pathfindingEdges.Add(new Edge<int>( Math.Min(_map.Height - 1, (startY)) * _map.Width + Math.Min(_map.Width - 1, startX), 
              Math.Min(_map.Height - 1, (endY)) * _map.Width + Math.Min(_map.Width - 1, endX)
            ));
          }
        }
      }
    }
    
    return pathfindingEdges.DistinctBy(e => $"{e.Source}-{e.Target}").ToAdjacencyGraph<int, Edge<int>>();
  }

  /// <summary>
  /// Parse rectangles from an Object layer in the Tiled map.
  /// See https://github.com/mikewesthad/navmesh/blob/master/tiled-navmesh-guide.md for inspiration.
  /// </summary>
  /// <returns></returns>
  private AdjacencyGraph<int, Edge<int>> BuildRectangleBasedNavmeshFromTiledLayer()
  {
    var navmeshLayer = _map.ObjectLayers.FirstOrDefault(l => l.Name == "Navmesh (Rects)");
    if (navmeshLayer == null)
    {
      return new AdjacencyGraph<int, Edge<int>>();
    }

    var pathfindingEdges = new List<Edge<int>>();
    foreach (var obj in navmeshLayer.Objects)
    {
      var left = (int)(obj.Position.X) / _map.TileWidth;
      var right = Math.Min(_map.Width - 1, left + (int)(obj.Size.Width) / _map.TileWidth);
      var horizontalCenter = Math.Min(_map.Height - 1, left + (int)(obj.Size.Width / 2) / _map.TileWidth);
      var top = (int)(obj.Position.Y) / _map.TileHeight;
      var bottom = Math.Min(_map.Height - 1, top + (int)(obj.Size.Height) / _map.TileWidth);
      var verticalCenter = Math.Min(_map.Height - 1, top + (int)(obj.Size.Height / 2) / _map.TileWidth);
     
      // Four sides of the rect.
      pathfindingEdges.Add(new Edge<int>(top * _map.Width + left, top * _map.Width + right));
      pathfindingEdges.Add(new Edge<int>(top * _map.Width + left, bottom * _map.Width + left));
      pathfindingEdges.Add(new Edge<int>(bottom * _map.Width + left, bottom * _map.Width + right));
      pathfindingEdges.Add(new Edge<int>(top * _map.Width + right, bottom * _map.Width + right));
     
      // 2 x diagonals
      pathfindingEdges.Add(new Edge<int>(top * _map.Width + right, bottom * _map.Width + left));
      pathfindingEdges.Add(new Edge<int>(bottom * _map.Width + right, top * _map.Width + left));
      
      // Horizontal line through the center
      pathfindingEdges.Add(new Edge<int>(verticalCenter * _map.Width + left, verticalCenter * _map.Width + right));
      
      // Vertical line through the center
      pathfindingEdges.Add(new Edge<int>(top * _map.Width + horizontalCenter, bottom * _map.Width + horizontalCenter));
      
    }
    
    return pathfindingEdges.DistinctBy(e => $"{e.Source}-{e.Target}").ToAdjacencyGraph<int, Edge<int>>();
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

  private AdjacencyGraph<int, Edge<int>> BuildRectangularGraph()
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
          
        // Left
        if (x > 0 && !hasObjectGraph[x - 1, y])
        {
          pathfindingEdges.Add(new Edge<int>(source, y * _map.Width + x - 1));
        }
      }
    }
    return pathfindingEdges.ToAdjacencyGraph<int, Edge<int>>();
  }

  private int WorldToPathfindingGridVertex(Vector2 worldCoords)
  {
    var tile = worldCoords / _tileWidthWorldUnits;
    var vertex = (int)tile.X + (int)tile.Y * _map.Width;

    return vertex;
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
    
    if (_pathfindingResults != null)
    {
      foreach (var edge in _pathfindingResults)
      {
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
