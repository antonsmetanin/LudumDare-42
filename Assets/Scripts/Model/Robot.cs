using System.Linq;
using Data;
using UniRx;
using Utils;

namespace Model
{
    public class Robot
    {
        public ReactiveProperty<int> MemorySize;
        public ReactiveCollection<Program> Programs;

        public UnityEngine.Transform Transform;

//        public IObservable<int> LeakedBytes;
//        public IObservable<int> ProducedBytes;

        public int GetLeakedBytes() => Programs.Sum(x => x.LeakedBytes.Value);
        public int GetProducedBytes() => Programs.Sum(x => x.ProducedBytes.Value);

        public Robot(RobotTemplate template)
        {
            MemorySize = new ReactiveProperty<int>(template.InitialMemorySize);
            Programs = new ReactiveCollection<Program>();

//            LeakedBytes = Programs.CountProperty()
//                .SelectMany(_ => Programs.Select(x => x.LeakedBytes).CombineLatest().Select(y => y.Sum()))
//                .ToReactiveProperty();
//
//            ProducedBytes = Programs.CountProperty()
//                .SelectMany(_ => Programs.Select(x => x.ProducedBytes).CombineLatest().Select(y => y.Sum()))
//                .ToReactiveProperty();
        }

        public class InstallProgramResult { }

        public class NotEnoughMemoryError : Error { }

        public Result<InstallProgramResult> InstallProgram(Program program, bool simulate = false)
        {
            if (Programs.Sum(x => x.MemorySize.Value) + program.MemorySize.Value > MemorySize.Value)
                return new NotEnoughMemoryError();

            if (!simulate)
                Programs.Add(new Program(program));

            return new InstallProgramResult();
        }
    }
}
