namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Sprites;

public class Bullet : GameObject
{
  const float SPEED = 100f;

  private readonly Game1 tankGame;
  private Vector2 Position { get; set; }
  private Vector2 Velocity { get; set; }
  private Sprite Sprite;
  
  public Bullet(Game1 game, Texture2D texture, Vector2 targetPosition, Vector2 initialPosition)
  {
    this.Position = initialPosition;
    this.Velocity = (targetPosition - initialPosition).NormalizedCopy() * SPEED;
    this.tankGame = game;
    this.Sprite = new Sprite(texture);
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    Sprite.Draw(spriteBatch, Position, Velocity.ToAngle(), new Vector2(0.05f));
  }

  public override void Update(GameTime gameTime)
  {
    this.Position += this.Velocity * gameTime.GetElapsedSeconds();

    // TODO: Use world coordinates.
    if (!this.tankGame.GraphicsDevice.Viewport.Bounds.Contains(this.Position))
    {
      this.tankGame.Bullets.Remove(this);
    }
  }
}
