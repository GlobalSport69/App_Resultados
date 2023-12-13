using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class ProductType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
