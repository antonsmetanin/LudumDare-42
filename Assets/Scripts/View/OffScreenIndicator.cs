using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace View
{
    public class OffScreenIndicator : MonoBehaviour, IDisposable
    {
        private CompositeDisposable _disposable;

        public void Show(Camera targetCamera, Transform targetTransform, Rect rect)
        {
            _disposable = new CompositeDisposable();

            Observable.EveryUpdate()
                .Subscribe(_ => {
                    var realPosition = (Vector2)targetCamera.WorldToScreenPoint(targetTransform.position);
                    var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

                    var intersectionPoint = GetSegments(rect)
                        .Select(segment => MathHelpers.Intersect(segment.start, segment.end, screenCenter, realPosition))
                        .FirstOrDefault(x => x != null);

                    gameObject.SetActive(intersectionPoint != null);

                    if (intersectionPoint != null)
                    {
                        transform.position = intersectionPoint.Value;
                        transform.up = realPosition - screenCenter;
                    }
                })
                .AddTo(_disposable);
        }

        private IEnumerable<(Vector2 start, Vector2 end)> GetSegments(Rect rect)
        {
            yield return (new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax));
            yield return (new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin));
            yield return (new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax));
            yield return (new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax));
        }

        public void Dispose()
        {
            _disposable.Dispose();
            gameObject.SetActive(false);
        }
    }
}