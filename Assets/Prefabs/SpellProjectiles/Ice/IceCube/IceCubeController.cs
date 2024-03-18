using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IceCubeController : NetworkBehaviour, ISpell
{
    [SerializeField] private float _baseLifeTime; 
    [SerializeField] private float _lifeTime;
    [SerializeField] int _spellStrength;
    [SerializeField] float _baseVelocity;
    [SerializeField] float _velocity;
    [SerializeField] Rigidbody rb;
    [SerializeField] MeshRenderer _icecubeMesh;
    [SerializeField] private IceCubeIceEffect iceEffect;
    public GameObject explosionPrefab;
    public GameObject player;
    public ulong playerID; 
    private Vector3 _dir;
    private List<GameObject> _currObjectsCollided;
    private float _timeModifier;
    private Vector3 _startingScale;
    private Material _iceMat;
    private SpellParamsContainer _spellParams;
    void Start()
    {
        _currObjectsCollided = new List<GameObject>();
    }

    void Update()
    {
        _lifeTime -= Time.deltaTime;
        _timeModifier = _lifeTime / _baseLifeTime;
        transform.localScale = _startingScale * _timeModifier;
        _iceMat.SetFloat("_dissolve_amount", 1 - 4 * (1 - _timeModifier));

        if (_lifeTime <= 0) {
            DestroySelf(false);
        }

        if(rb.velocity.y > 0.5f){
            rb.velocity = new Vector3(rb.velocity.x, 0.5f, rb.velocity.z);
        }
    }

    void OnCollisionEnter(Collision col){
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.gameObject.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!_currObjectsCollided.Contains(col.gameObject)) {
            _currObjectsCollided.Add(col.gameObject);
        }
        if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy")) {
            DestroySelf(true);
        }
    }

    void DestroySelf(bool explode){
        if(NetworkManager == null) return;
        if (explode) {
            Debug.Log("destroy");
            GameObject spawnedExplosionPrefab = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ISpell iSpell = spawnedExplosionPrefab.GetComponent<ISpell>();
            iSpell.setPlayerId(playerID);
            iSpell.preInit(_spellParams);
            spawnedExplosionPrefab.GetComponent<NetworkObject>().Spawn();
            spawnedExplosionPrefab.GetComponent<IceCubeExplosionController>().SetSpellStrength(_spellStrength);
            iSpell.postInit();
            spawnedExplosionPrefab.transform.position = transform.position;
        }
        Destroy(gameObject);
    }

    public void postInit()
    {
        _dir = new Vector3(_dir.x, 0, _dir.z);
        Vector3.Normalize(_dir);

        ApplySpellStrength();

        transform.position = player.transform.position + _dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist
        transform.forward = _dir;
        rb.velocity = _dir * _velocity;
    }

    public void preInit(SpellParamsContainer spellParams)
    {
        _spellParams = spellParams;
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;

        ChargeSystemSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        _dir = prms.Direction3D;
        _spellStrength = (int)prms.BinNumber;
        _startingScale = transform.localScale; 
        _iceMat = _icecubeMesh.material;
    }

    public void preInitBackfire() {}

    public void setPlayerId(ulong playerId) {
        playerID = playerId;
        iceEffect.playerID = playerID;
    }
    void ApplySpellStrength(){
        float multiplier = 0.8f + _spellStrength * 0.2f;
        _baseLifeTime *= multiplier;
        _lifeTime = _baseLifeTime;
        _startingScale *= multiplier;
        _velocity = _baseVelocity * multiplier;
    }
}
