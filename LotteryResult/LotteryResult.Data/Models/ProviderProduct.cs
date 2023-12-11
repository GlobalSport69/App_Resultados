using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class ProviderProduct
{
    public int Id { get; set; }

    public int ProviderId { get; set; }

    public int ProductId { get; set; }

    public string? CronExpression { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Provider Provider { get; set; } = null!;
}
