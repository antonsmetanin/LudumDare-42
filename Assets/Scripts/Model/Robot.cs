using System.Linq;
using UniRx;
using Utils;

namespace Model
{
    public class Robot
    {
        public ReactiveProperty<int> MemorySize;
        public ReactiveCollection<Program> Programs;

        public UnityEngine.Transform Transform;

        public class InstallProgramResult { }

        public class NotEnoughMemoryError : Error { }

        public Result<InstallProgramResult> InstallProgram(Program program, bool simulate = false)
        {
            if (Programs.Sum(x => x.Size.Value) + program.Size.Value > MemorySize.Value)
                return new NotEnoughMemoryError();

            if (!simulate)
                Programs.Add(new Program(program));

            return new InstallProgramResult();
        }
    }
}
