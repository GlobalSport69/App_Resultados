﻿using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
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
        private INotifyPremierService notifyPremierService;

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

        public RuletaActivaOfficial(IUnitOfWork unitOfWork, ILogger<RuletaActivaOfficial> logger, INotifyPremierService notifyPremierService)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
            this.notifyPremierService = notifyPremierService;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var data = await "https://latococa.com/proyect/api/v1"
                .AppendPathSegments("sorteo-publicados")
                .PostJsonAsync(new
                {
                    idLoteria = 1,
                    fecha= venezuelaNow.ToString("yyyy-MM-dd") /*2024-06-25*/
                })
                .ReceiveJson<RuletaActivaResponse>();

                if (data is null || !data.loteria.publicaciones.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(RuletaActivaOfficial));
                    return;
                }

                var json = data.loteria.publicaciones;

                var response = json
                    .SelectMany(x => {
                        var list = new List<LotteryDetail> {
                            new LotteryDetail {
                                Time = x.hora,
                                Sorteo = "A",
                                Result = x.a.nro,
                                Complement = x.a.nombre.Capitalize()
                            },
                            new LotteryDetail {
                                Time = x.hora,
                                Sorteo = "B",
                                Result = x.b.nro,
                                Complement = x.b.nombre.Capitalize()
                            },
                            new LotteryDetail {
                                Time = x.hora,
                                Sorteo = "C",
                                Result = x.c.nro,
                                Complement = x.c.nombre.Capitalize()
                            },
                            new LotteryDetail {
                                Time = x.hora,
                                Sorteo = "D",
                                Result = x.d.nro,
                                Complement = x.d.nombre.Capitalize()
                            }
                        };
                        return list;
                    })
                    .ToList();



                var oldResult = await unitOfWork.ResultRepository
                    .GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    var time = RuletaActivaOfficial.FormatTime(item.Time.ToUpper());
                    long? premierId = item.Sorteo == "D" ? lotteries[time] : null;

                    return new Result
                    {
                        Result1 = item.Result + " " + item.Complement,
                        Time = time,
                        Date = venezuelaNow.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        Sorteo = item.Sorteo,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS,
                        PremierId = premierId,
                    };
                })
                .OrderBy(x => x.Time)
                .ToList();


                var toUpdate = new List<Result>();
                foreach (var item in newResult)
                {
                    var found = oldResult.FirstOrDefault(y => item.Time == y.Time && item.Sorteo == y.Sorteo && item.Result1 != y.Result1);
                    if (found is null)
                        continue;

                    found.Result1 = item.Result1;
                    toUpdate.Add(found);
                }
                var toInsert = newResult.Where(x => !oldResult.Exists(y => x.Time == y.Time && x.Sorteo == y.Sorteo));
                var needSave = false;
                foreach (var item in toUpdate)
                {
                    unitOfWork.ResultRepository.Update(item);
                    needSave = true;
                }
                foreach (var item in toInsert)
                {
                    unitOfWork.ResultRepository.Insert(item);
                    needSave = true;
                }


                if (needSave)
                {
                    await unitOfWork.SaveChangeAsync();

                    if (toUpdate.Any())
                        notifyPremierService.Handler(toUpdate.Select(x => x.Id).ToList(), NotifyType.Update);
                    if (toInsert.Any())
                        notifyPremierService.Handler(toInsert.Select(x => x.Id).ToList(), NotifyType.New);
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

