namespace IGE.TankShooter.Entry.Core.StatsDisplay.Data;
using System;
using System.Collections.Generic;
using System.Linq;

using IGE.TankShooter.Entry.Stats;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class FrameRateCounter : GameObject, ITextValue
{
  public const int MAX_SAMPLES = 100;
  private Queue<float> samples = new Queue<float>();

  private CountdownTimer timer;

  public FrameRateCounter()
  {

  }

  public string Text { get; private set; }

  public long TotalFrames { get; set; }
  public float TotalSeconds { get; set; }
  public float AverageFramesPerSecond { get; set; }
  public float CurrentFramesPerSecond { get; set; }

  public override void Initialize()
  {
    this.timer = new CountdownTimer(0.5f, true);
    this.timer.CountdownTriggered += Timer_CountdownTriggered;
    this.Text = "FPS: 0";
    base.Initialize();
  }

  private void Timer_CountdownTriggered(object sender, EventArgs e)
  {
    this.AverageFramesPerSecond = this.samples.Average(i => i);
    this.Text = String.Format("FPS: {0}", this.AverageFramesPerSecond.ToString("0.00"));
  }

  public override void Update(GameTime gameTime)
  {
    this.timer.Update(gameTime);

    var sample = 1.0f / gameTime.GetElapsedSeconds();
    this.AddSample(sample);

    //TotalFrames++;
    //TotalSeconds += gameTime.GetElapsedSeconds();

    //CurrentFramesPerSecond = 1.0f / gameTime.GetElapsedSeconds();
  }

  private void AddSample(float sample)
  {
    samples.Enqueue(sample);

    if (samples.Count > MAX_SAMPLES)
      samples.Dequeue();
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    // Not Required.
    throw new NotImplementedException();
  }

}
