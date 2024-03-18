using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

enum ElevatorGroundState
{
    Inactive,
    WaitingForPlayer,
    FirstClimb,
    HoldUp,
    FightOne,
    FightTwo,
    SecondClimb,
    ReachedTop
}

public class ElevatorGround : MonoBehaviour
{
    public int lightConditionCount = 2;
    public int firstConditionCount = 1;
    public int secondConditionCount = 2;
    
    // Fight Details
    public UnityEvent firstFightBegin;
    public UnityEvent secondFightBegin;
    
    // Other Details
    public List<Animator> gears;
    public GameObject sealWall;

    private ElevatorGroundState _state;

    public PlayerDetector activationDetector;

    public float climbSpeed = 0.0f;
    public float climbFightHeight = 0.0f; // absolute max height
    public float climbHeight = 0.0f; // absolute max height
    
    private static readonly int Turning = Animator.StringToHash("Turning");

    void SetMoving(bool value)
    {
        gears.ForEach(x => x.SetBool(Turning, value));
    }

    // Player can activate balloons outside of the elevator, creating a soft-lock.
    private void StartWaitingForPlayer()
    {
        _state = ElevatorGroundState.WaitingForPlayer;

        if (activationDetector.GetPlayer() != null)
        {
            StartFirstClimb();
        }
    }
    
    private void StartFirstClimb()
    {
        _state = ElevatorGroundState.FirstClimb;
        
        sealWall.SetActive(true);
        
        SetMoving(true);
    }
    
    private void StartHoldUp()
    {
        _state = ElevatorGroundState.HoldUp;
        
        SetMoving(false);
        
        Invoke(nameof(StartFightOne), 4.0f);
    }
    
    private void StartFightOne()
    {
        _state = ElevatorGroundState.FightOne;
        
        firstFightBegin.Invoke();
    }

    private void StartFightTwo()
    {
        _state = ElevatorGroundState.FightTwo;
        
        secondFightBegin.Invoke();
    }
    
    private void StartSecondClimb()
    {
        _state = ElevatorGroundState.SecondClimb;
        
        SetMoving(true);
    }
    
    private void StartReachedTop()
    {
        _state = ElevatorGroundState.ReachedTop;
        
        SetMoving(false);
    }
    
    public void LightConditionMet()
    {
        if (lightConditionCount > 0)
        {
            lightConditionCount--;

            if (lightConditionCount <= 0)
            {
                Invoke(nameof(StartWaitingForPlayer), 1.0f);
            }
        }
    }

    public void EnemyConditionMet()
    {
        switch (_state)
        {
            case ElevatorGroundState.FightOne:
                if (firstConditionCount > 0)
                {
                    firstConditionCount--;

                    if (firstConditionCount <= 0)
                    {
                        // Wait a bit before climbing again...
                        Invoke(nameof(StartFightTwo), 3.0f);
                    }
                }

                break;
                
            case ElevatorGroundState.FightTwo:
                if (secondConditionCount > 0)
                {
                    secondConditionCount--;

                    if (secondConditionCount <= 0)
                    {
                        // Wait a bit before climbing again...
                        Invoke(nameof(StartSecondClimb), 3.0f);
                    }
                }

                break;
        }
    }

    private void MoveUpwards()
    {
        var current = transform;
        var position = current.position;
        
        transform.Translate(Vector3.up * (Time.deltaTime * climbSpeed));
    }

    private void Update()
    {
        switch (_state)
        {
            case ElevatorGroundState.WaitingForPlayer:
                if (activationDetector.GetPlayer() != null)
                {
                    StartFirstClimb();
                }

                break;
            
            case ElevatorGroundState.FirstClimb:
                if (transform.position.y < climbFightHeight)
                {
                    MoveUpwards();
                }
                else
                {
                    StartHoldUp();
                }

                break;
                
            case ElevatorGroundState.SecondClimb:
                if (transform.position.y < climbHeight)
                {
                    MoveUpwards();
                }
                else
                {
                    StartReachedTop();
                }

                break;
        }
    }
}
