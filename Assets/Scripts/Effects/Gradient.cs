using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Effects/Gradient")]
#if UNITY_5_2
    public class Gradient : BaseMeshEffect {
#else
        public class Gradient:BaseVertexEffect
#endif
        [SerializeField] private Color topColor = Color.white;
		[SerializeField] private Color bottomColor = Color.black;

#if UNITY_5_2
        public override void ModifyMesh(Mesh mesh)
        {
            if (!this.IsActive())
                return;

            List<UIVertex> list = new List<UIVertex>();
            using (VertexHelper vertexHelper = new VertexHelper(mesh))
            {
                vertexHelper.GetUIVertexStream(list);
            }

            ModifyVertices(list);

            using (VertexHelper vertexHelper2 = new VertexHelper())
            {
                vertexHelper2.AddUIVertexTriangleStream(list);
                vertexHelper2.FillMesh(mesh);
            }
        }
#endif
        public void ModifyVertices(List<UIVertex> vertexList)
		{
            if (!this.IsActive())
                return;

            int count = vertexList.Count;
            float bottomY = vertexList[0].position.y;
            float topY = vertexList[0].position.y;

            for (int i = 1; i < count; i++)
            {
                float y = vertexList[i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }

            float uiElementHeight = topY - bottomY;

            for (int i = 0; i < count; i++)
            {
                UIVertex uiVertex = vertexList[i];
                uiVertex.color = uiVertex.color * Color.Lerp(bottomColor, topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
                vertexList[i] = uiVertex;
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            throw new NotImplementedException();
        }
    }
}