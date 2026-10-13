// MIT. Runtime smoke coverage for this fork's Unity 6000.6 compatibility branch.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
namespace Crest.Compatibility
{
 [Serializable] public class SmokeResult
 {
  public string unityVersion,renderPipeline="Built-in",checkedAt,scope="Windows D3D11 standalone FFT/depth smoke; not all example or XR certification";
  public bool passed,oceanActive,fftActive,terrainShaderSupported,depthActive;
  public float sampledHeightRange,fps;public string[] errors,screenshots;
 }
 public class Unity6000Smoke:MonoBehaviour
 {
  public GameObject oceanObject;public Camera view;public Material terrainMaterial;
  OceanRenderer ocean;ShapeFFT fft;OceanDepthCache depth;float fps;bool automatic;
  readonly List<string> errors=new List<string>();
  void Awake(){automatic=Array.IndexOf(Environment.GetCommandLineArgs(),"-crestCompatibilityAutoTest")>=0;Application.runInBackground=automatic;Application.targetFrameRate=30;QualitySettings.vSyncCount=0;Application.logMessageReceived+=CaptureError;}
  void CaptureError(string message,string trace,LogType type){if((type==LogType.Error||type==LogType.Exception)&&!errors.Contains(message))errors.Add(message);}
  void Start()
  {
   var data=new TerrainData {heightmapResolution=129,size=new Vector3(80,16,80)};var map=new float[129,129];
   for(int z=0;z<129;z++)for(int x=0;x<129;x++){float wx=(x/128f-.5f)*80,wz=(z/128f-.5f)*80;float h=2.2f*Mathf.Exp(-((wx+11)*(wx+11)/310+wz*wz/280))-1.1f;map[z,x]=(h+4)/16;}data.SetHeights(0,0,map);
   var albedo=new Texture2D(2,2,TextureFormat.RGB24,false);albedo.SetPixels(new[]{new Color(.32f,.43f,.24f),new Color(.38f,.47f,.27f),new Color(.34f,.44f,.25f),new Color(.39f,.48f,.28f)});albedo.Apply();
   data.terrainLayers=new[]{new TerrainLayer {diffuseTexture=albedo,tileSize=new Vector2(5,5)}};
   var terrain=Terrain.CreateTerrainGameObject(data);terrain.transform.position=new Vector3(-40,-4,-40);terrain.GetComponent<Terrain>().materialTemplate=terrainMaterial;
   ocean=oceanObject.GetComponent<OceanRenderer>();ocean.Viewpoint=view.transform;fft=oceanObject.GetComponentInChildren<ShapeFFT>(true);oceanObject.SetActive(true);
   var cache=new GameObject("Smoke terrain depth cache");cache.transform.localScale=new Vector3(90,1,90);depth=cache.AddComponent<OceanDepthCache>();depth._layers=1;depth._resolution=256;
   view.transform.position=new Vector3(18,8,-24);view.transform.LookAt(new Vector3(0,0,-6));if(automatic)StartCoroutine(Verify());
  }
  void Update(){fps=Mathf.Lerp(fps,1/Mathf.Max(.001f,Time.unscaledDeltaTime),.04f);}
  IEnumerator Verify()
  {
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-crestCompatibilityOutput");string dir=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.dataPath,"..","CompatibilityCaptures");Directory.CreateDirectory(dir);
   yield return new WaitForSecondsRealtime(7);float low=float.MaxValue,high=float.MinValue;var helper=new SampleHeightHelper();
   for(int i=0;i<120;i++){helper.Init(new Vector3(12,0,-10),1,true);float h;if(helper.Sample(out h)){low=Mathf.Min(low,h);high=Mathf.Max(high,h);}yield return null;}
   string shot=Path.Combine(dir,"unity6000-crest-smoke.png");yield return new WaitForEndOfFrame();
   // Capture the actual camera through a render target, so a splash/occluded window cannot
   // falsely produce a blank screenshot while the GPU wave-query smoke still passes.
   var previous=view.targetTexture;var rt=new RenderTexture(1280,800,24,RenderTextureFormat.ARGB32);rt.Create();view.targetTexture=rt;view.Render();
   var oldActive=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();File.WriteAllBytes(shot,image.EncodeToPNG());
   RenderTexture.active=oldActive;view.targetTexture=previous;rt.Release();Destroy(rt);Destroy(image);yield return null;
   var result=new SmokeResult {unityVersion=Application.unityVersion,checkedAt=DateTimeOffset.Now.ToString("o"),oceanActive=OceanRenderer.Instance!=null,fftActive=fft!=null&&fft.isActiveAndEnabled,depthActive=depth!=null&&depth.enabled,terrainShaderSupported=terrainMaterial!=null&&terrainMaterial.shader.isSupported,sampledHeightRange=high>=low?high-low:0,fps=fps,errors=errors.ToArray(),screenshots=new[]{Path.GetFileName(shot)}};
   result.passed=result.oceanActive&&result.fftActive&&result.depthActive&&result.terrainShaderSupported&&result.sampledHeightRange>.001f&&errors.Count==0;
   File.WriteAllText(Path.Combine(dir,"runtime-verification.json"),JsonUtility.ToJson(result,true));Debug.Log("CREST_UNITY6000_SMOKE passed="+result.passed);Application.Quit(result.passed?0:1);
  }
  void OnDestroy(){Application.logMessageReceived-=CaptureError;}
 }
}
