namespace IGE.TankShooter.Entry.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;

using Core;

using GameObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using Vector2 = Microsoft.Xna.Framework.Vector2;

/// <summary>
/// Draw a predefined tile map to the screen. The tilemap starts at (0, 0), is made up of 16px by 16px tiles, and
/// each tile is TileWidthWorldUnits wide in world units.
///
/// This is also responsible for returning a collection of ICollisionActors representing "the edge of the world".
/// In the future, it will also include collision actors for each of the entities defined as collidable via the
/// map editor (the Tiled map editor supports the notion of collision boundaries for tiles).
/// 
/// Each map could be a different size, so we expose the size in world units and in screen coordinates for the
/// game to be able to setup other aspects
///
/// While the MonoGame.Extended.Tiled library is perfectly capable of drawing tiles directly to the buffer, and
/// and indeed it will be performant as it only draws the tiles in view, this approach is not used. Instead, we
/// first render the entire map to a texture during LoadContent() and render that texture during Draw().
/// This is because the way in which we smoothly zoom in-and-out with the speed of the tank causes floating point
/// rounding errors and thus the tiles end up with black lines between rows and/or columns. By first rendering to
/// a texture then letting that texture get rendered, we alleviate these issues.
/// </summary>
public class BackgroundMap
{

  private TiledMap _map;
  private RenderTarget2D _texture;
  private Sprite _sprite;

  private const float TileWidthWorldUnits = 2f;
  private const float TileSize = 16;

  private readonly Transform2 _transform;
  private readonly Game1 _game;

  public BackgroundMap(Game1 game)
  {
    this._game = game;
    
    const float scale = TileWidthWorldUnits / TileSize;
    _transform = new Transform2(Vector2.Zero, 0f, new Vector2(scale, scale));
  }

  private RectangleF _boundingBox;

  public RectangleF BoundingBox
  {
    get
    {
      EnsureMapIsLoaded();
      return _boundingBox;
    }
  }

  private void EnsureMapIsLoaded()
  {
      if (_map == null)
      {
        throw new Exception("Attempting to access prior to the LoadContent() method being called.");
      }
  }

  private List<ICollisionActor> _collisionTargets = null;

