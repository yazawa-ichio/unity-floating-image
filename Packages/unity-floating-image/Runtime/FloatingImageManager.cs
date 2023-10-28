using System.Collections.Generic;
using UnityEngine;

namespace UnityFloatingImage
{
	class FloatingImageManager : MonoBehaviour
	{
		static FloatingImageManager s_Instance;

		[RuntimeInitializeOnLoadMethod]
		static void Init()
		{
			s_Instance = new GameObject("FloatingImageManager").AddComponent<FloatingImageManager>();
			GameObject.DontDestroyOnLoad(s_Instance.gameObject);
		}

		public static void Add(FloatingImage image)
		{
			s_Instance.m_Images.Add(image);
		}

		public static void Remove(FloatingImage image)
		{
			s_Instance.m_Remove.Enqueue(image);
		}

		List<FloatingImage> m_Images = new();
		Queue<FloatingImage> m_Remove = new();

		void LateUpdate()
		{
			foreach (var image in m_Images)
			{
				image.DoUpdate();
			}
			while (m_Remove.Count > 0)
			{
				var image = m_Remove.Dequeue();
				m_Images.Remove(image);
			}
		}

		void OnDestroy()
		{
			foreach (var image in m_Images.ToArray())
			{
				image.Dispose();
			}
			while (m_Remove.Count > 0)
			{
				var image = m_Remove.Dequeue();
				m_Images.Remove(image);
			}
			m_Images.Clear();
		}

		private void OnApplicationFocus(bool focus)
		{
			//if (focus)
			{
				foreach (var image in m_Images)
				{
					image.OnFocus();
				}
			}
		}
	}
}