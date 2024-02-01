using System;
using NUnit.Framework;
using Unity.Netcode;

namespace Modern
{
    public class DamageParameters
    {
        public int damage = 5;
        public int knockbackStrength = 200;
    }
    
    public delegate void DamageHandler();
    public delegate void DeathHandler();

    public class Entity: NetworkBehaviour
    {
        public NetworkVariable<int> health = new(15);
        public NetworkVariable<int> totalHealth = new(15);
        
        public NetworkVariable<bool> dead = new();

        public DamageHandler OnDamage = () => { };
        public DeathHandler OnDeath = () => { };

        [ClientRpc]
        void DamageClientRpc()
        {
            OnDamage();
        }
        
        [ClientRpc]
        void DeathClientRpc()
        {
            OnDeath();
        }
        
        public void Reset()
        {
            health.Value = totalHealth.Value;
            dead.Value = false;
        }
        
        public void Damage(DamageParameters parameters)
        {
            Assert.True(IsServer);
            
            if (dead.Value)
            {
                return;
            }

            health.Value -= parameters.damage;
            
            DamageClientRpc();
            
            if (health.Value < 0)
            {
                dead.Value = true;
                
                DeathClientRpc();
            }
        }
    }
}