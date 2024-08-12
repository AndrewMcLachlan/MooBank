
int age = 45;
decimal initialBalance = 350000m;
decimal annualSalary = 210000m;
decimal annualContributionRate = 0.15m; // 9%
decimal contributionTaxRate = 0.15m;
int retirementAge = 67;
decimal averageAnnualReturn = 0.075m; // 6%
decimal inflationRate = 0.025m; // 2.5%
decimal annualFees = 372; // Annual fees
decimal insurancePremium = 364; // Annual insurance premium

decimal estimatedSuperBalance = CalculateSuperBalance(initialBalance, annualSalary,
                                                    annualContributionRate, contributionTaxRate, retirementAge,
                                                    averageAnnualReturn, inflationRate, annualFees, insurancePremium, age);
Console.WriteLine($"Estimated superannuation balance at retirement: {estimatedSuperBalance:C}");


static decimal CalculateSuperBalance(decimal initialBalance, decimal annualSalary,
                                     decimal annualContributionRate, decimal contributionTaxRate, int retirementAge,
                                     decimal averageAnnualReturn, decimal inflationRate,
                                     decimal annualFees, decimal insurancePremium, int currentAge)
{
    int yearsUntilRetirement = retirementAge - currentAge;

    // Calculate the annual contribution
    decimal annualContribution = annualSalary * annualContributionRate;
    annualContribution -= annualContribution * contributionTaxRate;
    decimal combinedFees = annualFees + insurancePremium;


    // Future value of initial balance
    decimal futureValuePrincipal = initialBalance * (decimal)Math.Pow((double)(1 + averageAnnualReturn), yearsUntilRetirement);

    // Future value of contributions
    decimal futureValueContributions = annualContribution * ((decimal)Math.Pow((double)(1 + averageAnnualReturn), yearsUntilRetirement) - 1) / averageAnnualReturn;

    // Total future value before fees
    decimal futureValueBeforeFees = futureValuePrincipal + futureValueContributions;

    // Adjust for inflation (subtracting fees each year)
    decimal adjustedFutureValue = AdjustForFeesAndInflation(futureValueBeforeFees, combinedFees, inflationRate, yearsUntilRetirement);

    return adjustedFutureValue;
}

static decimal AdjustForFeesAndInflation(decimal futureValue, decimal annualFees, decimal inflationRate, int years)
{
    // Total fees to be deducted over the investment period
    decimal totalFees = annualFees * years;

    // Adjust future value for fees and inflation
    decimal futureValueAfterFees = futureValue - totalFees;
    decimal adjustedFutureValue = futureValueAfterFees * (decimal)Math.Pow((double)(1 - inflationRate), years);

    return adjustedFutureValue;
}
