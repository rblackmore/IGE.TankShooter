namespace IGE.TankShooter.Entry.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class GraphicsHelpers
{
  public static Texture2D CreateColouredRectangle(GraphicsDevice graphicsDevice, int height, int width, Color color)
  {
    Texture2D rectangle = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);

    Color[] colorData = new Color[height * width];

    for (int i = 0; i < colorData.Length; i++)
    {
      colorData[i] = color;
    }

    rectangle.SetData(colorData);

    return rectangle;
  }
}
