using UniRx;

namespace Model
{
    public class Robot
    {
        public ReactiveProperty<int> Size;
        public ReactiveCollection<Program> Programs;
    }
}
