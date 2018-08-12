using System;
using JetBrains.Annotations;

namespace Utils
{
	public abstract class Error { }

    public class StringError : Error
    {
        public readonly string Error;

        public StringError(string error)
        {
            Error = error;
        }

        public override string ToString() => Error;
    }

    public sealed class None
    {
        public static readonly None _ = null;
    }

    public interface IResult<out TOk>
    {
        TOk Value { get; }
        [CanBeNull] Error Error { get; }
    }

    public struct Result<TOk> : IResult<TOk>
    {
        public readonly TOk Value;
        [CanBeNull] public readonly Error Error;

        public Result(TOk value)
        {
            Value = value;
            Error = null;
        }

        public Result(Error error)
        {
            Value = default(TOk);
            Error = error;
        }

        TOk IResult<TOk>.Value => Value;
        [CanBeNull] Error IResult<TOk>.Error => Error;

        public TOk OrElse(TOk elseValue)
        {
            return Error != null ? elseValue : Value;
        }

        public TOk OrElse(Func<Error, TOk> handler)
        {
            return Error != null ? handler(Error) : Value;
        }

        public TOk Unwrap()
        {
            if (Error != null)
                throw new Exception(Error.ToString());

            return Value;
        }

        public static implicit operator Result<TOk>(Error error) => new Result<TOk>(error);
        public static implicit operator Result<TOk>(TOk value) => new Result<TOk>(value);

        public Result<TBase> Cast<TBase>()
            where TBase : class
        {
            if (Error != null)
                return Error;

            return Value as TBase;
        }

        public Result<TOut> Select<TOut>(Func<TOk, TOut> selector) => Error ?? new Result<TOut>(selector(Value));
    }
}
