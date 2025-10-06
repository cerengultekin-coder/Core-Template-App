using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Common.Results;

public static class ResultExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> src, Func<TIn, TOut> map) =>
        src.IsSuccess ? Result<TOut>.Ok(map(src.Value!)) : Result<TOut>.Fail(src.Error);
}
