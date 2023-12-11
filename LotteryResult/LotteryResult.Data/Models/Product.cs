using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Enable { get; set; }

    public virtual ICollection<ProviderProduct> ProviderProducts { get; set; } = new List<ProviderProduct>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
