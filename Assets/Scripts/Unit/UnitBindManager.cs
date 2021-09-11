using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///如果一个gameobject下1个或多个子gameobject装上了UnitBindPoint，但是又希望只管理这个gameobject，那就添加这个
///</summary>
public class UnitBindManager : MonoBehaviour{
    ///<summary>
    ///获得子GameObject下的某个UnitBindPoint
    ///<param name="key">这个UnitBindPoint的key</param>
    ///<return>如果找到就return，否则为null</return>
    ///</summary>
    public UnitBindPoint GetBindPointByKey(string key){
        UnitBindPoint[] bindPoints = this.gameObject.GetComponentsInChildren<UnitBindPoint>();
        for (int i = 0; i < bindPoints.Length; i++){
            if (bindPoints[i].key == key){
                return bindPoints[i];
            }
        }
        return null;
    }

    ///<summary>
    ///往某个绑点下添加一个gameObject绑定
    ///<param name="bindPointKey">绑点的key</param>
    ///<param name="goPath">要挂载的gameObject的prefab在resources下的路径</param>
    ///<param name="key">挂载信息的key，其实就是dictionary的key，手动删除的时候要用</param>
    ///<param name="loop">是否循环播放，直到手动删除</param>
    ///</summary>
    public void AddBindGameObject(string bindPointKey, string go, string key, bool loop){
        UnitBindPoint bp = GetBindPointByKey(bindPointKey);
        if (bp == null) return;
        bp.AddBindGameObject(go, key, loop);
    }

    ///<summary>
    ///在某个绑点下删除一个gameObject绑定
    ///<param name="bindPointKey">绑点的key</param>
    ///<param name="key">挂载信息的key</param>
    ///</summary>
    public void RemoveBindGameObject(string bindPointKey, string key){
        UnitBindPoint bp = GetBindPointByKey(bindPointKey);
        if (bp == null) return;
        bp.RemoveBindGameObject(key);
    }

    ///<summary>
    ///删除绑点下所有一个gameObject绑定
    ///<param name="key">挂载信息的key</param>
    ///</summary>
    public void RemoveAllBindGameObject(string key){
        UnitBindPoint[] bindPoints = this.gameObject.GetComponentsInChildren<UnitBindPoint>();
        for (int i = 0; i < bindPoints.Length; i++){
            bindPoints[i].RemoveBindGameObject(key);
        }
    }
}