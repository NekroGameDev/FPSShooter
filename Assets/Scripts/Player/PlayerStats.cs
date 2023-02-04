using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth;
    private float health;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textHealth;
    [SerializeField] private Slider sliderHealth;

    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            sliderHealth.value = health / 100;
            //textHealth.text = health.ToString();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Health = maxHealth;
    }

    public void TakeDamage(float m_value)
    {
        TakeDamageRPC(m_value);
    }

    [ObserversRpc]
    public void TakeDamageRPC(float m_value)
    {
        if (Health - m_value > 0)
        {
            Health -= m_value;
        }
        else
        {
            Transform m_respawnPosition = GameManager.Instance.SpawnPoints[Random.Range(0, GameManager.Instance.SpawnPoints.Length)];
            transform.position = m_respawnPosition.position;

            Health = maxHealth;
        }
    }
}
