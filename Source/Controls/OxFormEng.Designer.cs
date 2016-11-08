namespace OxLib.Controls
{
	partial class OxFormEng
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
			this.BackPanel = new OxLib.Controls.OxPanel();
			this.SuspendLayout();
			// 
			// BackPanel
			// 
			this.BackPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BackPanel.Location = new System.Drawing.Point(0, 0);
			this.BackPanel.Margin = new System.Windows.Forms.Padding(0);
			this.BackPanel.Name = "BackPanel";
			this.BackPanel.Size = new System.Drawing.Size(284, 262);
			this.BackPanel.TabIndex = 0;
			// 
			// OxFormEng
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.BackPanel);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "OxFormEng";
			this.Text = "OxFormEng";
			this.ResumeLayout(false);

		}

		#endregion

		public OxPanel BackPanel;
	}
}