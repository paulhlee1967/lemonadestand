using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace LemonadeStand
{
    partial class LemonadeStandGame
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "LemonadeStandGame";
            this.ResumeLayout(false);
        }
    }
}