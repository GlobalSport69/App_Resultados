using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class Provider
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public bool Enable { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<ProviderProduct> ProviderProducts { get; set; } = new List<ProviderProduct>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
