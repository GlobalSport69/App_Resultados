using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class Lottery
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int ProductId { get; set; }

    public int PremierId { get; set; }

    public TimeOnly LotteryHour { get; set; }

    public virtual Product Product { get; set; } = null!;
}
