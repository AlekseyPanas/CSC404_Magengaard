using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IceImpulseController : NetworkBehaviour, ISpell
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _damage;
    [SerializeField] private float _baseTemperature;
    [SerializeField] private float _temperature;
    [SerializeField] int _spellStrength;
    [SerializeField] float _clusterDistance;
    [SerializeField] GameObject _iceMesh;
    private Vector3 dir;
    public ulong playerID;
    float timer = 0.1f;
    SpellParamsContainer _spellParams;
    public List<GameObject> objectsAlreadyCollided;
    Vector3 startScale;
    void Awake(){
        Invoke(nameof(DestroySpell), lifeTime);
        timer += Time.time;
        objectsAlreadyCollided = new List<GameObject>();
        GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider col){
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!objectsAlreadyCollided.Contains(col.gameObject)){
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = _temperature, mesh = _iceMesh});
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int) _damage, SourcePosition = transform.position});
            objectsAlreadyCollided.Add(col.gameObject);
        }
    }

    void DestroySpell(){
        if(!IsOwner) return;
        Destroy(gameObject);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() { }

    public void preInit(SpellParamsContainer spellParams) {
        _spellParams = spellParams;
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;

        ChargeSystemSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        dir = prms.Direction3D;
        _spellStrength = (int)prms.BinNumber;
    }

    public void SetDir(Vector3 newDir){
        dir = newDir;
    }

    public void postInit() {
        Vector3.Normalize(dir);
        dir = new Vector3(dir.x, 0, dir.z);

        transform.position = player.transform.position + dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist
        transform.forward = dir;

        ApplySpellStrength();
    }
    public void SetSpellStrength(int spellStrength){
        _spellStrength = spellStrength;
    }

    public void Update(){
        if (Time.time > timer){
            GetComponent<Collider>().enabled = false;
        }
    }
    void ApplySpellStrength(){
        startScale = transform.localScale;
        float multiplier = 0.8f + _spellStrength * 0.2f;
        _damage = _baseDamage * multiplier;
        _temperature = _baseTemperature * multiplier;
        transform.localScale *= multiplier;
        if (_spellStrength == 3) {
            SpellStrength3();
        }
        if (_spellStrength == 4) {
            SpellStrength4();
        }
    }
    void SpawnCluster(Vector3 position, int strength){
        GameObject cluster = Instantiate(gameObject, transform.position, Quaternion.identity);
        cluster.transform.localScale = startScale;
        ISpell iSpell = cluster.GetComponent<ISpell>();
        iSpell.setPlayerId(playerID);
        iSpell.preInit(_spellParams);
        cluster.GetComponent<NetworkObject>().Spawn();
        cluster.GetComponent<IceImpulseController>().SetSpellStrength(strength);
        cluster.GetComponent<IceImpulseController>().SetDir(Vector3.zero);
        iSpell.postInit();
        cluster.transform.position = position;
    }

    void SpellStrength3(){
        Vector3 pos1 = transform.position + transform.forward * _clusterDistance;
        Vector3 pos2 = transform.position - transform.forward * _clusterDistance;
        Vector3 pos3 = transform.position + transform.right * _clusterDistance;
        Vector3 pos4 = transform.position - transform.right * _clusterDistance;
        SpawnCluster(pos1,2);
        SpawnCluster(pos2,2);
        SpawnCluster(pos3,2);
        SpawnCluster(pos4,2);
    }

    void SpellStrength4(){
        SpellStrength3();
        Vector3 dir1 = (transform.forward + transform.right).normalized * _clusterDistance;
        Vector3 dir2 = (transform.forward - transform.right).normalized * _clusterDistance;
        Vector3 pos1 = transform.position + dir1;
        Vector3 pos2 = transform.position - dir1;
        Vector3 pos3 = transform.position + dir2;
        Vector3 pos4 = transform.position - dir2;
        SpawnCluster(pos1,2);
        SpawnCluster(pos2,2);
        SpawnCluster(pos3,2);
        SpawnCluster(pos4,2);
    }
}
