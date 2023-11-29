namespace IGE.TankShooter.Entry.Graphics;

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

/// <summary>
/// Randomly generate a map of a particular size.
/// Will generate a background layer, then add another layer on top with some flourishes to lighten up the scenery.
/// </summary>
public class BackgroundMap
{

  private TiledMap Map;
  private TiledMapRenderer Renderer;
  
  private const float TileWidthWorldUnits = 3f;
  private const int TileWidth = 64;
  private const int TileHeight = 64;
  private const int TileRows = 7;
  private const int TileCols = 18;
  private const float PercentageOfDecoratedTiles = 0.05f;

  private const int GlobalIdentifier = 0;

  public RectangleF BoundingBox { get; }

  private readonly int NumTilesWide;
  private readonly int NumTilesHigh;
  private readonly float Scale;
  private readonly Matrix ScaleMatrix;

  public BackgroundMap(float widthInWorldUnits, float heightInWorldUnits)
  {
    this.Scale = TileWidthWorldUnits / TileWidth;
    this.ScaleMatrix = Matrix.CreateScale(this.Scale);
    this.NumTilesWide = (int)((widthInWorldUnits / this.Scale) / TileWidth);
    this.NumTilesHigh = (int)((heightInWorldUnits / this.Scale) / TileHeight);

    this.BoundingBox = new RectangleF(0f, 0f, widthInWorldUnits, heightInWorldUnits);
  }

  public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
  {
    Map = content.Load<TiledMap>("Maps/level1");
    Renderer = new TiledMapRenderer(graphicsDevice, Map);
  }

  public void Draw(OrthographicCamera camera)
  {
    // The tiles are 64px (TileWidth) in screen coordinates, but we want them to render in a scale that is world
    // coordinates and sizes, therefore create a scaling matrix. Multiply it by the view matrix to ensure that we
    // respect zoom/translate of the camera in addition to the scale.
    Renderer.Draw(ScaleMatrix * camera.GetViewMatrix());
  }
}
