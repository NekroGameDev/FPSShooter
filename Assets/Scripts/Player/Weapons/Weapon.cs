using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float damage;
    [SerializeField] protected float fireRate;
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected float reloadTime;
    private float nextTime2Fire = 0f;
    private int currentAmmo;

    [Header("Sway")]
    [SerializeField] private Transform swayObject;
    [SerializeField] private float smooth;
    [SerializeField] private float swayMultiplier;
    private const string mouseX = "Mouse X";
    private const string mouseY = "Mouse Y";

    [Header("Aiming")]
    [SerializeField] private Transform sightTarget;
    [SerializeField] private Transform defaultWeaponPosition;
    [SerializeField] private float sightOffset;
    [SerializeField] private float aimingTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;

    [Header("Recoil")]
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;
    [SerializeField] private float kickBackZ;
    [SerializeField] private float snappiness;
    [SerializeField] private float returnAmount;
    private Vector3 currentPosition;
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 targetPosition;
    private Vector3 initialGunPosition;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip shootAudio;
    [SerializeField] private AudioClip reloadAudio;

    [Header("Animatios")]
    [SerializeField] protected Animator _animator;
    protected const string animShoot = "Shoot";
    protected const string animReload = "ReloadRifle";
    protected const string animRun = "IsRun";
    protected const string animSliding = "IsSliding";

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Instantiate")]
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletForce;

    [Header("Componetns")]
    [SerializeField] private CameraController cameraController;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textAmmo;

    public bool IsAiming { get; set; }
    public bool IsReloading { get; private set; }
    public int CurrentAmmo
    {
        get
        {
            return currentAmmo;
        }
        private set
        {
            currentAmmo = value;
            textAmmo.text = $"{currentAmmo}/{maxAmmo}";
        }
    }

    private void Start()
    {
        CurrentAmmo = maxAmmo;
        initialGunPosition = transform.localPosition;
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        CalculateWeaponSway();
        CalculateAiming();
        CalculateWeaponRecoil();
    }
    
    [ObserversRpc(RunLocally = true)]
    public void UseFire()
    {
        if ((Time.time >= nextTime2Fire) && (currentAmmo > 0) && (!IsReloading))
        {
            nextTime2Fire = Time.time + 1f / fireRate;
            CurrentAmmo--;

            Fire();
        }
    }

    [ObserversRpc(RunLocally = true)]
    public void UseReload()
    {
        if (currentAmmo >= maxAmmo) { return; }

        Reload();
    }

    #region VirtualVoids

    protected virtual void Fire()
    {
        SpawnBulletRPC();

        Recoil();
    }

    [ServerRpc(RunLocally = false)]
    private void SpawnBulletRPC()
    {
        //Ray m_ray = new Ray(cameraController.GetCameraPivot.position, cameraController.GetCameraPivot.forward);
        //RaycastHit m_hit;

        //if (Physics.Raycast(m_ray, out m_hit)) { bulletSpawnPoint.LookAt(m_hit.point); }
        //else { bulletSpawnPoint.localRotation = Quaternion.identity; }
        
        GameObject m_bullet = Instantiate(bulletPrefab.gameObject, bulletSpawnPoint.position, Quaternion.identity);
        Spawn(m_bullet);
        m_bullet.TryGetComponent(out Rigidbody m_rigidbody);
        m_rigidbody.AddForce(bulletSpawnPoint.forward * bulletForce, ForceMode.Impulse);
        Destroy(m_bullet, 5f);
    }

    protected virtual void Reload()
    {
        if (IsReloading) { return; }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        IsAiming = false;
        IsReloading = true;
        RPCReloadEffects();

        yield return new WaitForSeconds(reloadTime);

        IsReloading = false;
        CurrentAmmo = maxAmmo;
    }

    [ObserversRpc(RunLocally = true)]
    private void RPCReloadEffects()
    {
        _animator.Play(animReload);
        RPCReloadEffectsServer();
    }

    [ServerRpc]
    private void RPCReloadEffectsServer()
    {
        _animator.Play(animReload);
    }

    #endregion

    #region Aiming

    private void CalculateAiming()
    {
        Vector3 m_targetPosition = defaultWeaponPosition.position;

        if (IsAiming)
        {
            m_targetPosition = cameraController.GetCameraPivot.position + (swayObject.position - sightTarget.position) + (cameraController.GetCameraPivot.forward * sightOffset);
        }

        weaponSwayPosition = swayObject.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, m_targetPosition, ref weaponSwayPositionVelocity, aimingTime);
        swayObject.position = weaponSwayPosition;
    }

    #endregion

    #region Recoil

    private void CalculateWeaponRecoil()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnAmount * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);

        KickBack();
    }

    [ObserversRpc(RunLocally = true)]
    private void Recoil()
    {
        targetPosition -= Vector3.forward * kickBackZ;
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }

    private void KickBack()
    {
        targetPosition = Vector3.Lerp(targetPosition, initialGunPosition, returnAmount * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, snappiness * Time.fixedDeltaTime);
        transform.localPosition = currentPosition;
    }

    #endregion

    #region Sway

    private void CalculateWeaponSway()
    {
        float m_mouseX = Input.GetAxisRaw(mouseX) * swayMultiplier;
        float m_mouseY = Input.GetAxisRaw(mouseY) * swayMultiplier;

        Quaternion m_rotationX = Quaternion.AngleAxis(-m_mouseY, Vector3.right);
        Quaternion m_rotationY = Quaternion.AngleAxis(m_mouseX, Vector3.up);

        Quaternion m_targetRotation = m_rotationX * m_rotationY;

        swayObject.localRotation = Quaternion.Slerp(swayObject.localRotation, m_targetRotation, smooth * Time.deltaTime);
    }

    #endregion

    #region Animatitions

    public void RunAnim(bool m_value)
    {
        _animator.SetBool(animRun, m_value);
    }

    public void SlidingAnim(bool m_value)
    {
        _animator.SetBool(animSliding, m_value);
    }

    #endregion
}
