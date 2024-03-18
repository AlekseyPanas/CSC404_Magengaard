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
    [SerializeField] private float _baseWindEffectSpeed;
    [SerializeField] float windEffectSpeed;
    [SerializeField] private float _baseWatervolume;
    [SerializeField] float _waterVolume;
    [SerializeField] private float _baseFireTemp;
    [SerializeField] float _fireTemp;
    [SerializeField] private float _baseDamage;
    [SerializeField] float damage;
    [SerializeField] private float lifeTime;
    [SerializeField] int _spellStrength;
    [SerializeField] float _baseTornadoSpeed;
    [SerializeField] float _tornadoSpeed;
    [SerializeField] GameObject waterAbsorb;
    [SerializeField] Rigidbody rb;
    public GameObject player;
    private Vector3 dir;
    public ulong playerID;
    float timer;
    List<GameObject> objectsAlreadyCollided;
    private ABSORB_STATE _absorbState;
    SpellParamsContainer spellParams;
    void Awake(){
        Invoke(nameof(DestroySpell), lifeTime);
        timer = 0.1f + Time.time;
        objectsAlreadyCollided = new List<GameObject>();
        GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider col){
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!objectsAlreadyCollided.Contains(col.gameObject)){
            Vector3 dir = col.gameObject.transform.position - transform.position;
            dir = new Vector3(dir.x, 0, dir.z).normalized;
            IEffectListener<WindEffect>.SendEffect(col.gameObject, new WindEffect(){Velocity = dir * windEffectSpeed});
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int) damage, SourcePosition = transform.position});
            if(_absorbState == ABSORB_STATE.WATER){
                IEffectListener<WaterEffect>.SendEffect(col.gameObject, new WaterEffect(){WaterVolume = _waterVolume});
            } else if(_absorbState == ABSORB_STATE.FIRE){
                IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = _fireTemp});
            } else if(_absorbState == ABSORB_STATE.SAND){
                //todo
            }
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

    public void Update(){
        if (Time.time > timer){
            GetComponent<Collider>().enabled = false;
        }
    }

    void ApplySpellStrength(){
        float multiplier = 0.8f + _spellStrength * 0.2f;
        damage = _baseDamage * multiplier;
        windEffectSpeed = _baseWindEffectSpeed * multiplier;
        transform.localScale *= multiplier;
        _waterVolume = _baseWatervolume * multiplier;
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
        Instantiate(waterAbsorb, transform);
    }
}
