﻿using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mail_summarizer_api.Middleware.Context;

public interface IFunctionContextAccessor
{
    FunctionContext? FunctionContext { get; set; }
}