using UnityEngine;

public class ExplosiveMushroom : MonoBehaviour
{
    [SerializeField] private GameObject _mushroom;

    private float _timeScaling;
    private float _timeScaleX = 1;

    public void Explode()
    {
        _timeScaleX = Mathf.PingPong(_timeScaling, 1);
        _mushroom.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }

    public void Refilled()
    {
        _timeScaleX = 1;
    }

    private void Update()
    {
        _timeScaling = Time.time/10;
        _mushroom.transform.localScale = new Vector3 (_timeScaleX, Mathf.PingPong(_timeScaling, 0.2f) + 0.8f, Mathf.PingPong(_timeScaling, 0.2f) + 0.8f);
    }
}