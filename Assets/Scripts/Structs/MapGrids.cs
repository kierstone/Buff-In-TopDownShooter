using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//地图的每个单元格的信息
public struct GridInfo{
    ///<summary>
    ///位于resources/prefabs文件夹下的的位置
    ///比如是resources/prefabs/terrain/grass，这个值就是"terrain/grass"
    ///</summary>
    public string prefabPath;

    ///<summary>
    ///地面移动是否可以通过
    ///</summary>
    public bool groundCanPass;

    ///<summary>
    ///飞行是否可以通过
    ///</summary>
    public bool flyCanPass;

    public GridInfo(string prefabPath, bool characterCanPass = true, bool bulletCanPass = true){
        this.prefabPath = prefabPath;
        this.groundCanPass = characterCanPass;
        this.flyCanPass = bulletCanPass;
    }

    ///<summary>
    ///场景外的格子，无意义的单元格
    ///</summary>
    public static GridInfo VoidGrid{get;} = new GridInfo("", false, false);
}

//整个地图的信息
public class MapInfo{
    ///<summary>
    ///地图的单元格信息，对应角色坐标为[x,z]
    ///</summary>
    public GridInfo[,] grid;

    ///<summary>
    ///每个单元格的宽度和高度
    ///单位：米
    ///</summary>
    public Vector2 gridSize{get;}

    ///<summary>
    ///获得地图的边界，单位：米
    ///</summary>
    public Rect border{get;}


    public MapInfo(GridInfo[,] map, Vector2 gridSize){
        this.grid = map;
        this.gridSize = new Vector2(
            Mathf.Max(0.1f, gridSize.x),    //最小0.1米
            Mathf.Max(0.1f, gridSize.y)
        );
        this.border = new Rect(
            -gridSize.x / 2.00f,
            -gridSize.y / 2.00f,
            gridSize.x,
            gridSize.y
        );
    }

    

    ///<summary>
    ///地图的宽度
    ///<return>单位：单元格</return>
    ///</summary>
    public int MapWidth(){
        return grid.GetLength(0);
    }

    ///<summary>
    ///地图的高度
    ///<return>单位：单元格</return>
    ///</summary>
    public int MapHeight(){
        return grid.GetLength(1);
    }

    ///<summary>
    ///获得某个东西坐落在的位置信息
    ///<param name="pos">要检查的点，单位：米</param>
    ///<return>返回这个点的单元格信息</return>
    ///</summary>
    public GridInfo GetGridInPosition(Vector3 pos){
        Vector2Int gPos = GetGridPosByMeter(pos.x, pos.z);
        if (gPos.x < 0 || gPos.x >= MapWidth() || gPos.y < 0 || gPos.y >= MapHeight()) 
            return GridInfo.VoidGrid;
        return grid[gPos.x, gPos.y];
    }

    ///<summary>
    ///从地图上x坐标的米，获得单元格坐标x
    ///<param name="x">角色坐标x，单位：米</param>
    ///<param name="z">角色坐标z，单位：米</param>
    ///<return>单元格坐标，单位：单元格</return>
    ///</summary>
    public Vector2Int GetGridPosByMeter(float x, float z){
        return new Vector2Int(
            //Mathf.FloorToInt((x + gridSize.x / 2.00f) / gridSize.x),
            //Mathf.FloorToInt((z + gridSize.y / 2.00f) / gridSize.y)
            Mathf.RoundToInt(x / gridSize.x),
            Mathf.RoundToInt(z / gridSize.y)
        );
    }

    ///<summary>
    ///判断某种移动模式下，某个单元格是否可过
    ///<param name="gridX">单元格坐标x</param>
    ///<param name="gridY">单元格坐标y</param>
    ///<param name="moveType">移动方式</param>
    ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
    ///<return>是否可过</return>
    ///</summary>
    public bool CanGridPasses(int gridX, int gridY, MoveType moveType, bool ignoreBorder){
        if (gridX < 0 || gridX >= MapWidth() || gridY < 0 || gridY >= MapHeight()) return ignoreBorder;
        switch (moveType){
            case MoveType.ground: return grid[gridX, gridY].groundCanPass;
            case MoveType.fly: return grid[gridX, gridY].flyCanPass;
        }
        return false;
    }

