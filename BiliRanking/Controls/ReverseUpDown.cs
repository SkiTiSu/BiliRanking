using System;
using System.Windows.Forms;

public class ReverseUpDown : NumericUpDown
{
    public override void UpButton()
    {
        base.DownButton();
    }
    public override void DownButton()
    {
        base.UpButton();
    }
}