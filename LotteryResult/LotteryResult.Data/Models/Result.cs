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

    /// <summary>
    /// se almacena el nombre del sorteo en caso de que el poroducto tenga mas de un sorteo por hora, ejemplo tripla A y triple B a las 7:00
    /// </summary>
    public string? Sorteo { get; set; }

    public int? ProductTypeId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductType? ProductType { get; set; }

    public virtual Provider Provider { get; set; } = null!;
}
