using System;
using System.Drawing;
using System.Windows.Forms;
using UnityEngine;

namespace UnityFloatingImage
{

	public class FloatingImageForm : Form
	{
		TextureImage m_Image;
		Action<bool> m_Refresh;
		bool m_Dragable = false;
		bool m_IsDragging = false;
		bool m_Relative;
		Point m_Offset;
		Vector2Int m_RelativePosition;
		RectInt m_PrevCurrentWindowRect;


		public FloatingImageForm(Texture texture)
		{
			m_Image = new TextureImage(texture);

			Width = texture.width;
			Height = texture.height;

			DoubleBuffered = true;
			ShowInTaskbar = false;
			TopMost = false;
			//TopLevel = false;

			FormBorderStyle = FormBorderStyle.None;
			TransparencyKey = System.Drawing.Color.FromArgb(0, 0, 0, 0);
			BackgroundImage = m_Image.Bitmap;
			BackgroundImageLayout = ImageLayout.None;
			ClientSize = m_Image.Bitmap.Size;
			BackColor = System.Drawing.Color.FromArgb(0, 0, 0);

			m_Refresh = (success) =>
			{
				if (success)
					Refresh();
			};
		}

		protected override void Dispose(bool disposing)
		{
			m_Image.Dispose();
			base.Dispose(disposing);
		}

		public void SetTexture(Texture texture)
		{
			m_Image?.Dispose();
			m_Image = new TextureImage(texture);
			BackgroundImage = m_Image.Bitmap;
			Width = texture.width;
			Height = texture.height;
			ClientSize = m_Image.Bitmap.Size;
		}

		public void UpdateTexture()
		{
			m_Image.Update();
			Refresh();
		}

		public void UpdateTextureAsync()
		{
			m_Image.UpdateAsync(m_Refresh);
		}

		public void SetPosition(Vector2Int position)
		{
			m_IsDragging = false;
			m_Relative = false;
			Location = new Point(position.x, position.y);
		}

		public void SetRelativePosition(Vector2Int pos)
		{
			m_IsDragging = false;
			m_Relative = true;
			m_RelativePosition = pos;
			m_PrevCurrentWindowRect = Util.GetCurrentWindowRect();
			SetRelativePositionImpl();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (m_Relative)
			{
				var rect = Util.GetCurrentWindowRect();
				if (!m_PrevCurrentWindowRect.Equals(rect))
				{
					m_PrevCurrentWindowRect = rect;
					SetRelativePositionImpl();
				}
			}
		}

		void SetRelativePositionImpl()
		{
			Location = new Point(m_PrevCurrentWindowRect.x + m_RelativePosition.x, m_PrevCurrentWindowRect.y + m_RelativePosition.y);
		}

		public void SetDragable(bool on)
		{
			if (m_Dragable == on)
			{
				return;
			}
			m_Dragable = on;
			if (on)
			{
				MouseDown += OnMouseDown;
				MouseMove += OnMouseMove;
				MouseUp += OnMouseUp;
			}
			else
			{
				MouseDown -= OnMouseDown;
				MouseMove -= OnMouseMove;
				MouseUp -= OnMouseUp;
			}
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_IsDragging = true;
				m_Offset = e.Location;
			}
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (m_IsDragging)
			{
				Point newLocation = this.PointToScreen(new Point(e.X, e.Y));
				newLocation.Offset(-m_Offset.X, -m_Offset.Y);
				Location = newLocation;
			}
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			m_IsDragging = false;
		}

	}

}