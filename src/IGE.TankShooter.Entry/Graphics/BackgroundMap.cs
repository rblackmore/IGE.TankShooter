namespace IGE.TankShooter.Entry.Graphics;

using System;
using System.Collections.Generic;

using Core;

using GameObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
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
      if (_map == null)
      {
        throw new Exception(
          "Attempting to access bounding box of BackgroundMap prior to the LoadContent() method being called. Must wait for this so that the size can be derived from the tile map."
          );
      }

      return _boundingBox;
    }
  }

  public List<ICollisionActor> GetCollisionTargets()
  {
    var targets = new List<ICollisionActor>(4)
    {
      new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Bottom, BoundingBox),
      new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Top, BoundingBox),
      new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Left, BoundingBox),
      new EdgeOfTheWorld(_game, EdgeOfTheWorld.Side.Right, BoundingBox)
    };

    return targets;
  }

  public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
  {
    _map = content.Load<TiledMap>("Maps/level1");
    _boundingBox = new RectangleF(0f, 0f, _map.Width * TileWidthWorldUnits, _map.Height * TileWidthWorldUnits);
    
    TiledMapRenderer renderer = new TiledMapRenderer(graphicsDevice, _map);

    _texture = new RenderTarget2D(graphicsDevice, _map.WidthInPixels, _map.HeightInPixels);
    _sprite = new Sprite(_texture) { OriginNormalized = new Vector2(0f, 0f) };
    graphicsDevice.SetRenderTarget(_texture);
    graphicsDevice.Clear(Color.Black);
    renderer.Draw();
    graphicsDevice.SetRenderTarget(null);
  }

  public void Draw(SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(_sprite, _transform);

    if (Debug.DrawDebugLines)
    {
      spriteBatch.DrawRectangle(BoundingBox, Color.Blue, 0.2f);
    }
  }
}
