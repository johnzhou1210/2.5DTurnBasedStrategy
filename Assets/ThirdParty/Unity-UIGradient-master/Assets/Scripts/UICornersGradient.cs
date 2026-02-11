using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/4 Corners Gradient")]
public class UICornersGradient : BaseMeshEffect {
	[FormerlySerializedAs("m_topLeftColor")] public Color mTopLeftColor = Color.white;
	[FormerlySerializedAs("m_topRightColor")] public Color mTopRightColor = Color.white;
	[FormerlySerializedAs("m_bottomRightColor")] public Color mBottomRightColor = Color.white;
	[FormerlySerializedAs("m_bottomLeftColor")] public Color mBottomLeftColor = Color.white;

    public override void ModifyMesh(VertexHelper vh)
    {
		if(enabled)
		{
            Rect rect = graphic.rectTransform.rect;
			UIGradientUtils.Matrix2X3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, Vector2.right);

			UIVertex vertex = default(UIVertex);
			for (int i = 0; i < vh.currentVertCount; i++) {
				vh.PopulateUIVertex (ref vertex, i);
				Vector2 normalizedPosition = localPositionMatrix * vertex.position;
				vertex.color *= UIGradientUtils.Bilerp(mBottomLeftColor, mBottomRightColor, mTopLeftColor, mTopRightColor, normalizedPosition);
				vh.SetUIVertex (vertex, i);
			}
		}
    }
}
