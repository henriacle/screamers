using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 100.0f)]  private float       _playerHealth   = 100.0f;
    [SerializeField] private CameraBloodEffect                  _cameraBloodEffect = null;
    [SerializeField]                        private GameObject  _healthBar      = null;
    [SerializeField]                                float       _currentHealth  = 0.0f;
                                                    RectTransform _lifeRect     = null;
                                                    float _lifeRectWidth        = 0.0f;
                                                    float _lifeRectHeight       = 0.0f;


    public float playerHealth { get { return _playerHealth; }           set { _playerHealth     = value; } }
    public float playerCurrentHealth { get { return _currentHealth; }   set { _currentHealth    = value; } }

    private void Start()
    {
        _lifeRect = _healthBar.GetComponent<RectTransform>();
        if(_lifeRect)
        {
            _lifeRectWidth  = _lifeRect.rect.width;
            _lifeRectHeight = _lifeRect.rect.height;
            playerCurrentHealth = playerHealth;
        }
    }

    public void gameObjectHit(HitInfo hit)
    {
        takeDamage(hit.hit.point, 5.0f, hit.force, hit.hit.rigidbody);
    }

    public void takeDamage(Vector3 position, float force, float damage, Rigidbody bodyPart, int hitDirection = 0)
    {
        doDamage(damage);
    }

    public void meleeDamage(float damage)
    {
        doDamage(damage);
    }

    private void doDamage(float damage)
    {

        playerCurrentHealth = _currentHealth - damage;
        //_lifeRectWidth = 250 - (250 - (250 * playerCurrentHealth / 100));
        //_lifeRect.sizeDelta = new Vector2(_lifeRectWidth, _lifeRect.rect.height);
        //_lifeRect.ForceUpdateRectTransforms();

        if (_cameraBloodEffect != null)
        {
            _cameraBloodEffect.minBloodAmount = (1.0f - playerCurrentHealth / 100.0f);
            _cameraBloodEffect.bloodAmount = Mathf.Min(_cameraBloodEffect.minBloodAmount + 0.3f, 1.0f);
        }

        if (playerCurrentHealth <= 0)
        {
            SceneManager.LoadScene(0);
        }
    }
}
