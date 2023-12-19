﻿using Flurl.Http;
using LotteryResult.Data.Abstractions;
using LotteryResult.Dtos;
using LotteryResult.Enum;
using PuppeteerSharp;

namespace LotteryResult.Services
{
    public class SelvaPlusOfficial : IGetResult
    {
        private IResultRepository resultRepository;
        private IUnitOfWork unitOfWork;
        private const int selvaPlusID = 10;
        private const int selvaPlusProviderID = 10;
        private readonly ILogger<SelvaPlusOfficial> _logger;

        public SelvaPlusOfficial(IResultRepository resultRepository, IUnitOfWork unitOfWork, ILogger<SelvaPlusOfficial> logger)
        {
            this.resultRepository = resultRepository;
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handler()
        {
            try
            {
                // Obtén la zona horaria de Venezuela
                TimeZoneInfo venezuelaZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");

                // Obtén la fecha y hora actual en UTC
                DateTime utcNow = DateTime.UtcNow;

                // Convierte la fecha y hora actual a la zona horaria de Venezuela
                DateTime venezuelaNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, venezuelaZone);


                var response = await "https://www.apuestanext.com/apuestanext.com/aplicativo/accion/apis/resultados.php"
                .PostJsonAsync(new
                {
                    id_tipo_loteria = 94,
                    fecha = venezuelaNow.ToString("yyyy-MM-dd")
                })
                .ReceiveJson<List<GuacharoOfficialResponse>>();

                if (!response.Any()) return;

                var oldResult = await resultRepository
                    .GetAllByAsync(x => x.ProviderId == selvaPlusProviderID &&
                        x.CreatedAt.ToUniversalTime().Date == DateTime.Now.ToUniversalTime().Date);
                foreach (var item in oldResult)
                {
                    resultRepository.Delete(item);
                }

                await unitOfWork.SaveChangeAsync();

                foreach (var item in response)
                {
                    resultRepository.Insert(new Data.Models.Result
                    {
                        Result1 = item.numero + " " + item.nombre,
                        Time = item.loteria.Replace("Selva Plus ", "").ToUpper(),
                        Date = string.Empty,
                        ProductId = selvaPlusID,
                        ProviderId = selvaPlusProviderID,
                        ProductTypeId = (int)ProductTypeEnum.ANIMALITOS
                    });
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: nameof(SelvaPlusOfficial));
                throw;
            }
        }
    }
}
