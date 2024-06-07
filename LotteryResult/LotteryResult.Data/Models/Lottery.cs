using System;
using System.Collections.Generic;

namespace LotteryResult.Data.Models;

public partial class Lottery
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int ProductId { get; set; }

    public long? PremierId { get; set; }

    public TimeOnly LotteryHour { get; set; }

    /// <summary>
    /// En esta columna se especifica si el sorteo es A,B,C,D, etc
    /// </summary>
    public string? Sorteo { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual Result Result { get; set; } = null!;

}
