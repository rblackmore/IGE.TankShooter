namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class Bullet : GameObject
{

  const float SPEED = 500f;

  private readonly Game1 tankGame;
  private Vector2 Position { get; set; }
  private Vector2 Velocity { get; set; }

  public Bullet(Game1 game, Vector2 targetPosition, Vector2 initialPosition)
  {
    this.Position = initialPosition;
    this.Velocity = (targetPosition - initialPosition).NormalizedCopy() * SPEED;
    this.tankGame = game;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.FillRectangle(new RectangleF(Position.X, Position.Y, 10f, 10f), Color.White);
  }

  public override void Update(GameTime gameTime)
  {
    this.Position += this.Velocity * gameTime.GetElapsedSeconds();

    if (!this.tankGame.GraphicsDevice.Viewport.Bounds.Contains(this.Position))
    {
      this.tankGame.Bullets.Remove(this);
    }
  }
}
