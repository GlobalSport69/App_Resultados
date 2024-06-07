using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using LotteryResult.Extensions;
using LotteryResult.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Globalization;

namespace LotteryResult.Services
{
    public class RuletaActivaOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 7;
        private const int providerID = 26;
        private readonly ILogger<RuletaActivaOfficial> _logger;

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
                            Number = x.a.nro,
                            Animal = x.a.nombre
                        },
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.b.nro+" "+x.b.nombre,
                            Sorteo = "Triple B",
                            Number = x.b.nro,
                            Animal = x.b.nombre
                        },
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.c.nro+" "+x.c.nombre,
                            Sorteo = "Triple C",
                            Number = x.c.nro,
                            Animal = x.c.nombre
                        },
                        new LotteryDetail {
                            Time = x.hora,
                            Result = x.d.nro+" "+x.d.nombre,
                            Sorteo = "Triple D",
                            Number = x.d.nro,
                            Animal = x.d.nombre
                        }
                    };
                    return list;
                });


                var sorteos = await unitOfWork.LotteryRepository
                    .GetAllByAsync(x => x.ProductId == productID);

                var TripleA = sorteos.Where(x => x.Sorteo == "A").ToDictionary(x => x.LotteryHour.FormatTime());
                var TripleB = sorteos.Where(x => x.Sorteo == "B").ToDictionary(x => x.LotteryHour.FormatTime());
                var TripleC = sorteos.Where(x => x.Sorteo == "C").ToDictionary(x => x.LotteryHour.FormatTime());
                var TripleD = sorteos.Where(x => x.Sorteo == "D").ToDictionary(x => x.LotteryHour.FormatTime());


                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = RuletaActivaOfficial.FormatTime(item.Time.ToUpper());

                    Data.Models.Lottery sorteo = null;
                    if (item.Sorteo == "Triple A")
                        sorteo = TripleA[time];
                    if (item.Sorteo == "Triple B")
                        sorteo = TripleB[time];
                    if (item.Sorteo == "Triple C")
                        sorteo = TripleC[time];
                    if (item.Sorteo == "Triple D")
                        sorteo = TripleD[time];

                    return new Result
                    {
                        Result1 = item.Result,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        Sorteo = item.Sorteo,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        LotteryId = sorteo.Id,
                        PremierId = sorteo.PremierId,
                        Number = item.Number,
                        Animal = item.Animal
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(RuletaActivaOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(RuletaActivaOfficial));
                throw;
            }
        }

        private static string FormatTime(string time)
        {
            DateTime dateTime;
            var cultureInfo = new CultureInfo("en-US");

            if (DateTime.TryParseExact(time, "H:mm tt", cultureInfo, DateTimeStyles.None, out dateTime))
            {
                string formattedTime = dateTime.ToString("hh:mm tt", cultureInfo);
                return formattedTime;
            }

            throw new Exception("El formato de la hora proporcionada no es válido. "+time);
        }

    }
}

