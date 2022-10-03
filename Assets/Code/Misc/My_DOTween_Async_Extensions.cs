using Cysharp.Threading.Tasks;
using DG.Tweening;

public static class My_DOTween_Async_Extensions
{
    public static UniTask<bool> If_Cancel(this Tween tween) {
        return tween.ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow();
    }

    public static UniTask<bool> Not_Cancel => UniTask.Create(async () => false);

}
