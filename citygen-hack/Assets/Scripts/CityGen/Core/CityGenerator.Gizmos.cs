using CityGen.Data;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CityGen.Core
{
    public partial class CityGenerator
    {
        [Header("Gizmos")]
        [SerializeField]
        private bool drawCityGizmos = true;

        [SerializeField]
        private bool drawBandLabels = true;

        [SerializeField]
        private bool drawLandmarkLabels = true;

        [SerializeField]
        private float landmarkGizmoRadius = 8f;

        private void OnDrawGizmosSelected()
        {
            if (!drawCityGizmos || generatedContext == null || generatedContext.Grid == null || generatedContext.VerticalBands == null)
            {
                return;
            }

            DrawBandGizmos(generatedContext);
            DrawLandmarkGizmos(generatedContext);
        }

        private void DrawBandGizmos(CityContext context)
        {
            float bandHeight = GetBandHeight(context);
            float radius = context.Grid.OuterRadiusMeters;

            for (int i = 0; i < context.VerticalBands.Count; i++)
            {
                VerticalBandData band = context.VerticalBands[i];
                float minY = bandHeight * i;
                float maxY = bandHeight * (i + 1);
                float centerY = (minY + maxY) * 0.5f;
                Color color = Color.HSVToRGB((i * 0.11f) % 1f, 0.6f, 0.9f);

                Gizmos.color = color;
                DrawWireCylinder(transform.position + new Vector3(0f, centerY, 0f), radius, bandHeight * 0.9f);

#if UNITY_EDITOR
                if (drawBandLabels)
                {
                    Handles.color = color;
                    Handles.Label(
                        transform.position + new Vector3(radius + 12f, centerY, 0f),
                        $"{band.DisplayName} [{band.DefaultAccessLevel}]");
                }
#endif
            }
        }

        private void DrawLandmarkGizmos(CityContext context)
        {
            if (context.Landmarks == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;

            for (int i = 0; i < context.Landmarks.Count; i++)
            {
                LandmarkNode landmark = context.Landmarks[i];
                Vector3 worldPosition = transform.TransformPoint(landmark.ApproximateLocalPosition);
                Gizmos.DrawSphere(worldPosition, landmarkGizmoRadius);

#if UNITY_EDITOR
                if (drawLandmarkLabels)
                {
                    Handles.color = Color.yellow;
                    Handles.Label(worldPosition + Vector3.up * (landmarkGizmoRadius + 4f), landmark.DisplayName);
                }
#endif
            }
        }

        private static void DrawWireCylinder(Vector3 center, float radius, float height)
        {
            float halfHeight = height * 0.5f;
            Vector3 top = center + Vector3.up * halfHeight;
            Vector3 bottom = center - Vector3.up * halfHeight;

#if UNITY_EDITOR
            Handles.DrawWireDisc(top, Vector3.up, radius);
            Handles.DrawWireDisc(bottom, Vector3.up, radius);
#endif

            Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
            Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
            Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
            Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        }
    }
}
