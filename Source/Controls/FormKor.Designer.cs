namespace Ulee.Controls
{
	partial class UlFormKor
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

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다.
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
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
            this.bgPanel.Font = new System.Drawing.Font("Malgun Gothic", 9F);
            this.bgPanel.InnerColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.bgPanel.InnerColor2 = System.Drawing.Color.White;
            this.bgPanel.Location = new System.Drawing.Point(0, 0);
            this.bgPanel.Name = "bgPanel";
            this.bgPanel.OuterColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.bgPanel.OuterColor2 = System.Drawing.Color.White;
            this.bgPanel.Size = new System.Drawing.Size(284, 261);
            this.bgPanel.Spacing = 0;
            this.bgPanel.TabIndex = 0;
            this.bgPanel.TextHAlign = Ulee.Controls.EUlHoriAlign.Center;
            this.bgPanel.TextVAlign = Ulee.Controls.EUlVertAlign.Middle;
            // 
            // UlFormKor
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.bgPanel);
            this.Font = new System.Drawing.Font("Malgun Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UlFormKor";
            this.Text = "UlFormKor";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        #endregion

        public UlPanel bgPanel;
    }
}
