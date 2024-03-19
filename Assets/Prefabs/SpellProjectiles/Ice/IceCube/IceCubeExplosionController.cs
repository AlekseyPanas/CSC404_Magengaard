using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class IceCubeExplosionController : NetworkBehaviour, ISpell
{
    [SerializeField] private float _baseDamage; 
    [SerializeField] private float _damage;
    [SerializeField] private float _baseTemperature; 
    [SerializeField] private float _temperature;
    [SerializeField] private float lifeTime;
    [SerializeField] int _spellStrength;    
    [SerializeField] Collider col;
    private float _timeModifier;
    public GameObject player;
    public ulong playerID;
    private List<GameObject> _currObjectsCollided;
    private Vector3 _startingScale;
    private Vector3 _scale;
    float timer;
    void Start()
    {
        _currObjectsCollided = new List<GameObject>();
        Destroy(gameObject, lifeTime);
        timer = 0.1f + Time.time;
    }

    void OnTriggerEnter(Collider col){
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!_currObjectsCollided.Contains(col.gameObject)) {
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int) (_damage * _timeModifier), SourcePosition = transform.position});
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = _temperature});
            _currObjectsCollided.Add(col.gameObject);
        }
    }

    void Update(){
        if(Time.time > timer){
            col.enabled = false;
        }
    }

    public void postInit()
    {
        _startingScale = Vector3.one;
        ApplySpellStrength();
    }

    public void preInit(SpellParamsContainer spellParams)
    {
        if(!IsServer) return;
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;

        ChargeSystemSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        _spellStrength = (int)prms.BinNumber;
    }

    public void SetSpellStrength(int spellStrength){
        _spellStrength = spellStrength;
    }

    public void SetTimeModifier(float t){
        _timeModifier = t;
    }

    public void preInitBackfire() {}
    public void setPlayerId(ulong playerId) { playerID = playerId; }
    void ApplySpellStrength(){
        float multiplier = 0.8f + _spellStrength * 0.2f;
        _damage = _baseDamage * multiplier;
        _scale = _startingScale * multiplier * _timeModifier;
        _temperature = _baseTemperature * multiplier;
        transform.localScale = _scale;
    }
}
