using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class Result
{
    public int Id { get; set; }

    public string Result1 { get; set; } = null!;

    public string Date { get; set; } = null!;

    public string Time { get; set; } = null!;

    public int ProductId { get; set; }

    public int ProviderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Provider Provider { get; set; } = null!;
}
