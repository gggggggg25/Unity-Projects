using System.Numerics;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectIleGun : MonoBehaviour
{
    public GameObject bullet;
    public float shootforce;
    public float upwardforce;
    public float timeBetweenShooting, timeBetweenShots, spread, reloadtime;
    public int magazineSize, bulletsPerTap;
    public bool automatic;
    int bulletsLeft, bulletsShot;
    bool shooting;
    bool readytoshoot;
    bool reloading;
    public Camera Cam;
    public Transform attackPoint;
    public bool allowInvoke = true;
    public GameObject muzzleflash;
    public TextMeshProUGUI ammunitionDisplay;


    private void Awake()
    {
        bulletsLeft = magazineSize;
        readytoshoot = true;
    }
    void Start()
    {
    }

    void Update()
    {
        myInput();
        if(ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize);
        }
        if (reloading)
        {
            ammunitionDisplay.SetText("Reloading...");
        }

        if(muzzleflash != null)
        {
            Instantiate(muzzleflash, attackPoint.position, UnityEngine.Quaternion.identity);
        }
    }

    private void myInput()
    {
        if (automatic) //check if allowed to shoot
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if(Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) //reload
        {
            Reload();
        }

        if(readytoshoot && shooting && !reloading && bulletsLeft <= 0) //auto reload if no bullets
        {
            Reload();
        }

        if(readytoshoot && shooting && !reloading && bulletsLeft > 0) // start shooting
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readytoshoot = false;

        //find bullet hit position
        Ray ray = Cam.ViewportPointToRay(new UnityEngine.Vector3(0.5f, 0.5f, 0)); // raycast in middle of screen
        RaycastHit Hit;
        UnityEngine.Vector3 targetpoint;
        if(Physics.Raycast(ray, out Hit))
        {
            targetpoint = Hit.point;
        }
        else
        {
            targetpoint = ray.GetPoint(100); // gets a point far away from player if no target
        }

        UnityEngine.Vector3 directionWithoutSpread = targetpoint - attackPoint.position; //find direction to target
        float x = UnityEngine.Random.Range(-spread, spread); //create random x spread
        float y = UnityEngine.Random.Range(-spread, spread); //create random y spread
        UnityEngine.Vector3 directionWithSpread = directionWithoutSpread + new UnityEngine.Vector3(x, y, 0);

        GameObject currentBullet = Instantiate(bullet, attackPoint.position, UnityEngine.Quaternion.identity); //spawn bullet

        currentBullet.transform.forward = directionWithSpread.normalized; //rotate

        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootforce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(Cam.transform.up * upwardforce, ForceMode.Impulse);

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if(bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readytoshoot = true;
        allowInvoke = true;
    }
    
    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadtime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

}
