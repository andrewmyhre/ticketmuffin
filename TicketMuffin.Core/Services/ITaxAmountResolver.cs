﻿namespace GroupGiving.Core.Services
{
    public interface ITaxAmountResolver
    {
        decimal LookupTax();
        decimal LookupTax(string country);
    }
}