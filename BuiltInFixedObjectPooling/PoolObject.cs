using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolObject : MonoBehaviour
{
    ObjectPool<PoolObject> _pool;
    float _enemyBlockTimeMeter = 0;
    float _spanTime;
    bool _canDie = false;

    private void Awake() {
    }

    private void Update() {
        if (_enemyBlockTimeMeter < _spanTime) {
            _enemyBlockTimeMeter += Time.deltaTime;
        }
        else {
            _canDie = true;
        }

        if (_canDie) {
            //Debug.LogAssertion("I will die immediately");
            PushBack2Pool();
        }
    }
    public void AssignPool(ObjectPool<PoolObject> pool) {
        //Ait oldugu havuzun referansini bildir.
        _pool = pool;
        Dl.Wr("GO Havuza atandi.");
    }

    public void SetSpanTime(float spanTime) {
        _canDie = false;
        _enemyBlockTimeMeter = 0;
        _spanTime = spanTime;
    }

    public void PushBack2Pool() {
        //Nullable cunku bir prefab degilse ve sahnedeyse bu kisim Pool referansi atanamadigindan hata verir.
        _pool?.Release(this);
    }
}
