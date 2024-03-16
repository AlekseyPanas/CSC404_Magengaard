public class AggroPlayerDetector : AAggroProvider
{
    public PlayerDetector pd;

    void Start(){
        pd.OnPlayerEnter += TriggerAggroEvent;
    }

    void OnDestroy(){
        pd.OnPlayerEnter -= TriggerAggroEvent;
    }
}
