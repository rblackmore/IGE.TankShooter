namespace IGE.TankShooter.Entry.Stats;
using System;

public class SimpleTextValueImplementation : ITextValue
{
  public string Text { get; private set; }

  public SimpleTextValueImplementation(string value)
  {
    this.Text = value;
  }
}
