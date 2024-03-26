using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WindImpulseController : NetworkBehaviour, ISpell
{
    [SerializeField] float _baseWindEffectSpeed;
    [SerializeField] float _kbMultiplier;
    [SerializeField] float windEffectSpeed;
    [SerializeField] float lifeTime;
    [SerializeField] int _spellStrength;
    public GameObject player;
    public ulong playerID;
    private Vector3 dir;
    float timer;
    List<GameObject> objectsAlreadyCollided;
    SpellParamsContainer _spellParams;
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
            IEffectListener<WindEffect>.SendEffect(col.gameObject, new WindEffect(){Velocity = dir * windEffectSpeed, KBMultiplier = _kbMultiplier});
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

        transform.position = player.transform.position + dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist
        transform.forward = dir;
    }

    public void Update(){
        if (Time.time > timer){
            GetComponent<Collider>().enabled = false;
        }
    }

    void ApplySpellStrength(){
        float multiplier = 0.8f + _spellStrength * 0.2f;
        windEffectSpeed = _baseWindEffectSpeed * multiplier;
        transform.localScale *= multiplier;
        if (_spellStrength == 3) {
            StartCoroutine(SpawnClusterAfterDelay(2));
        }
        if (_spellStrength == 4) {
            StartCoroutine(SpawnClusterAfterDelay(4));
        }
    }
    public void SetSpellStrength(int spellStrength){
        _spellStrength = spellStrength;
    }
    void SpawnCluster(int i){
        GameObject cluster = Instantiate(gameObject, transform.position, Quaternion.identity);
        ISpell iSpell = cluster.GetComponent<ISpell>();
        iSpell.setPlayerId(playerID);
        iSpell.preInit(_spellParams);
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
}
