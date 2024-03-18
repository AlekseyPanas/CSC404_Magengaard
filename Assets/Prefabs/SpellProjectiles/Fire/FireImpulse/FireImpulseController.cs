using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireImpulseController : NetworkBehaviour, ISpell
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _damage;
    [SerializeField] private float _baseTemperature;
    [SerializeField] private float _temperature;
    [SerializeField] private float _clusterDistance;
    [SerializeField] int _spellStrength;
    private Vector3 dir;
    public ulong playerID;
    float timer = 0.1f;
    SpellParamsContainer _spellParams;
    List<GameObject> objectsAlreadyCollided;
    Vector3 startScale;
    void Awake(){
        Invoke("DestroySpell", lifeTime);
        timer += Time.time;
        objectsAlreadyCollided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!objectsAlreadyCollided.Contains(col.gameObject)){
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = _temperature});
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

    public void postInit() {
        Vector3.Normalize(dir);
        dir = new Vector3(dir.x, 0, dir.z);

        ApplySpellStrength();

        transform.position = player.transform.position + dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist1
        transform.forward = dir;
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
        if (_spellStrength == 3){
            StartCoroutine(SpellStrenght3());
        }
        else if (_spellStrength == 4) {
            StartCoroutine(SpellStrenght4());
        }
    }
    public void SetSpellStrength(int spellStrength){
        _spellStrength = spellStrength;
    }
    void SpawnCluster(Vector3 position, int strength){
        GameObject cluster = Instantiate(gameObject, transform.position, Quaternion.identity);
        cluster.transform.localScale = startScale;
        ISpell iSpell = cluster.GetComponent<ISpell>();
        iSpell.setPlayerId(playerID);
        iSpell.preInit(_spellParams);
        cluster.GetComponent<NetworkObject>().Spawn();
        cluster.GetComponent<FireImpulseController>().SetSpellStrength(strength);
        iSpell.postInit();
        cluster.transform.position = position;
    }

    IEnumerator SpellStrenght3(){
        yield return new WaitForSeconds(0.2f);
        Vector3 center1 = transform.position + transform.forward * _clusterDistance;
        Vector3 crossOffset = transform.right * _clusterDistance;
        Vector3 pos1 = center1 + crossOffset/2;
        Vector3 pos2 = center1 - crossOffset/2;
        SpawnCluster(pos1, 2);
        SpawnCluster(pos2, 2);
    }

    IEnumerator SpellStrenght4(){
        yield return new WaitForSeconds(0.2f);
        Vector3 center1 = transform.position + transform.forward * _clusterDistance;
        Vector3 crossOffset = transform.right * _clusterDistance;
        Vector3 pos1 = center1 + crossOffset/2;
        Vector3 pos2 = center1 - crossOffset/2;
        SpawnCluster(pos1, 2);
        SpawnCluster(pos2, 2);
        yield return new WaitForSeconds(0.2f);
        Vector3 center2 = center1 + transform.forward * _clusterDistance;
        Vector3 pos3 = center2;
        Vector3 pos4 = center2 + crossOffset;
        Vector3 pos5 = center2 - crossOffset;
        SpawnCluster(pos3, 1);
        SpawnCluster(pos4, 1);
        SpawnCluster(pos5, 1);
    }
}
