using System.Threading;
using UniRx;
using UnityEngine;

public enum UniRx_Pool_Type
{
    None,
    Goblin,
    Human,
}

public class UniRx_Pool_Component : MonoBehaviour
{
    public UniRx_Pool_Type type;
    public string poolKey;

    private CompositeDisposable _com = null;

    public bool HasCompositeDisposable {
        get {
            return _com != null;
        }
    }

    public CompositeDisposable GetCompositeDisposableOnReturn {
        get {
            if (_com == null) {
                _com = new CompositeDisposable();
            }
            return _com;
        }
    }

    private CancellationTokenSource _cancel = null;

    public bool HasCancellationTokenSource {
        get {
            return _cancel != null;
        }
    }

    public void CallCancel() {
        _cancel?.Cancel();
        _cancel?.Dispose();
        _cancel = null;
    }

    public CancellationToken GetCancellationTokenOnReturn {
        get {
            if (_cancel == null) {
                _cancel = new CancellationTokenSource();
            }

            return _cancel.Token;
        }
    }

}

