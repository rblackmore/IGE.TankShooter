namespace IGE.TankShooter.Entry.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public abstract class GameObject
{
  public abstract void Initialize();
  public abstract void LoadContent();
  public abstract void UnloadContent();
  public abstract void Update(GameTime gameTime);
  public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