  public List<ICollisionActor> GetCollisionTargets()
  {
    if (_collisionTargets == null)
    {
      EnsureMapIsLoaded();
      _collisionTargets = new List<ICollisionActor>()
      {
        new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Bottom, BoundingBox),
        new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Top, BoundingBox),
        new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Left, BoundingBox),
        new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Right, BoundingBox)
      };

      var mapObjects = LoadCollisionShapesFromTiledMap(_map, _transform);
      
      // Call MergeMapObjects twice. The first time, it will combine all horizontally of adjacent objects
      // together, and the next time, it will do vertically adjacent objects.
      // We can optimise the function so that it only needs one pass, but this suffices for now.
      var mergedMapObjects = MergeMapObjects(MergeMapObjects(mapObjects));
      
      _collisionTargets.AddRange(mergedMapObjects);
    }

    return _collisionTargets;
  }

  public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
  {
    _map = content.Load<TiledMap>("Maps/level1");
    _boundingBox = new RectangleF(0f, 0f, _map.Width * TileWidthWorldUnits, _map.Height * TileWidthWorldUnits);
    
    TiledMapRenderer renderer = new TiledMapRenderer(graphicsDevice, _map);

    _texture = new RenderTarget2D(graphicsDevice, _map.WidthInPixels, _map.HeightInPixels);
    _sprite = new Sprite(_texture) { OriginNormalized = new Vector2(0f, 0f) };
    graphicsDevice.SetRenderTarget(_texture);
    graphicsDevice.BlendState = BlendState.AlphaBlend;
    graphicsDevice.Clear(Color.Black);
    renderer.Draw();
    graphicsDevice.SetRenderTarget(null);
  }

  public void Draw(SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(_sprite, _transform);

    if (Debug.DrawDebugLines)
    {
      foreach (var target in GetCollisionTargets())
      {
        if (target.Bounds is RectangleF rect)
        {
          spriteBatch.DrawRectangle(rect, Color.Blue, 0.1f);
        }
        else if (target.Bounds is CircleF circle)
        {
          spriteBatch.DrawCircle(circle, 10, Color.Blue, 0.1f);
        }
      }
    }
  }

  /**
   * API still seems a bit lacking: https://community.monogame.net/t/getting-tile-properties-from-tiledmaptile/8858/6
   * But essentially for each tile, we loop over all tilesets and their respective tiles until we find the
   * one which matches the tile in question, so that we can ask questions like "what is your collision bounds".
   * At least we only need to do it once for now.
   */
  private static List<MapObject> LoadCollisionShapesFromTiledMap(TiledMap map, Transform2 transform)
  {
    List<MapObject> mapObjects = new List<MapObject>();
    map.ForEachTile(((tile, tilesetTile) =>
    {
      if (tilesetTile.Objects.Count > 0)
      {
        var mapObject = new MapObject(map, tile, tilesetTile, transform.Scale);
        mapObjects.Add(mapObject);
      }
    }));
    
    SortMapObjects(mapObjects);

    return mapObjects;
  }

  /**
   * In order to merge adjacent objects together, we need to sort them from top to bottom, left to right.
   * This is because the merge algorithm below iterates over them assuming they are in this order, merging
   * horizontally adjacent cells before then merging vertically adjacent ones.
   *  
   * Not sure this is required... objects in a tiled map seem by convention to already be laid out from left
   * to right, top to bottom.
   */
  private static void SortMapObjects(List<MapObject> mapObjects)
  {
    mapObjects.Sort((m1, m2) =>
    {
      if (m1.Bounds is CircleF)
      {
        return 1;
      }

      if (m2.Bounds is CircleF)
      {
        return -1;
      }

      var r1 = (RectangleF)m1.Bounds;
      var r2 = (RectangleF)m2.Bounds;
     
      if (Math.Abs(r1.Top - r2.Top) > 0.05)
      {
        return r1.Top.CompareTo(r2.Top);
      }
      else
      {
        return r1.Left.CompareTo(r2.Left);
      }
    });
  }

  private static bool IsWithinMergeDistance(float a, float b)
  {
    const float MERGE_LEEWAY = 2;
    return Math.Abs(a - b) < MERGE_LEEWAY;
  }

  private static IEnumerable<MapObject> MergeMapObjects(IEnumerable<MapObject> mapObjects)
  {
    List<MapObject> mergedMapObjects = new List<MapObject>();
    MapObject currentObj = null;
    foreach (MapObject nextObj in mapObjects)
    {
      if (nextObj.Bounds is CircleF)
      {
        mergedMapObjects.Add(nextObj);
        continue;
      }
        
      if (currentObj == null)
      {
        currentObj = nextObj;
        continue;
      }

      var currentRect = (RectangleF)currentObj.Bounds;
      var nextRect = (RectangleF)nextObj.Bounds;

      // Should we merge this with the previously collected objects to the left or above?
      if (
        (
          IsWithinMergeDistance(currentRect.Top, nextRect.Top) &&
          IsWithinMergeDistance(currentRect.Bottom, nextRect.Bottom) &&
          IsWithinMergeDistance(currentRect.Right, nextRect.Left)
        ) || (
          IsWithinMergeDistance(currentRect.Left, nextRect.Left) &&
          IsWithinMergeDistance(currentRect.Right, nextRect.Right) &&
          IsWithinMergeDistance(currentRect.Bottom, nextRect.Top)
        )
      )
      {
        currentObj = new MapObject(currentRect, nextRect);
        continue;
      }

      // Couldn't figure out how to merge the newly created record with the last merged
      // row, so just add it as its own thing. Of course, the next merged item still gets a chance to merge with
      // this in the future, so all is not lost.
      mergedMapObjects.Add(currentObj);
      currentObj = nextObj;
      
    }

    if (currentObj != null)
    {
      mergedMapObjects.Add(currentObj);
    }

    return mergedMapObjects;
  }
}
