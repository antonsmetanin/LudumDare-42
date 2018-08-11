using UniRx;

namespace Model
{
    public class Memory
    {
        public ReactiveProperty<int> Size;
        public ReactiveCollection<Program> Programs;
    }
}
