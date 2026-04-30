using CityGen.Data;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CityGen.Core
{
    public partial class CityGenerator
    {
        private const string DefaultProjectBlockoutMaterialPath = "Assets/CityGen/Default.mat";

        [Header("Blockout")]
        [SerializeField]
        private bool useProBuilderForShell = true;

        [SerializeField]
        private bool useProBuilderForPublicRoute = true;

        [SerializeField]
        private bool useProBuilderForLandmarkArches = true;

        [SerializeField]
        private bool clearPreviousBlockoutBeforeGenerate = true;

        [SerializeField]
        private float shellThicknessMeters = 16f;

        [SerializeField]
        private float floorPlateInsetMeters = 10f;

        [SerializeField]
        private float publicRouteWidthMeters = 14f;

        [SerializeField]
        private float coreRouteWidthMeters = 8f;

        [SerializeField]
        private Material blockoutMaterialOverride;

        [System.NonSerialized]
        private Material runtimeResolvedBlockoutMaterial;

        [ContextMenu("Generate Blockout")]
        public void GenerateBlockout()
        {
            if (generatedContext == null)
            {
                GenerateCityContext();
            }

            if (generatedContext == null)
            {
                return;
            }

            if (clearPreviousBlockoutBeforeGenerate)
            {
                ClearBlockout();
            }

            generatedBlockoutRoot = EnsureBlockoutRoot();

            BuildCoreBlockout(generatedContext, generatedBlockoutRoot);
            BuildBandBlockout(generatedContext, generatedBlockoutRoot);
            BuildPublicRouteBlockout(generatedContext, generatedBlockoutRoot);
            BuildLandmarkBlockout(generatedContext, generatedBlockoutRoot);
        }

        [ContextMenu("Clear Blockout")]
        public void ClearBlockout()
        {
            if (generatedBlockoutRoot == null)
            {
                Transform existing = transform.Find("GeneratedCityBlockout");
                if (existing != null)
                {
                    generatedBlockoutRoot = existing;
                }
            }

            if (generatedBlockoutRoot == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedBlockoutRoot.gameObject);
            }
            else
            {
                DestroyImmediate(generatedBlockoutRoot.gameObject);
            }

            generatedBlockoutRoot = null;
        }

        private Transform EnsureBlockoutRoot()
        {
            Transform existing = transform.Find("GeneratedCityBlockout");
            if (existing != null)
            {
                return existing;
            }

            GameObject root = new GameObject("GeneratedCityBlockout");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root.transform;
        }

        private void BuildCoreBlockout(CityContext context, Transform root)
        {
            float totalHeight = context.Grid.TotalHeightMeters;
            float centerY = totalHeight * 0.5f;

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            core.name = "CoreShaft";
            core.transform.SetParent(root, false);
            core.transform.localPosition = new Vector3(0f, centerY, 0f);
            core.transform.localScale = new Vector3(
                context.Grid.CoreRadiusMeters * 2f,
                totalHeight * 0.5f,
                context.Grid.CoreRadiusMeters * 2f);
            ApplyBlockoutMaterial(core);

            GameObject hiddenRoute = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hiddenRoute.name = "HiddenRoute_CoreLift";
            hiddenRoute.transform.SetParent(root, false);
            hiddenRoute.transform.localPosition = new Vector3(0f, centerY, 0f);
            hiddenRoute.transform.localScale = new Vector3(
                coreRouteWidthMeters,
                totalHeight,
                coreRouteWidthMeters);
            ApplyBlockoutMaterial(hiddenRoute);
        }

        private void BuildBandBlockout(CityContext context, Transform root)
        {
            float bandHeight = GetBandHeight(context);
            float outerRadius = context.Grid.OuterRadiusMeters;
            float innerFloorRadius = Mathf.Max(context.Grid.CoreRadiusMeters + 8f, outerRadius - floorPlateInsetMeters);

            for (int i = 0; i < context.VerticalBands.Count; i++)
            {
                VerticalBandData band = context.VerticalBands[i];
                float centerY = bandHeight * (i + 0.5f);

                GameObject bandRoot = new GameObject(band.DisplayName);
                bandRoot.transform.SetParent(root, false);
                bandRoot.transform.localPosition = new Vector3(0f, centerY, 0f);

                GameObject shell = useProBuilderForShell
                    ? CreatePipe($"{band.Id}_Shell", outerRadius, bandHeight * 0.9f, shellThicknessMeters, bandRoot.transform)
                    : CreateCylinder($"{band.Id}_Shell", outerRadius, bandHeight * 0.9f, bandRoot.transform);

                shell.transform.localPosition = Vector3.zero;

                GameObject floorPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                floorPlate.name = $"{band.Id}_FloorPlate";
                floorPlate.transform.SetParent(bandRoot.transform, false);
                floorPlate.transform.localPosition = Vector3.zero;
                floorPlate.transform.localScale = new Vector3(
                    innerFloorRadius * 2f,
                    Mathf.Max(1f, bandHeight * 0.05f),
                    innerFloorRadius * 2f);
                ApplyBlockoutMaterial(floorPlate);
            }
        }

        private void BuildPublicRouteBlockout(CityContext context, Transform root)
        {
            float bandHeight = GetBandHeight(context);
            float stairInnerRadius = Mathf.Max(context.Grid.CoreRadiusMeters + 24f, context.Grid.OuterRadiusMeters * 0.68f);

            for (int i = 0; i < context.VerticalBands.Count; i++)
            {
                float angle = i * 28f;
                float centerY = bandHeight * (i + 0.5f);
                GameObject routeSegment;

                if (useProBuilderForPublicRoute)
                {
                    routeSegment = ShapeGenerator.GenerateCurvedStair(
                        PivotLocation.Center,
                        publicRouteWidthMeters,
                        bandHeight * 0.85f,
                        stairInnerRadius,
                        140f,
                        Mathf.Max(4, Mathf.RoundToInt(bandHeight / 6f)),
                        true).gameObject;
                }
                else
                {
                    routeSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    routeSegment.transform.localScale = new Vector3(
                        publicRouteWidthMeters,
                        bandHeight * 0.25f,
                        stairInnerRadius * 1.25f);
                }

                routeSegment.name = $"PublicRoute_{i:00}";
                routeSegment.transform.SetParent(root, false);
                routeSegment.transform.localPosition = new Vector3(0f, centerY, 0f);
                routeSegment.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                ApplyBlockoutMaterial(routeSegment);
            }
        }

        private void BuildLandmarkBlockout(CityContext context, Transform root)
        {
            for (int i = 0; i < context.Landmarks.Count; i++)
            {
                LandmarkNode landmark = context.Landmarks[i];

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = landmark.DisplayName;
                marker.transform.SetParent(root, false);
                marker.transform.localPosition = landmark.ApproximateLocalPosition;
                marker.transform.localScale = new Vector3(18f, 18f, 18f);
                ApplyBlockoutMaterial(marker);

                if (!useProBuilderForLandmarkArches)
                {
                    continue;
                }

                GameObject arch = ShapeGenerator.GenerateArch(
                    PivotLocation.Center,
                    180f,
                    14f,
                    6f,
                    4f,
                    10,
                    true,
                    true,
                    true,
                    true,
                    true).gameObject;

                arch.name = $"{landmark.DisplayName}_Arch";
                arch.transform.SetParent(marker.transform, false);
                arch.transform.localPosition = new Vector3(0f, 12f, 0f);
                arch.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                ApplyBlockoutMaterial(arch);
            }
        }

        private static float GetBandHeight(CityContext context)
        {
            return context.Grid.TotalHeightMeters / Mathf.Max(1, context.VerticalBands.Count);
        }

        private static GameObject CreateCylinder(string name, float radius, float height, Transform parent)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            return cylinder;
        }

        private static GameObject CreatePipe(string name, float radius, float height, float thickness, Transform parent)
        {
            ProBuilderMesh pipe = ShapeGenerator.GeneratePipe(
                PivotLocation.Center,
                radius,
                height,
                Mathf.Clamp(thickness, 1f, radius * 0.8f),
                16,
                1);
            pipe.gameObject.name = name;
            pipe.transform.SetParent(parent, false);
            return pipe.gameObject;
        }

        private void ApplyBlockoutMaterial(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Material material = ResolveBlockoutMaterial();
            if (material == null)
            {
                return;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderer.sharedMaterial = material;
                    continue;
                }

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private Material ResolveBlockoutMaterial()
        {
            if (blockoutMaterialOverride != null)
            {
                return blockoutMaterialOverride;
            }

            Material projectDefault = LoadProjectDefaultBlockoutMaterial();
            if (projectDefault != null)
            {
                return projectDefault;
            }

            if (runtimeResolvedBlockoutMaterial != null)
            {
                return runtimeResolvedBlockoutMaterial;
            }

            if (GraphicsSettings.currentRenderPipeline != null && GraphicsSettings.currentRenderPipeline.defaultMaterial != null)
            {
                runtimeResolvedBlockoutMaterial = GraphicsSettings.currentRenderPipeline.defaultMaterial;
                return runtimeResolvedBlockoutMaterial;
            }

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null)
            {
                runtimeResolvedBlockoutMaterial = new Material(urpLit);
                runtimeResolvedBlockoutMaterial.name = "GeneratedBlockout_URPLit";
                return runtimeResolvedBlockoutMaterial;
            }

            Shader standard = Shader.Find("Standard");
            if (standard != null)
            {
                runtimeResolvedBlockoutMaterial = new Material(standard);
                runtimeResolvedBlockoutMaterial.name = "GeneratedBlockout_Standard";
                return runtimeResolvedBlockoutMaterial;
            }

            return null;
        }

        private Material LoadProjectDefaultBlockoutMaterial()
        {
#if UNITY_EDITOR
            Material directMatch = AssetDatabase.LoadAssetAtPath<Material>(DefaultProjectBlockoutMaterialPath);
            if (directMatch != null)
            {
                return directMatch;
            }

            string[] guids = AssetDatabase.FindAssets("Default t:Material", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Material candidate = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (candidate != null)
                {
                    return candidate;
                }
            }
#endif

            return null;
        }
    }
}
