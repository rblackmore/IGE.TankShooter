namespace IGE.TankShooter.Entry.Core;

using System.Collections.Generic;

using Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;

using QuikGraph;
using QuikGraph.Algorithms;

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

  public Pathfinder(TiledMap map, float tileWidthWorldUnits)
  {
    this._map = map;
    this._tileWidthWorldUnits = tileWidthWorldUnits;
    this._mapScale = new Vector2(tileWidthWorldUnits / map.TileWidth, tileWidthWorldUnits / map.TileHeight);
  }

  public void LoadContent()
  {
    this._pathfindingGraph = BuildRectangularGraph();
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
    var mouseVertex = (int)destTile.X + (int)destTile.Y * _map.Width;

    var algo = _pathfindingGraph.ShortestPathsAStar(
      _ => 1,
      _ => 1,
      targetVertex
    );

    if (!algo(mouseVertex, out _pathfindingResults))
    {
      _pathfindingResults = null;
    }
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

    var gridColour = new Color(0, 0, 0, 0.5f);

    foreach (var vertexIdx in _pathfindingGraph.Vertices)
    {
      var vertex = PathfindingGridVertexToWorld(vertexIdx);
      spriteBatch.DrawCircle(
        vertex.X, 
        vertex.Y, 
        0.2f, 
        10, 
        gridColour,
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
        gridColour,
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
