using Cysharp.Threading.Tasks;
using DG.Tweening;

public static class My_DOTween_Async_Extensions
{
    public static UniTask<bool> To_Kill_Cancel_Surpress(this Tween tween) {
        return tween.ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow();
    }

}
