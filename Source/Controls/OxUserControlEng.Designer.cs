namespace OxLib.Controls
{
	partial class OxUserControlEng
	{
		/// <summary> 
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region 구성 요소 디자이너에서 생성한 코드

		/// <summary> 
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
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
			this.BackPanel.Size = new System.Drawing.Size(300, 300);
			this.BackPanel.TabIndex = 0;
			// 
			// OxUserControlEng
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.BackPanel);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "OxUserControlEng";
			this.Size = new System.Drawing.Size(300, 300);
			this.ResumeLayout(false);

		}

		#endregion

		public Controls.OxPanel BackPanel;
	}
}
