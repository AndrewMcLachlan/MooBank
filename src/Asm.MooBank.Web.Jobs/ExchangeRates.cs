﻿using Asm.MooBank.Services;
using Microsoft.Azure.WebJobs;

namespace Asm.MooBank.Web.Jobs;
public class ExchangeRates(IExchangeRateService exchangeRateService)
{
#if DEBUG
    private const bool RunOnStartup = false;
#else
    private const bool RunOnStartup = false;
#endif

    public Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = RunOnStartup)] TimerInfo _) =>
        exchangeRateService.UpdateExchangeRates();
}
