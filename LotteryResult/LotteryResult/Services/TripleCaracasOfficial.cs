﻿using Flurl;
using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;
using System.Globalization;

namespace LotteryResult.Services
{
    public class TripleCaracasOfficial : IGetResult
    {
        private IUnitOfWork unitOfWork;
        public const int productID = 3;
        private const int providerID = 9;
        private readonly ILogger<TripleCaracasOfficial> _logger;
        private Dictionary<string, long> TripleA = new Dictionary<string, long>
        {
            { "01:00 PM", 146 },
            { "04:30 PM", 164 },
            { "07:00 PM", 149 }
        };

        private Dictionary<string, long> TripleB = new Dictionary<string, long>
        {
            { "01:00 PM", 147 },
            { "04:30 PM", 165 },
            { "07:00 PM", 150 }
        };

        private Dictionary<string, long> TripleC = new Dictionary<string, long>
        {
            { "01:00 PM", 148 },
            { "04:30 PM", 166 },
            { "07:00 PM", 151 }
        };

        public static Dictionary<string, string> ZodiacSigns = new Dictionary<string, string>
        {
            {"ARI", "ARIES"},
            {"TAU", "TAURO"},
            {"GEM", "GEMINIS"},
            {"CAN", "CANCER"},
            {"LEO", "LEO"},
            {"VIR", "VIRGO"},
            {"LIB", "LIBRA"},
            {"ESC", "ESCORPIO"},
            {"SAG", "SAGITARIO"},
            {"CAP", "CAPRICORNIO"},
            {"ACU", "ACUARIO"},
            {"PIS", "PISCIS"}
        };


        public TripleCaracasOfficial(IUnitOfWork unitOfWork, ILogger<TripleCaracasOfficial> logger)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;

                var results = await "http://165.227.185.69:8000"
                .AppendPathSegments("api", "resultados")
                .SetQueryParams(new
                {
                    fecha = venezuelaNow.ToString("yyyy-MM-dd"),
                })
                .GetJsonAsync<TripleCaracasOfficialResponse>();

                var response = results.resultados
                    .GroupBy(x => x.producto_juego.order)
                    .OrderBy(x => x.Key)
                    .Where(x => x.Key < 4)
                    .SelectMany(x => x)
                    .ToList();

                if (!response.Any())
                {
                    _logger.LogInformation("No se obtuvieron resultados en {0}", nameof(TripleCaracasOfficial));
                    return;
                }

                var oldResult = await unitOfWork.ResultRepository.GetAllByAsync(x => x.ProviderId == providerID && x.CreatedAt.Date == venezuelaNow.Date);
                oldResult = oldResult.OrderBy(x => x.Time).ToList();

                var newResult = response.Select(item => {
                    // Crear un DateTime a partir de la cadena de texto "13:00:00"
                    DateTime dt = DateTime.ParseExact(item.sorteo.hora, "HH:mm:ss", CultureInfo.InvariantCulture);
                    // Convertir a formato de 12 horas (AM/PM)
                    string time = dt.ToString("hh:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    long premierId = 0;
                    if (item.producto_juego.nombre == "Triple A") { 
                        premierId = TripleA[time];
                    }

                    if (item.producto_juego.nombre == "Triple B"){ 
                        premierId = TripleB[time];
                    }

                    if (item.producto_juego.nombre == "Zodiacal Caracas") { 
                        premierId = TripleC[time];
                    }

                    var complement = string.IsNullOrEmpty(item.resultado_elemento) ? null : $" {ZodiacSigns[item.resultado_elemento]}";
                    var resultado = item.resultado + complement ?? "";

                    return new Result
                    {
                        Result1 = resultado,
                        Time = time,
                        Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        ProductId = productID,
                        ProviderId = providerID,
                        ProductTypeId = (int)ProductTypeEnum.TRIPLES,
                        Sorteo = item.producto_juego.nombre,
                        PremierId = premierId,
                        //Number: item.resultado,
                        //Complement: complement
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
                    _logger.LogInformation("No hubo cambios en los resultados de {0}", nameof(TripleCaracasOfficial));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(TripleCaracasOfficial));
                throw;
            }
        }
    }
}
