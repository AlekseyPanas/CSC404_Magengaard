public class AggroPlayerDetector : AAggroProvider
{
    public PlayerDetector pd;

    void Start()
    {
        var player = pd.GetPlayer();
        
        if (player != null)
        {
            TriggerAggroEvent(player);
        }
        
        pd.OnPlayerEnter += TriggerAggroEvent;
    }

    void OnDestroy(){
        pd.OnPlayerEnter -= TriggerAggroEvent;
    }
}
