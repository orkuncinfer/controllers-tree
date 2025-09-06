using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public static class UniTaskHelpers
{
    public static async UniTask ToUniTaskWithImmediateCancel(this Tween tween, CancellationToken token)
    {
        await using var _ = token.Register(() =>
        {
            if (tween.IsActive())
            {
                tween.Kill();
            }
        }, true);

        await tween.ToUniTask(TweenCancelBehaviour.Kill, token);
    }
}