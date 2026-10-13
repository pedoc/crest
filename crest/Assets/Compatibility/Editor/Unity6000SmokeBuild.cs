// MIT. Reproducible editor-to-Windows build smoke without removing optional packages.
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Crest.Compatibility.Editor
{
 public static class Unity6000SmokeBuild
 {
  public static void BuildWindows()
  {
   string oldName=PlayerSettings.productName;int oldWidth=PlayerSettings.defaultScreenWidth,oldHeight=PlayerSettings.defaultScreenHeight;var oldMode=PlayerSettings.fullScreenMode;
   try
   {
   GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.productName="Crest Unity 6000.6 compatibility smoke";PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;
   PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
   const string root="Assets/Compatibility/Generated";Directory.CreateDirectory(root);AssetDatabase.Refresh();
   var terrainMaterial=AssetDatabase.LoadAssetAtPath<Material>(root+"/Terrain.mat");if(terrainMaterial==null){terrainMaterial=new Material(Shader.Find("Nature/Terrain/Standard"));AssetDatabase.CreateAsset(terrainMaterial,root+"/Terrain.mat");}
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.4f,.46f,.52f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.62f,.70f,.75f);RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=90;RenderSettings.fogEndDistance=400;
   var sun=new GameObject("Smoke sunlight").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.transform.rotation=Quaternion.Euler(40,-30,0);sun.shadows=LightShadows.Soft;RenderSettings.sun=sun;
   var cam=new GameObject("Smoke camera").AddComponent<Camera>();cam.tag="MainCamera";cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=RenderSettings.fogColor;cam.nearClipPlane=.15f;cam.farClipPlane=800;cam.fieldOfView=48;cam.allowHDR=true;cam.depthTextureMode=DepthTextureMode.Depth;
   var markerData=AssetDatabase.LoadAssetAtPath<TerrainData>(root+"/TerrainMarker.asset");if(markerData==null){markerData=new TerrainData {heightmapResolution=33,size=new Vector3(2,1,2)};AssetDatabase.CreateAsset(markerData,root+"/TerrainMarker.asset");}
   var marker=Terrain.CreateTerrainGameObject(markerData);marker.transform.position=Vector3.down*500;marker.GetComponent<Terrain>().materialTemplate=terrainMaterial;
   var obj=new GameObject("Upstream Crest Ocean");obj.SetActive(false);var ocean=obj.AddComponent<OceanRenderer>();ocean.OceanMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Crest/Crest/Materials/Ocean.mat");ocean.Viewpoint=cam.transform;ocean._globalWindSpeed=15;
   var so=new SerializedObject(ocean);so.FindProperty("_lodDataResolution").intValue=256;so.FindProperty("_lodCount").intValue=6;so.FindProperty("_followSceneCamera").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();
   var waves=new GameObject("Crest FFT");waves.transform.SetParent(obj.transform,false);var fft=waves.AddComponent<ShapeFFT>();fft._resolution=128;fft._weight=.65f;
   var ctrl=new GameObject("Compatibility smoke controller").AddComponent<Unity6000Smoke>();ctrl.oceanObject=obj;ctrl.view=cam;ctrl.terrainMaterial=terrainMaterial;
   string scenePath=root+"/Smoke.unity";EditorSceneManager.SaveScene(scene,scenePath);AssetDatabase.SaveAssets();
   string path=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..","builds","unity6000-smoke","Crest.Unity6000Smoke.exe"));Directory.CreateDirectory(Path.GetDirectoryName(path));
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{scenePath},target=BuildTarget.StandaloneWindows64,locationPathName=path,options=BuildOptions.Development});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Unity 6000.6 standalone smoke build failed: "+report.summary.result);
   }
   finally
   {PlayerSettings.productName=oldName;PlayerSettings.defaultScreenWidth=oldWidth;PlayerSettings.defaultScreenHeight=oldHeight;PlayerSettings.fullScreenMode=oldMode;}
  }
 }
}
