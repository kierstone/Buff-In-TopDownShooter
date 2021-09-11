using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///圆形3d ui的控制器，用于脚下血环之类的东西，网上抄来的代码，知识不需要钱，拿来就是了
///</summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PieChartController : MonoBehaviour{
    public float radius = 2;
    [Range(0,360)]//把角度限制在0-360
    public float startAngleDegree = 0;
    [Range(0,360)]//把角度限制在0-360
    public float angleDegree = 100;

    public int angleDegreePrecision = 1000;
    public int radiusPrecision = 1000;

    private MeshFilter meshFilter;

    private SectorMeshCreator creator = new SectorMeshCreator();

    [ExecuteInEditMode]
    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        meshFilter.mesh = creator.CreateMesh(radius, startAngleDegree, angleDegree, angleDegreePrecision, radiusPrecision);
    }

    //在Scene界面画辅助线
    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        DrawMesh();
    }

    //在Scene界面画辅助线
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        DrawMesh();
    }

    private void DrawMesh()
    {
        Mesh mesh = creator.CreateMesh(radius, startAngleDegree, angleDegree, angleDegreePrecision, radiusPrecision);
        int[] tris = mesh.triangles;
        for (int i = 0; i < tris.Length; i += 3)
        {
            Gizmos.DrawLine(convert2World(mesh.vertices[tris[i]]), convert2World(mesh.vertices[tris[i + 1]]));
            Gizmos.DrawLine(convert2World(mesh.vertices[tris[i]]), convert2World(mesh.vertices[tris[i + 2]]));
            Gizmos.DrawLine(convert2World(mesh.vertices[tris[i + 1]]), convert2World(mesh.vertices[tris[i + 2]]));
        }
    }

    private Vector3 convert2World(Vector3 src)
    {
        return transform.TransformPoint(src);
    }

    private class SectorMeshCreator
    {
        private float radius;
        private float startAngleDegree;
        private float angleDegree;

        private Mesh cacheMesh;

        /// <summary>  
        /// 创建一个扇形Mesh  
        /// </summary>  
        /// <param name="radius">扇形半径</param>  
        /// <param name="startAngleDegree">扇形开始角度</param> 
        /// <param name="angleDegree">扇形角度</param>  
        /// <param name="angleDegreePrecision">扇形角度精度（在满足精度范围内，认为是同个角度）</param>  
        /// <param name="radiusPrecision">  
        /// <pre>  
        /// 扇形半价精度（在满足半价精度范围内，被认为是同个半价）。  
        /// 比如：半价精度为1000，则：1.001和1.002不被认为是同个半径。因为放大1000倍之后不相等。  
        /// 如果半价精度设置为100，则1.001和1.002可认为是相等的。  
        /// </pre>  
        /// </param>  
        /// <returns></returns>  
        public Mesh CreateMesh(float radius, float startAngleDegree, float angleDegree, int angleDegreePrecision, int radiusPrecision)
        {
            if (checkDiff(radius, startAngleDegree, angleDegree, angleDegreePrecision, radiusPrecision))
            {//参数有改变才需要重新画mesh
                Mesh newMesh = Create(radius, startAngleDegree, angleDegree);
                if (newMesh != null)
                {
                    cacheMesh = newMesh;
                    this.radius = radius;
                    this.startAngleDegree = startAngleDegree;
                    this.angleDegree = angleDegree;
                }
            }
            return cacheMesh;
        }

        private Vector3 CalcPoint(float angle)
        {//这个函数计算了非特殊角度射线在正方形上的点
            angle = angle % 360;
            if (angle == 0) {
                return new Vector3 (1, 0, 0);
            } else if (angle == 180) {
                return new Vector3(-1,0,0);
            }
            //这里分别对应这个射线处于上图哪个三角形中, 分别计算
            if (angle <= 45 || angle > 315) {
                return new Vector3(1,Mathf.Tan(Mathf.Deg2Rad*angle), 0);
            } else if (angle <= 135) {
                return new Vector3(1/Mathf.Tan(Mathf.Deg2Rad*angle), 1, 0);
            } else if (angle <= 225) {
                return new Vector3(-1, -Mathf.Tan(Mathf.Deg2Rad*angle), 0);
            } else {
                return new Vector3(-1/Mathf.Tan(Mathf.Deg2Rad*angle), -1, 0);
            }
        }

        private Mesh Create(float radius, float startAngleDegree, float angleDegree)
        {
            if (startAngleDegree == 360) {
                startAngleDegree = 0;
            }
            Mesh mesh = new Mesh();
            List<Vector3> calcVertices = new List<Vector3> ();
            calcVertices.Add (Vector3.zero);//第一个点是圆心点
            calcVertices.Add (CalcPoint (startAngleDegree));//第二个点是起始角度对应的点

            float [] specialAngle = new float[]{45, 135, 225, 315};//上图正方形4个顶点对应的角度和点
            Vector3 [] specialPoint = new Vector3[] {
                new Vector3(1,1,0), 
                new Vector3(-1,1,0), 
                new Vector3(-1,-1,0),
                new Vector3(1,-1,0)
                // new Vector3(1,0,1),
                // new Vector3(-1,0,1),
                // new Vector3(-1,0,-1),
                // new Vector3(1,0,-1),    
            };
            //计算正方形四个点是不是在要画的扇形范围内, 如果是, 则是要画出来的顶点
            for (int i = 0; i < specialAngle.Length; ++i) {
                if(startAngleDegree < specialAngle[i] && specialAngle[i] - startAngleDegree < angleDegree)
                {
                    calcVertices.Add(specialPoint[i]);
                }
            }
            //这里为什么要第二个for循环? 扇形的角度+起始角度就可能超过360, 所以要计算到720
            for (int i = 0; i < specialAngle.Length; ++i) {
                if(startAngleDegree < specialAngle[i]+360 && specialAngle[i]+360 - startAngleDegree < angleDegree)
                {
                    calcVertices.Add(specialPoint[i]);
                }
            }
            calcVertices.Add (CalcPoint (startAngleDegree + angleDegree));

            Vector3[] vertices = new Vector3[calcVertices.Count];

            //uv是网格上的点对应到纹理上的某个位置的像素, 纹理是一张图片, 所以是二维
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i) {
                //之前是按半径为单位长度1计算的顶点, 实际顶点是要算上实际半径
                vertices[i] = calcVertices[i]*radius;
                //纹理的半径就是0.5, 圆心在0.5f, 0.5f的位置
                uvs[i] = new Vector2(calcVertices[i].x*0.5f+0.5f,calcVertices[i].y*0.5f+0.5f);
            }

            int[] triangles = new int[(vertices.Length-2)*3];
            for (int i = 0, vi = 1; i < triangles.Length; i += 3, vi++)
            {//每个三角形都是由圆心点+两个相邻弧度上的点构成的
                triangles[i] = 0;
                triangles[i + 2] = vi;
                triangles[i + 1] = vi + 1;
            }           
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            return mesh;
        }

        private bool checkDiff(float radius, float startAngleDegree, float angleDegree, int angleDegreePrecision, int radiusPrecision)
        {
            return (int)(startAngleDegree - this.startAngleDegree) != 0 || (int)((angleDegree - this.angleDegree) * angleDegreePrecision) != 0 ||
                (int)((radius - this.radius) * radiusPrecision) != 0;
        }
    }
}