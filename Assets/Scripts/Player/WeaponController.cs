using FishNet.Object;
using TMPro;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private Weapon currentWeapon;
    [SerializeField] private GameObject[] handsComponents;

    public bool IsCanShoot { get; set; } = true;
    public Weapon CurrentWeapon => currentWeapon;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            foreach (GameObject m_handsComponent in handsComponents)
            {
                m_handsComponent.layer = 3;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (IsCanShoot) 
        {
            if (Input.GetMouseButton(0))
            {
                currentWeapon.UseFire();
            }
            if ((Input.GetMouseButton(1)) && (!currentWeapon.IsReloading))
            {
                currentWeapon.IsAiming = true;
            }
            if (Input.GetKey(KeyCode.R))
            {
                currentWeapon.UseReload();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            currentWeapon.IsAiming = false;
        }
    }
}
