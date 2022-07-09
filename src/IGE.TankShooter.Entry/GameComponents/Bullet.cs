namespace IGE.TankShooter.Entry.GameComponents;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class Bullet : DrawableGameComponent
{

  const float SPEED = 500f;

  private readonly Game1 tankGame;
  private Vector2 Position { get; set; }
  private Vector2 Velocity { get; set; }

  public Bullet(Game1 game, Vector2 targetPosition, Vector2 initialPosition) : base(game)
  {
    this.tankGame = game;
    this.Position = initialPosition;
    this.Velocity = (targetPosition - initialPosition).NormalizedCopy() * SPEED;

  }

  public override void Draw(GameTime gameTime)
  {
    base.Draw(gameTime);
    this.tankGame._spriteBatch.Begin();
    this.tankGame._spriteBatch.FillRectangle(new RectangleF(Position.X, Position.Y, 10f, 10f), Color.White);
    this.tankGame._spriteBatch.End();
  }

  public override void Update(GameTime gameTime)
  {
    base.Update(gameTime);
    this.Position += this.Velocity * gameTime.GetElapsedSeconds();

    if (!this.tankGame.GraphicsDevice.Viewport.Bounds.Contains(this.Position))
    {
      this.Game.Components.Remove(this);
    }
  }
}
