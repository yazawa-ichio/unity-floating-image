using UnityEngine;

namespace UnityFloatingImage.Sample
{

	public class WindowSample : MonoBehaviour
	{
		[SerializeField]
		RenderTexture m_RenderTexture;

		FloatingImage m_FloatingImage;

		[SerializeField]
		Transform m_TestObject;

		void Start()
		{
			m_FloatingImage = new FloatingImage(m_RenderTexture);
			m_FloatingImage.IsDragable = true;
			m_FloatingImage.ZOrder = ZOrder.Top;
		}

		void OnDestroy()
		{
			m_FloatingImage.Dispose();
		}

		// Update is called once per frame
		void Update()
		{
			m_TestObject.Rotate(Vector3.up, Time.deltaTime * 20);
		}
	}

}