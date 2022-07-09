namespace IGE.TankShooter.Entry;

using System.Collections.Generic;

using GameComponents;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended.Input;

public class Game1 : Game
{
  private GraphicsDeviceManager _graphics;
  public SpriteBatch _spriteBatch;

  public Game1()
  {
    _graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    // TODO: Add your initialization logic here

    base.Initialize();
  }

  protected override void LoadContent()
  {
    _spriteBatch = new SpriteBatch(GraphicsDevice);
  }
  
  protected override void Update(GameTime gameTime)
  {
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      Exit();

    if (MouseExtended.GetState().WasButtonJustDown(MouseButton.Left))
    {
      var target = Mouse.GetState().Position;
      var initial = GraphicsDevice.Viewport.Bounds.Center;
      Components.Add(new Bullet(this, new Vector2(target.X, target.Y), new Vector2(initial.X, initial.Y)));
    }
    
    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(Color.CornflowerBlue);

    // TODO: Add your drawing code here

    base.Draw(gameTime);
  }
}
