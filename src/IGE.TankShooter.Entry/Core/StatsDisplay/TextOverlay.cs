namespace IGE.TankShooter.Entry.Core.StatsDisplay;

using System.Collections.Generic;
using System.Linq;

using IGE.TankShooter.Entry.Core;
using IGE.TankShooter.Entry.Stats;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class TextOverlay : GameObject
{
  private readonly Game1 game;
  private SpriteFont font;

  private IList<ITextValue> values = new List<ITextValue>();
  private Vector2 position = new Vector2(5);
  private float lineHeight;

  public TextOverlay(Game1 game)
  {
    this.game = game;
  }

  public void Add(ITextValue newValue)
  {
    values.Add(newValue);
  }

  public void Remove(ITextValue removeValue)
  {
    values.Remove(removeValue);
  }

  public override void Initialize()
  {
    base.Initialize();
  }

  public override void LoadContent()
  {
    base.LoadContent();
    font = game.Content.Load<SpriteFont>("Fonts/Hack");
    lineHeight = font.MeasureString("Hello, World!!!").Y;
  }

  public override void Update(GameTime gameTime)
  {
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    var lineNo = 0;

    foreach (var textValue in values)
    {
      var linePosition = position.SetY(position.Y + lineHeight * lineNo);
      spriteBatch.DrawString(font, textValue.Text, linePosition, Color.White);
      lineNo++;
    }
  }
}
