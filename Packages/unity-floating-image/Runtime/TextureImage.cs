using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFloatingImage
{
	public class TextureImage : IDisposable
	{
		public Bitmap Bitmap => m_Bitmap;

		ComputeShader m_Shader;
		Texture m_Texture;
		int m_Kernel;
		uint[] m_Color;

		Bitmap m_Bitmap;
		ComputeBuffer m_Buffer;
		int m_ThreadGroupSizeX;
		int m_ThreadGroupSizeY;
		int m_ThreadGroupSizeZ;
		Action<AsyncGPUReadbackRequest> m_OnGPUReadback;
		Queue<Action<bool>> m_CompleteQueue = new();
		bool m_Disposed;


		public TextureImage(Texture texture)
		{
			m_Texture = texture;
			m_Bitmap = new Bitmap(texture.width, texture.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			m_Buffer = new ComputeBuffer(texture.width * texture.height, sizeof(uint));
			m_Color = new uint[texture.width * texture.height];

			m_Shader = ComputeShader.Instantiate(Resources.Load<ComputeShader>("FloatingImage/BitCopy"));
			m_Kernel = m_Shader.FindKernel("CSMain");
			m_Shader.SetTexture(m_Kernel, "Input", m_Texture);
			m_Shader.SetBuffer(m_Kernel, "Result", m_Buffer);

			m_Shader.GetKernelThreadGroupSizes(m_Kernel, out var x, out var y, out var z);
			m_ThreadGroupSizeX = Mathf.CeilToInt(m_Texture.width / x);
			m_ThreadGroupSizeY = Mathf.CeilToInt(m_Texture.height / y);
			m_ThreadGroupSizeZ = Mathf.CeilToInt(z);

			m_OnGPUReadback = OnGPUReadback;
		}

		public unsafe void Update()
		{
			if (m_Disposed) return;

			m_Shader.Dispatch(m_Kernel, m_ThreadGroupSizeX, m_ThreadGroupSizeY, m_ThreadGroupSizeZ);
			m_Buffer.GetData(m_Color);

			var bitmap = m_Bitmap;
			var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
			var dst = data.Scan0;
			Span<uint> buf = new(dst.ToPointer(), bitmap.Width * bitmap.Height);
			m_Color.CopyTo(buf);

			bitmap.UnlockBits(data);
			bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
		}

		public void UpdateAsync(Action<bool> complate)
		{
			if (m_Disposed) return;

			m_CompleteQueue.Enqueue(complate);
			m_Shader.Dispatch(m_Kernel, m_ThreadGroupSizeX, m_ThreadGroupSizeY, m_ThreadGroupSizeZ);

			AsyncGPUReadback.Request(m_Buffer, m_OnGPUReadback);
		}

		unsafe void OnGPUReadback(AsyncGPUReadbackRequest request)
		{
			if (m_Disposed)
			{
				m_CompleteQueue.Clear();
				return;
			}

			if (request.hasError)
			{
				while (m_CompleteQueue.Count > 0)
				{
					m_CompleteQueue.Dequeue()(false);
				}
				return;
			}

			request.GetData<uint>().CopyTo(m_Color);

			var bitmap = m_Bitmap;
			var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
			var dst = data.Scan0;
			Span<uint> buf = new(dst.ToPointer(), bitmap.Width * bitmap.Height);
			m_Color.CopyTo(buf);

			bitmap.UnlockBits(data);
			bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

			while (m_CompleteQueue.Count > 0)
			{
				m_CompleteQueue.Dequeue()(true);
			}
		}

		public void Dispose()
		{
			m_Disposed = true;
			m_Bitmap.Dispose();
			m_Buffer.Dispose();
		}

	}
}