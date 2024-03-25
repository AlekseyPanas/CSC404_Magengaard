public class AggroPlayerDetector : AAggroProvider
{
    public PlayerDetector pd;
    public bool agroOnSpawn;

    void Start()
    {
        var player = pd.GetPlayer();
        
        if (player != null && agroOnSpawn)
        {
            TriggerAggroEvent(player);
        }
        
        pd.OnPlayerEnter += TriggerAggroEvent;
    }

    new void OnDestroy(){
        pd.OnPlayerEnter -= TriggerAggroEvent;
        base.OnDestroy();
    }

    public void SetAgroOnSpawn(bool a){
        agroOnSpawn = a;
    }
}
