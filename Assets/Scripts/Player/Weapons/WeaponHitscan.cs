using FishNet.Object;
using TMPro;
using UnityEngine;

public class WeaponHitscan : Weapon
{
    [Header("Hitscan settings")]
    [SerializeField] private Transform _camera;
    [SerializeField] private LayerMask layerMask;

    protected override void Fire()
    {
        base.Fire();

        CastRay();
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void CastRay()
    {
        Ray m_ray = new Ray(_camera.position, _camera.forward);
        RaycastHit m_hit;

        if (Physics.Raycast(m_ray, out m_hit, Mathf.Infinity, layerMask))
        {
            if (m_hit.transform.TryGetComponent(out PlayerStats m_playerStats))
            {             
                m_playerStats.TakeDamage(damage);
            }
        }
    }
}