    ///<summary>
    ///判断一个单位是否可以站在某个位置上
    ///<param name="pos">单位的位置，单位：米</param>
    ///<param name="radius">单位的半径，单位：米</param>
    ///<param name="moveType">单位移动模式</param>
    ///<return>是否可以站在这里，true代表可以</return>
    ///</summary>
    public bool CanUnitPlacedHere(Vector3 pos, float radius, MoveType moveType){
        Vector2Int lt = GetGridPosByMeter(Mathf.Max(0, pos.x - radius), Mathf.Max(0, pos.z - radius));
        Vector2Int rb = GetGridPosByMeter(Mathf.Min(pos.x + radius, MapWidth() - 1), Mathf.Min(pos.z + radius, MapHeight() - 1));
        int aw = rb.x - lt.x + 1;
        int ah = rb.y - lt.y + 1;
        List<Rect> collisionRects = new List<Rect>();
        for (int i = lt.x; i <= rb.x; i++){
            for (int j = lt.y; j <= rb.y; j++){
                if (
                    (moveType == MoveType.ground && grid[i, j].groundCanPass == false) ||
                    (moveType == MoveType.fly && grid[i, j].flyCanPass == false)
                ){
                    collisionRects.Add(new Rect(
                        (i - 0.5f) * gridSize.x,
                        (j - 0.5f) * gridSize.y,
                        gridSize.x,
                        gridSize.y
                    ));
                }
                
            }
        }
        return !Utils.CircleHitRects(new Vector2(pos.x, pos.z), radius, collisionRects);
        // Vector2Int gPos = GetGridPosByMeter(pos.x, pos.z);
        // if (gPos.x < 0 || gPos.y < 0 || gPos.x >= MapWidth() || gPos.y >= MapHeight()) return false;
        // return grid[gPos.x, gPos.y].groundCanPass;
    }

        
    ///<summary>
    ///随机获得一个坐标，可以让角色站立在那里
    ///获得的坐标单位是米的，而非单元格，其坐标y始终为0
    ///如果实在没有这个位置，就会返回vector3.zero
    ///<param name="range">一个随机的范围，单位：单元格</param>
    ///<param name="chaRadius">角色半径，单位：米</param>
    ///<param name="moveType">单位移动模式</param>
    ///<return>可以落脚的坐标点</return>
    public Vector3 GetRandomPosForCharacter(RectInt range, float chaRadius = 0.00f, MoveType moveType = MoveType.ground){
        List<Vector3> mayRes = new List<Vector3>();
        for (var i = range.x; i < range.x + range.width; i ++){
            for (var j = range.y; j < range.y + range.height; j++){
                //if (i >= 0 && i < MapWidth() && j >= 0 && j < MapHeight() && gridInfo[i, j].characterCanPass == true){
                Vector3 ranPos = new Vector3(
                    i * gridSize.x, 
                    0, 
                    j * gridSize.y
                );
                if (CanUnitPlacedHere(ranPos, chaRadius, moveType) == true) {  
                    mayRes.Add(ranPos);
                }
            }
        }
        return mayRes[Mathf.FloorToInt(Random.Range(0, mayRes.Count))];
    }


    
    ///<summary>
    ///从一个点（单位米）出发获得方向上的第一个水平阻挡
    ///<param name="pivot">出发的点，单位：米</param>
    ///<param name="dir">查询方向以及长度，单位：米</param>
    ///<param name="radius">假设有一个半径（当做点是圆形中心），也就是额外追加一个距离，单位：米</param>
    ///<param name="moveType">移动方式</param>
    ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
    ///<return>最合适的x坐标</return>
    ///</summary>
    public float GetNearestVerticalBlock(Vector3 pivot, float dir, float radius, MoveType moveType, bool ignoreBorder){
        if (dir == 0) return pivot.x;
        int dv = dir > 0 ? 1 : -1;
        float bestX = pivot.x + dir;
        int seekWidth = Mathf.CeilToInt((Mathf.Abs(dir) + radius) / gridSize.x + 2);
        Vector2Int gPos = GetGridPosByMeter(pivot.x, pivot.z);
        for (var i = 0; i < seekWidth; i++){
            int cgX = gPos.x + dv * i;
            if (this.CanGridPasses(cgX, gPos.y, moveType, ignoreBorder) == false){
                float wallX = (cgX - dv * 0.5f) * gridSize.x - dv * radius;
                if (dv > 0){
                    return Mathf.Min(wallX, bestX);
                }else{
                    return Mathf.Max(wallX, bestX);
                }
            }
        }
        return bestX;
    }

