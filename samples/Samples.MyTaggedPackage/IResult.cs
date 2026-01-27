// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace Samples.MyTaggedPackage;

public partial interface IResult
{
    bool IsSuccess { get; }
}

#if FEATURE_ASPNET
public partial interface IResult : Microsoft.AspNetCore.Http.IResult
{
}
#endif
