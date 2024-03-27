using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


enum ABSORB_STATE {
    WATER = 1,
    FIRE = 2,
    SAND = 3
}

public class TornadoController : NetworkBehaviour, ISpell, IEffectListener<WaterEffect>
{
    [SerializeField] float _baseWindEffectSpeed;
    [SerializeField] float _kbMultiplier;
    [SerializeField] float _windEffectSpeed;
    [SerializeField] float _projectileReflectionDamageMultiplier;
    [SerializeField] float _baseAbsorbtionRate;
    [SerializeField] float _absorbtionRate;
    [SerializeField] float lifeTime;
    [SerializeField] int _spellStrength;
    [SerializeField] float _baseTornadoSpeed;
    [SerializeField] float _tornadoSpeed;
    [SerializeField] float _damageTickInterval;
    [SerializeField] float _damageTickTimer = 0;
    [SerializeField] GameObject waterAbsorb;
    [SerializeField] Rigidbody rb;
    [SerializeField] ParticleSystem ps;
    [SerializeField] GameObject deflectionPS;
    public GameObject player;
    private Vector3 dir;
    public ulong playerID;
    public List<GameObject> objectsCurrentlyColliding;
    private ABSORB_STATE _absorbState;
    SpellParamsContainer spellParams;
    float _currAbsorbedVolume = 0f;
    bool _hasAbsorbed = false;
    void Awake(){
        Invoke(nameof(DestroySpell), lifeTime);
        objectsCurrentlyColliding = new List<GameObject>();
        GetComponent<Collider>().enabled = true;
    }

    void Update(){
        if(Time.time > _damageTickTimer){
            _damageTickTimer = Time.time + _damageTickInterval;
            DoDamageTick();
        }
    }

    void OnTriggerEnter(Collider col){
        Debug.Log(col.name);
        objectsCurrentlyColliding.RemoveAll(item => item == null);
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!objectsCurrentlyColliding.Contains(col.gameObject)){
            objectsCurrentlyColliding.Add(col.gameObject);
        }
        if(col.CompareTag("Ground")){
            Invoke(nameof(DestroySpell), 0.25f);
        }
    }

    void DoDamageTick(){
        foreach(GameObject g in objectsCurrentlyColliding){
            if(g != null){
                Vector3 dir = g.transform.position - transform.position;
                dir = new Vector3(dir.x, 0, dir.z).normalized;
                IEffectListener<WindEffect>.SendEffect(g, new WindEffect(){Direction = g.transform.position - transform.position, 
                Velocity = dir * _windEffectSpeed, ReflectDamageMultiplier = _projectileReflectionDamageMultiplier,
                DeflectionParticle = deflectionPS, KBMultiplier = _kbMultiplier});
                if(_absorbState == ABSORB_STATE.WATER){
                    IEffectListener<WaterEffect>.SendEffect(g, new WaterEffect(){WaterVolume = _currAbsorbedVolume});
                }
            }
        }
    }

    void OnTriggerExit(Collider col){
        objectsCurrentlyColliding.RemoveAll(item => item == null);
        if(objectsCurrentlyColliding.Contains(col.gameObject)){
            objectsCurrentlyColliding.Remove(col.gameObject);
        }
    }

    void DestroySpell(){
        if(!IsOwner) return;
        ps.Stop();
        Destroy(gameObject, 2f);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() { }

    public void preInit(SpellParamsContainer spellParams) {
        this.spellParams = spellParams;
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;

        ChargeSystemSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        dir = prms.Direction3D;
        _spellStrength = (int)prms.BinNumber;
    }

    public void postInit() {
        Vector3.Normalize(dir);
        dir = new Vector3(dir.x, 0, dir.z);

        ApplySpellStrength();

        transform.position = player.transform.position + dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist
        transform.forward = dir;
        rb.velocity = dir * _tornadoSpeed;
    }
    void ApplySpellStrength(){
        float multiplier = 0.8f + _spellStrength * 0.2f;
        _windEffectSpeed = _baseWindEffectSpeed * multiplier;
        transform.localScale *= multiplier;
        _absorbtionRate = _baseAbsorbtionRate * multiplier;
        _tornadoSpeed = _baseTornadoSpeed * multiplier;
        // if (_spellStrength == 3) {
        //     StartCoroutine(SpawnClusterAfterDelay(2));
        // }
        // if (_spellStrength == 4) {
        //     StartCoroutine(SpawnClusterAfterDelay(4));
        // }
    }
    public void SetSpellStrength(int spellStrength){
        _spellStrength = spellStrength;
    }
    void SpawnCluster(int i){
        GameObject cluster = Instantiate(gameObject, transform.position, Quaternion.identity);
        ISpell iSpell = cluster.GetComponent<ISpell>();
        iSpell.setPlayerId(playerID);
        iSpell.preInit(spellParams);
        cluster.GetComponent<NetworkObject>().Spawn();
        cluster.GetComponent<WindImpulseController>().SetSpellStrength(1);
        iSpell.postInit();
        cluster.transform.position = transform.position + ((i + 1) * 0.3f * transform.forward) + ((i % 2 * 2 - 1) * 0.2f * transform.right);
        cluster.transform.localScale *= 1 - i * 0.1f;
    }

    IEnumerator SpawnClusterAfterDelay(int numClusters){
        for(int i = 0; i < numClusters; i++) {
            yield return new WaitForSeconds(0.15f);
            SpawnCluster(i);
        }
    }

    public void OnEffect(WaterEffect effect)
    {
        _absorbState = ABSORB_STATE.WATER;
        _currAbsorbedVolume += _absorbtionRate;
        if (!_hasAbsorbed) {
            Instantiate(waterAbsorb, transform);
            _hasAbsorbed = true;
        }
    }
}
