using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace Utils
{
    public class CollectionFromInt : IReadOnlyReactiveCollection<Unit>
    {
        private IReadOnlyReactiveProperty<int> _count;
        private Subject<CollectionAddEvent<Unit>> _observeAdd = new Subject<CollectionAddEvent<Unit>>();
        private Subject<CollectionRemoveEvent<Unit>> _observeRemove = new Subject<CollectionRemoveEvent<Unit>>();

        private int _currentCount;

        public CollectionFromInt(IReadOnlyReactiveProperty<int> count)
        {
            _count = count;

            _currentCount = _count.Value;

            count.Subscribe(newCount =>
            {
                var difference = newCount - _currentCount;

                if (difference > 0)
                    for (var i = 0; i < difference; i++)
                        _observeAdd.OnNext(new CollectionAddEvent<Unit>(_currentCount + i, Unit.Default));
                else if (difference < 0)
                    for (var i = 0; i < difference; i++)
                        _observeRemove.OnNext(new CollectionRemoveEvent<Unit>(_currentCount - i - 1, Unit.Default));

                _currentCount = newCount;
            });
        }

        public Unit this[int index] => Unit.Default;
        public int Count => _count.Value;

        public IEnumerator<Unit> GetEnumerator()
        {
            for (var i = 0; i < _count.Value; i++)
                yield return Unit.Default;
        }

        public IObservable<CollectionAddEvent<Unit>> ObserveAdd() => _observeAdd;

        public IObservable<int> ObserveCountChanged(bool notifyCurrentCount = false)
        {
            return _count;
        }

        public IObservable<CollectionMoveEvent<Unit>> ObserveMove()
        {
            throw new NotImplementedException();
        }

        public IObservable<CollectionRemoveEvent<Unit>> ObserveRemove() => _observeRemove;
        public IObservable<CollectionReplaceEvent<Unit>> ObserveReplace()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> ObserveReset()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}