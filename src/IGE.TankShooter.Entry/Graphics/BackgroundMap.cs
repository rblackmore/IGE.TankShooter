namespace IGE.TankShooter.Entry.Graphics;

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using Physics;

/// <summary>
/// Randomly generate a map of a particular size.
/// Will generate a background layer, then add another layer on top with some flourishes to lighten up the scenery.
/// </summary>
public class BackgroundMap : IBoundingBox
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

  private RectangleF BoundingBox;

  private static readonly ushort[] BackgroundTileIndices = { 0, 1 };
  private static readonly ushort[] DecorativeTileIndices = {
    55, 56, 57, 58, 59, 60,
    73, 74, 75, 76, 77, 78,
    91, 92, 93, 94, 95, 96,
    99, 100, 103, 104,
  };

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
    Map = new TiledMap("background", NumTilesWide, NumTilesHigh, TileWidth, TileHeight, TiledMapTileDrawOrder.LeftDown, TiledMapOrientation.Orthogonal);
    var tileSetTexture = content.Load<Texture2D>("background_tiles");
    var tileSet = new TiledMapTileset(tileSetTexture, TileWidth, TileHeight, TileRows * TileCols, 0, 0, TileCols);
    Map.AddTileset(tileSet, GlobalIdentifier);

    var baseLayer = new TiledMapTileLayer("base", NumTilesWide, NumTilesHigh, TileWidth, TileHeight);
    var decorativeLayer = new TiledMapTileLayer("flourishes", NumTilesWide, NumTilesHigh, TileWidth, TileHeight);

    var rand = new Random();
    for (ushort x = 0; x < NumTilesWide; x++)
    {
      for (ushort y = 0; y < NumTilesHigh; y++)
      {
        baseLayer.SetTile(x, y, BackgroundTileIndices[rand.Next(0, BackgroundTileIndices.Length)]);
        if (rand.NextSingle() < PercentageOfDecoratedTiles)
        {
          decorativeLayer.SetTile(x, y, DecorativeTileIndices[rand.Next(0, DecorativeTileIndices.Length)]);
        }
      }
    }
    
    Map.AddLayer(baseLayer);
    Map.AddLayer(decorativeLayer);
    
    Renderer = new TiledMapRenderer(graphicsDevice, Map);
  }

  public void Draw(OrthographicCamera camera)
  {
    // The tiles are 64px (TileWidth) in screen coordinates, but we want them to render in a scale that is world
    // coordinates and sizes, therefore create a scaling matrix. Multiply it by the view matrix to ensure that we
    // respect zoom/translate of the camera in addition to the scale.
    Renderer.Draw(ScaleMatrix * camera.GetViewMatrix());
  }

  public RectangleF GetBoundingBox()
  {
    return this.BoundingBox;
  }
}
