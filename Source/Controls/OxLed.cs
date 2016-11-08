
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

//------------------------------------------------------------------------------
namespace OxLib.Controls
{
	//--------------------------------------------------------------------------
	public enum EOxLedType
	{
		Rectangle,
		Circle
	}

	[ToolboxBitmap(typeof(OxLed), "OxLed.bmp")]
	//--------------------------------------------------------------------------
	public partial class OxLed : OxPanel
	{
		private bool active;
		private EOxLedType type;
		private Color onColor;
		private Color offColor;
		private int gap;
		private Brush bBrush;
		private Brush lBrush;

		//----------------------------------------------------------------------
		[Category("Behavior")]
		public bool Active
		{
			get
			{
				return active;
			}
			set
			{
				active = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public EOxLedType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color OnColor
		{
			get
			{
				return onColor;
			}
			set
			{
				onColor = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Appearance")]
		public Color OffColor
		{
			get
			{
				return offColor;
			}
			set
			{
				offColor = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		[Category("Layout")]
		public int Gap
		{
			get
			{
				return gap;
			}
			set
			{
				gap = value;
				Invalidate();
			}
		}

		//----------------------------------------------------------------------
		public OxLed()
		{
			active = false;
			type = EOxLedType.Circle;
			offColor = Color.Black;
			onColor = Color.Red;
			bBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
			BevelInner = EOxBevelStyle.None;
			BevelOuter = EOxBevelStyle.None;
			gap = 0;

			Invalidate();
		}

		//----------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (Width > 3 && Height > 3)
			{
				switch (type)
				{
					case EOxLedType.Circle:
						DrawCircle(e.Graphics);
						break;

					case EOxLedType.Rectangle:
						DrawRectangle(e.Graphics);
						break;
				}
			}
		}

		//----------------------------------------------------------------------
		private void DrawCircle(Graphics g)
		{
			Pen P;
			int nX, nY, nWidth, nHeight, nThick;

			nX = gap;
			nY = gap;
			nWidth = Width - (gap * 2);
			nHeight = Height - (gap * 2);

			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.FillEllipse(bBrush, nX, nY, nWidth-1, nHeight-1);

			nX++;
			nY++;
			nWidth = Width - (gap * 2) - 2;
			nHeight = Height - (gap * 2) - 2;
			lBrush = new SolidBrush(active ? onColor : offColor);

			g.FillEllipse(lBrush, nX, nY, nWidth-1, nHeight-1);

			nThick = Width / 20;
			if (nThick < 1) nThick = 1;

			if ((active == false) && (offColor == Color.White))
			{
				P = new Pen(Color.FromArgb(240, 240, 240), nThick);
			}
			else
			{
				P = new Pen(Color.White, nThick);
			}

			nThick = Width / 5;
			nX += nThick;
			nY += nThick;
			nWidth = Width - (gap * 2) - (nThick * 2);
			nHeight = Height - (gap * 2) - (nThick * 2);

			g.DrawArc(P, nX, nY, nWidth, nHeight, 200, 50);
			g.SmoothingMode = SmoothingMode.None;
		}

		//----------------------------------------------------------------------
		private void DrawRectangle(Graphics g)
		{
			int nX, nY, nWidth, nHeight;
			lBrush = new SolidBrush(active ? onColor : offColor);

			nX = gap;
			nY = gap;
			nWidth = Width - (gap * 2);
			nHeight = Height - (gap * 2);

			g.FillRectangle(bBrush, nX, nY, nWidth, nHeight);

			nX++;
			nY++;
			nWidth = Width - (gap * 2) - 2;
			nHeight = Height - (gap * 2) - 2;

			g.FillRectangle(lBrush, nX, nY, nWidth, nHeight);
		}
	}
}
//------------------------------------------------------------------------------
