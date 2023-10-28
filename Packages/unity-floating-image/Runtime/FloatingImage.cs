using System;
using UnityEngine;

namespace UnityFloatingImage
{
	public enum ZOrder
	{
		None = 0,
		Bottom = 1,
		Top = 2,
		TopMost = 3,
	}

	public class FloatingImage : IDisposable
	{

		FloatingImageForm m_Form;
		bool m_IsDragable;
		ZOrder m_ZOrder;
		bool m_Disposed;
		Texture m_Texture;

		public bool IsDragable
		{
			get => m_IsDragable;
			set => m_Form.SetDragable(m_IsDragable = value);
		}

		public bool AutoUpdate { get; set; } = true;

		public Vector2Int DisplayPosition
		{
			get => new(m_Form.Location.X, m_Form.Location.Y);
			set => m_Form.SetPosition(value);
		}

		public ZOrder ZOrder
		{
			get => m_ZOrder;
			set
			{
				if (value == m_ZOrder)
				{
					return;
				}
				m_ZOrder = value;
				m_Form.TopMost = value == ZOrder.TopMost;
				UpdateOrder();
			}
		}

		public FloatingImage(Texture texture)
		{
			m_Texture = texture;
			m_Form = new FloatingImageForm(texture);
			m_Form.Show();
			UpdateOrder();
			FloatingImageManager.Add(this);
		}

		public void SetRelativePosition(Vector2Int pos)
		{
			m_Form.SetRelativePosition(pos);
		}

		internal void DoUpdate()
		{
			if (m_Disposed) return;
			if (!m_Texture)
			{
				Dispose();
				return;
			}
			if (AutoUpdate)
			{
				m_Form.UpdateTextureAsync();
			}
		}

		void UpdateOrder()
		{
			if (m_Disposed) return;
			switch (m_ZOrder)
			{
				case ZOrder.Bottom:
					{
						var handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
						Util.SetWindowPos(m_Form.Handle, handle, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010);
					}
					break;
				case ZOrder.Top:
					{
						Util.SetWindowPos(m_Form.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010);
						var handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
						Util.SetWindowPos(handle, m_Form.Handle, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010);
					}
					break;
			}
		}

		public void UpdateSync()
		{
			m_Form.UpdateTexture();
		}

		public void Update()
		{
			m_Form.UpdateTextureAsync();
		}

		public void SetTexture(Texture texture)
		{
			m_Form.SetTexture(texture);
		}

		public void Dispose()
		{
			if (m_Disposed) return;
			m_Disposed = true;
			m_Form?.Dispose();
			FloatingImageManager.Remove(this);
		}

		internal void OnFocus()
		{
			UpdateOrder();
		}

	}
}