    ///<summary>
    ///从一个点（单位米）出发获得方向上的第一个垂直阻挡
    ///<param name="pivot">出发的点，单位：米</param>
    ///<param name="dir">查询方向以及高度，单位：米</param>
    ///<param name="radius">假设有一个半径（当做点是圆形中心），也就是额外追加一个距离，单位：米</param>
    ///<param name="moveType">移动方式</param>
    ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
    ///<return>最合适的z坐标</return>
    ///</summary>
    public float GetNearestHorizontalBlock(Vector3 pivot, float dir, float radius, MoveType moveType, bool ignoreBorder){
        if (dir == 0) return pivot.z;
        int dv = dir > 0 ? 1 : -1;
        float bestZ = pivot.z + dir;
        int seekHeight = Mathf.CeilToInt((Mathf.Abs(dir) + radius) / gridSize.y + 2);
        Vector2Int gPos = GetGridPosByMeter(pivot.x, pivot.z);
        for (var i = 0; i < seekHeight; i++){
            int cgY = gPos.y + dv * i;
            if (this.CanGridPasses(gPos.x, cgY, moveType, ignoreBorder) == false){
                float wallZ = (cgY - dv * 0.5f) * gridSize.y - dv * radius;
                if (dv > 0){
                    return Mathf.Min(wallZ, bestZ);
                }else{
                    return Mathf.Max(wallZ, bestZ);
                }
            }
        }
        return bestZ;
    }

    ///<summary>
    ///根据一个圆（中心点和半径），获得期望移动到某个坐标的最理想的点
    ///TODO 浮点数问题，导致边界碰撞有异常，接近0（无论是x还是z）都会出现异常
    ///<param name="pivot">这个圆形的中心点坐标</param>
    ///<param name="radius">这个圆形的半径</param>
    ///<param name="targetPos">这个圆形期望移动到的坐标</param>
    ///<param name="moveType">圆形的移动方式</param>
    ///<param name="ignoreBorder">是否把地图外的区域都当做可过</param>
    ///<return>应该移动到的坐标</return>
    ///</summary>
    public MapTargetPosInfo FixTargetPosition(Vector3 pivot, float radius, Vector3 targetPos, MoveType moveType, bool ignoreBorder){
        float xDir = targetPos.x - pivot.x;
        float zDir = targetPos.z - pivot.z;
        float bestX = GetNearestVerticalBlock(pivot, xDir, radius, moveType, ignoreBorder);
        float bestZ = GetNearestHorizontalBlock(pivot, zDir, radius, moveType, ignoreBorder);

        bool obstacled =  (bestX != targetPos.x || bestZ != targetPos.z);
        return new MapTargetPosInfo(obstacled, new Vector3(bestX, targetPos.y, bestZ));
    }
}

///<summary>
///目标地点的信息
///</summary>
public struct MapTargetPosInfo{
    ///<summary>
    ///是否会碰到阻碍
    ///</summary>
    public bool obstacle;

    ///<summary>
    ///建议移动到的位置
    ///</summary>
    public Vector3 suggestPos;

    public MapTargetPosInfo(bool obstacle, Vector3 suggestPos){
        this.obstacle = obstacle;
        this.suggestPos = suggestPos;
    }
}
