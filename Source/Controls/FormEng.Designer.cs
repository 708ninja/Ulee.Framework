namespace Ulee.Controls
{
	partial class UlFormEng
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.bgPanel = new Ulee.Controls.UlPanel();
            this.SuspendLayout();
            // 
            // bgPanel
            // 
            this.bgPanel.AutoSize = true;
            this.bgPanel.BevelInner = Ulee.Controls.EUlBevelStyle.None;
            this.bgPanel.BevelOuter = Ulee.Controls.EUlBevelStyle.None;
            this.bgPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bgPanel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.bgPanel.InnerColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.bgPanel.InnerColor2 = System.Drawing.Color.White;
            this.bgPanel.Location = new System.Drawing.Point(0, 0);
            this.bgPanel.Margin = new System.Windows.Forms.Padding(0);
            this.bgPanel.Name = "bgPanel";
            this.bgPanel.OuterColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.bgPanel.OuterColor2 = System.Drawing.Color.White;
            this.bgPanel.Size = new System.Drawing.Size(284, 261);
            this.bgPanel.Spacing = 0;
            this.bgPanel.TabIndex = 0;
            this.bgPanel.TextHAlign = Ulee.Controls.EUlHoriAlign.Center;
            this.bgPanel.TextVAlign = Ulee.Controls.EUlVertAlign.Middle;
            // 
            // UlFormEng
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.bgPanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "UlFormEng";
            this.Text = "UlFormEng";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public UlPanel bgPanel;
	}
}