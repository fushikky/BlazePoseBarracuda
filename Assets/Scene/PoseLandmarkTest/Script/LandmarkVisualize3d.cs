using UnityEngine;
using UnityEngine.UI;
using MediaPipe.PoseLandmark;

public class LandmarkVisualize3d : MonoBehaviour
{
   [SerializeField] SourceInput _Input;
    [SerializeField] RawImage _Image;
    [Space]
    [SerializeField] ResourceSet _Resources;
    [SerializeField] Shader _Shader;
    [Space]
    [SerializeField] bool _UpperBodyOnly = true;
    [SerializeField] bool _DisplaySegmentation = true;

    PoseLandmarkDetector _Detector;
    Material _Material;

    [SerializeField] RawImage _debugPlane;
    [SerializeField] RawImage _debugPlane2;
    [SerializeField] GameObject _debugSphere;
    [SerializeField] GameObject _debugSphere2;
    [SerializeField] float z;

    ComputeBuffer _KeyPointArgs;
    
    int width = Screen.width;
    int height = Screen.height;
 
    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        _Detector = new PoseLandmarkDetector(_Resources, _UpperBodyOnly);
        _Material = new Material(_Shader);

        var cbType = ComputeBufferType.IndirectArguments;
        _KeyPointArgs = new ComputeBuffer(4, sizeof(uint), cbType);
        _KeyPointArgs.SetData(new [] {_Detector.VertexCount * 4, 1, 0, 0});
    }

    void OnDestroy(

    )
    {
        _Detector.Dispose();
        Destroy(_Material);

        _KeyPointArgs.Dispose();
    }

    void LateUpdate()
    {
        _Detector.ProcessImage(_Input.SourceTexture);
        _Image.texture = 
           _DisplaySegmentation ? _Detector.SegmentationBuffer : _Input.SourceTexture;

        Vector2 rightHand = _Detector.GetKeyPoint(PoseLandmarkDetector.KeyPoint.RightWrist);
        Vector2 leftHand = _Detector.GetKeyPoint(PoseLandmarkDetector.KeyPoint.LeftWrist);

        _debugPlane.GetComponent<RectTransform>().anchoredPosition = new Vector3(rightHand.x * width, rightHand.y * height, 0);
        _debugPlane2.GetComponent<RectTransform>().anchoredPosition = new Vector3(leftHand.x * width, leftHand.y * height, 0);

        Vector3 rightHand3d = Camera.main.ScreenToWorldPoint(new Vector3(rightHand.x * width, rightHand.y * height, z));
        Vector3 leftHand3d = Camera.main.ScreenToWorldPoint(new Vector3(leftHand.x * width, leftHand.y * height, z));

        _debugSphere.transform.position = rightHand3d;
        _debugSphere2.transform.position = leftHand3d;
    }

    void OnRenderObject()
    {
        _Material.SetBuffer("_KeyPoints", _Detector.OutputBuffer);
        _Material.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Lines, _KeyPointArgs, 0);
    }
}
