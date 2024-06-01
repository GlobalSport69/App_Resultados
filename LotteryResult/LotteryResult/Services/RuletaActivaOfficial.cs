using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace LotteryResult.Services
{
    public class RuletaActivaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 7;
        private const int providerID = 26;
        private readonly ILogger<RuletaActivaOfficial> _logger;

        private Dictionary<string, long> lotteries = new Dictionary<string, long>
        {
            { "09:00 AM", 24 },
            { "10:00 AM", 25 },
            { "11:00 AM", 26 },
            { "12:00 PM", 27 },
            { "01:00 PM", 28 },
            { "02:00 PM", 224 },
            { "03:00 PM", 29 },
            { "04:00 PM", 30 },
            { "05:00 PM", 31 },
            { "06:00 PM", 32 },
            { "07:00 PM", 33 }
        };

        public RuletaActivaOfficial(IUnitOfWork unitOfWork, ILogger<RuletaActivaOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var data = await "https://apis.sigtve.com/api/v1"
                .AppendPathSegments("sorteo-publicados")
                .PostJsonAsync(new
                {
                    idLoteria = 1
                    //fecha = null
                })
                .ReceiveJson<RuletaActivaResponse>();

                if (data is null || !data.loteria.publicaciones.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(LaGranjitaOfficial));
                    return;
                }

                var response = data.loteria.publicaciones.SelectMany(x => {
                    var list = new List<LotteryDetail> {
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.a.nro+" "+x.a.nombre,
                            Sorteo = "Triple A",
                        },
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.b.nro+" "+x.b.nombre,
                            Sorteo = "Triple B",
                        },
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.c.nro+" "+x.c.nombre,
                            Sorteo = "Triple C",
                        },
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.d.nro+" "+x.d.nombre,
                            Sorteo = "Triple D",
                        }
                    };
                    return list;
                });



                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = LaGranjitaTerminalOfficial.FormatTime(item.Time.ToUpper());
                    var premierId = lotteries[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        Sorteo = item.Sorteo,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = premierId,
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();

                var needSave = false;
                // no hay resultado nuevo
                var len = oldResult.Count();
                if (len == newResult.Count())
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (oldResult[i].Time == newResult[i].Time && oldResult[i].Result1 != newResult[i].Result1)
                        {
                            oldResult[i].Result1 = newResult[i].Result1;
                            unitOfWork.ResultRepository.Update(oldResult[i]);
                            needSave = true;
                        }
                    }
                }

                // hay resultado nuevo
                if (newResult.Count() > len)
                {
                    var founds = newResult.Where(x => !oldResult.Any(y => y.Time == x.Time));

                    foreach (var item in founds)
                    {
                        unitOfWork.ResultRepository.Insert(item);
                        needSave = true;
                    }
                }

                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();
                }

                if (!needSave)
                {
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(LottoActivoOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(RuletaActivaOfficial));
                throw;
            }
        }
    }
}

