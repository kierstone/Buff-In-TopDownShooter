using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //总有一个角色是主角，也就是玩家控制的，并且镜头跟随的
    public GameObject mainActor{get{
        return mainCharacter;
    }}
    private GameObject mainCharacter;  

    //所有的gameobject放置的地方
    private GameObject root;

    //特效管理器
    private Dictionary<string, GameObject> sightEffect = new Dictionary<string, GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        root = GameObject.Find("GameObjectLayer");

        //创建地图
        SceneVariants.RandomMap(Random.Range(10, 15), Random.Range(10, 15));
        CreateMapGameObjects();

        //创建主角
        Vector3 playerPos = SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight()));
        mainCharacter = this.CreateCharacter(
            "FemaleGunner", 1, new Vector3(), new ChaProperty(100, Random.Range(5000,7000), 600, Random.Range(50,70)), 0
        );  //这里必须是new Vector3()因为相机跟随的设置问题
        mainCharacter.AddComponent<PlayerController>().mainCamera = Camera.main;
        
        //镜头跟随
        GameObject.Find("Main Camera").GetComponent<CamFollow>().SetFollowCharacter(mainCharacter);   
        //ui血量捕捉，别问我这是什么狗屎了，我也觉得狗屎
        GameObject.Find("PlayerHP").GetComponent<PlayerStateListener>().playerGameObject = mainCharacter;

        //再设置主角位置
        mainCharacter.transform.position = playerPos;  
        ChaState mcs = mainCharacter.GetComponent<ChaState>();
        mcs.LearnSkill(DesingerTables.Skill.data["fire"]);
        mcs.LearnSkill(DesingerTables.Skill.data["roll"]);
        mcs.LearnSkill(DesingerTables.Skill.data["spaceMonkeyBall"]);  
        mcs.LearnSkill(DesingerTables.Skill.data["homingMissle"]);   
        mcs.LearnSkill(DesingerTables.Skill.data["cloakBoomerang"]);
        mcs.LearnSkill(DesingerTables.Skill.data["teleportBullet"]);
        mcs.LearnSkill(DesingerTables.Skill.data["grenade"]);
        mcs.LearnSkill(DesingerTables.Skill.data["explosiveBarrel"]);

        //【test】给主角添加火焰护盾的aoe
        // this.CreateAoE(new AoeLauncher(
        //     DesingerTables.AoE.data["BulletShield"], mainCharacter, mainCharacter.transform.position,
        //     0.3f, 600.00f, 0,
        //     DesignerScripts.AoE.aoeTweenFunc["AroundCaster"],//DesignerScripts.AoE.aoeTweenFunc["AroundCaster"], 
        //     new object[]{1.2f, 360.0f}
        // ));
        
    }

    void Update()
    {
        
    }

    private void FixedUpdate() {
        //管理一下视觉特效，看哪些需要清楚了
        List<string> toRemoveKey = new List<string>();
        foreach(KeyValuePair<string, GameObject> se in sightEffect){
            if (se.Value == null) toRemoveKey.Add(se.Key);
        }
        for (int i = 0; i < toRemoveKey.Count; i++) sightEffect.Remove(toRemoveKey[i]);
        toRemoveKey = null;

        
    }


    //根据prefab下的资源创建东西
    private GameObject CreateFromPrefab(string prefabPath, Vector3 position = new Vector3(), float rotation = 0.00f){
        GameObject go = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/" + prefabPath),
            position,
            Quaternion.identity
        );
        if (rotation != 0){
            go.transform.Rotate(new Vector3(0, rotation, 0));
        }
        go.transform.SetParent(root.transform);
        return go;
    }

    //根据global.map制作地图的prefabs
    private void CreateMapGameObjects(){
        GameObject[] mt = GameObject.FindGameObjectsWithTag("MapTile");
        for (var i = 0; i < mt.Length; i++){
            Destroy(mt[i]);
        }
        mt = null;

        for (var i = 0; i < SceneVariants.map.MapWidth(); i++){
            for (var j = 0; j < SceneVariants.map.MapHeight(); j++){
                CreateFromPrefab(SceneVariants.map.grid[i,j].prefabPath, new Vector3(i, 0, j));
            }
        }
    }

    ///<summary>
    ///创建一个子弹对象在场景上
    ///<param name="bulletLauncher">子弹发射器</param>
    ///</summary>
    public void CreateBullet(BulletLauncher bulletLauncher){
        //创建一个bulletObj，这是个“空”的子弹，其实也就是没有视觉效果，其他都有了
        GameObject bulletObj = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/Bullet/BulletObj"),
            bulletLauncher.firePosition,
            Quaternion.identity,
            root.transform
        );
        
        //处理bulletObj的数据
        bulletObj.transform.RotateAround(bulletObj.transform.position, Vector3.up, bulletLauncher.fireDegree);
        
        bulletObj.GetComponent<BulletState>().InitByBulletLauncher(
            bulletLauncher, 
            GameObject.FindGameObjectsWithTag("Character") //我这个游戏里，只给你角色对象，你要跟踪子弹，那就再把子弹也抓进来就好
        );
    }

    ///<summary>
    ///删除一个存在的子弹Object
    ///<param name="aoe">子弹的GameObject</param>
    ///<param name="immediately">是否当场清除，如果false，就是把时间变成0</param>
    ///</summary>
    public void RemoveBullet(GameObject bullet, bool immediately = false){
        if (!bullet) return;
        BulletState bulletState = bullet.GetComponent<BulletState>();
        if (!bulletState) return;
        bulletState.duration = 0;
        if (immediately == true){
            if (bulletState.model.onRemoved != null){
                bulletState.model.onRemoved(bullet);
            }
            Destroy(bullet);
        }
    }

    ///<summary>
    ///创建一个aoe对象在场景上
    ///<param name="aoeLauncher">aoe的创建信息</param>
    ///</summary>
    public void CreateAoE(AoeLauncher aoeLauncher){
        //创建一个bulletObj，这是个“空”的子弹，其实也就是没有视觉效果，其他都有了
        GameObject aoeObj = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/Effect/AoeObj"),
            aoeLauncher.position,
            Quaternion.identity,
            root.transform
        );
        
        aoeObj.GetComponent<AoeState>().InitByAoeLauncher(aoeLauncher);
    }

    ///<summary>
    ///删除一个存在的aoeObject
    ///<param name="aoe">aoe的GameObject</param>
    ///<param name="immediately">是否当场清除，如果false，就是把时间变成0</param>
    ///</summary>
    public void RemoveAoE(GameObject aoe, bool immediately = false){
        if (!aoe) return;
        AoeState aoeState = aoe.GetComponent<AoeState>();
        if (!aoeState) return;
        aoeState.duration = 0;
        if (immediately == true){    
            if (aoeState.model.onRemoved != null){
                aoeState.model.onRemoved(aoe);
            }
            Destroy(aoe);
        }
    }

    ///<summary>
    ///创建一个视觉特效在场景上
    ///<param name="prefab">特效的prefab文件夹，约定就在Prefabs/下，所以路径不应该加这段</param>
    ///<param name="pos">创建的位置</param>
    ///<param name="degree">角度</param>
    ///<param name="key">特效的key，如果重复则无法创建，删除的时候也有用，空字符串的话不加入管理</param>
    ///<param name="loop">是否循环，循环的得手动remove</param>
    ///</summary>
    public void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false){
        if (sightEffect.ContainsKey(key) == true) return;    //已经存在，加不成

        GameObject effectGO = Instantiate<GameObject>(
            Resources.Load<GameObject>("Prefabs/"+prefab),
            pos,
            Quaternion.identity,
            this.gameObject.transform
        );
        effectGO.transform.RotateAround(effectGO.transform.position, Vector3.up, degree);
        if (!effectGO) return;
        SightEffect se = effectGO.GetComponent<SightEffect>();
        if (!se){
            Destroy(effectGO);
            return;
        } 
        if (loop == false){
            effectGO.AddComponent<UnitRemover>().duration = se.duration;
        }

        if (key != "")  sightEffect.Add(key, effectGO);
    }

    ///<summary>
    ///删除一个视觉特效在场景上
    ///<param name="key">特效的key</param>
    ///</summary>
    public void RemoveSightEffect(string key){
        if (sightEffect.ContainsKey(key) == false) return;
        Destroy(sightEffect[key]);
        sightEffect.Remove(key);
    }

    ///<summary>
    ///创建一个角色到场上
    ///<param name="prefab">特效的prefab文件夹，约定就在Prefabs/Character/下，所以路径不应该加这段</param>
    ///<param name="unitAnimInfo">角色的动画信息</param>
    ///<param name="side">所属阵营</param>
    ///<param name="pos">创建的位置</param>
    ///<param name="degree">角度</param>
    ///<param name="baseProp">初期的基础属性</param>
    ///<param name="tags">角色的标签，分类角色用的</param>
    ///</summary>
    public GameObject CreateCharacter(string prefab, int side, Vector3 pos, ChaProperty baseProp, float degree, string unitAnimInfo = "Default_Gunner", string[] tags = null){
        GameObject chaObj = CreateFromPrefab("Character/CharacterObj");
        //Vector3 playerPos = SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight()));
        //cha.AddComponent<PlayerController>().mainCamera = Camera.main; //敌人没有controller
        ChaState cs = chaObj.GetComponent<ChaState>();
        if (cs){
            cs.InitBaseProp(baseProp);
            cs.side = side;
            Dictionary<string, AnimInfo> aInfo = new Dictionary<string, AnimInfo>();
            if (unitAnimInfo != "" && DesingerTables.UnitAnimInfo.data.ContainsKey(unitAnimInfo)){
                aInfo = DesingerTables.UnitAnimInfo.data[unitAnimInfo];
            }
            cs.SetView(CreateFromPrefab("Character/" + prefab), aInfo);
            if (tags != null) cs.tags = tags;
        }
        
        chaObj.transform.position = pos;
        chaObj.transform.RotateAround(chaObj.transform.position, Vector3.up, degree);
        return chaObj;
    }
}
