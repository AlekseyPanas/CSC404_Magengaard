using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMagicController : MonoBehaviour
{
    public GameObject projectile;
    public GameObject player;
    public GameObject playerCam;
    [SerializeField]
    private LayerMask layermask;
    private Vector3 difference;
    private Vector3 direction;
    public GestureSystem gs;
    public bool canShoot = false;
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            if(canShoot){
                ShootProjectile(projectile);
            }
        }
    }

    void ShootProjectile(GameObject projectileToShoot) {
        direction = new Vector2(0,0);
        GameObject spawnedProjectile = Instantiate(projectileToShoot, player.transform.position, Quaternion.identity);
        Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
        {
            difference = hit.point - player.transform.position;
            direction = new Vector3(difference.x, 0, difference.z).normalized;
        }
        float projectileSpeed = projectileToShoot.GetComponent<Projectile>().speed;
        float projectileLifeTime = projectileToShoot.GetComponent<Projectile>().lifeTime;
        spawnedProjectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;
        spawnedProjectile.GetComponent<Projectile>().player = gameObject;
        Destroy(spawnedProjectile, projectileLifeTime);
        canShoot = false;
        Invoke("ResetGesture", 1);
    }

    void ResetGesture(){
        gs.isReadyToFire = false;
    }
}
