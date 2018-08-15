using UnityEngine;

namespace Utils
{
    public interface IFactory<T>
    {
        T Create();
        void Destroy(T t);
    }

    public class Instantiator<T> : IFactory<T>
        where T : Component
    {
        private readonly T _template;

        public Instantiator(T template) => _template = template;
        public T Create() => Object.Instantiate(_template);
        public void Destroy(T t) => Object.Destroy(t.gameObject);
    }
}