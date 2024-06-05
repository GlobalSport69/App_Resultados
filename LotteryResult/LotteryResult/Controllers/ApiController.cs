using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace LotteryResult.Controllers
{
    [Route("api/results")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private IUnitOfWork unitOfWork;

        public ApiController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IEnumerable<ResultsDto>> Get([FromQuery] string fromDate = null, [FromQuery] string endDate = null)
        {
            try
            {
                DateTime venezuelaFromDate, venezuelaEndDate;
                venezuelaFromDate = venezuelaEndDate = DateTime.Now;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    venezuelaFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    venezuelaEndDate = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                var products = await unitOfWork.ProductRepository.GetResultByProductsByRangeDate(venezuelaFromDate, venezuelaEndDate);

                return products.Select(p => new ResultsDto
                {
                    ProductName = p.Name,
                    Results = p.Results.GroupBy(x => x.CreatedAt).OrderBy(x => x.Key).SelectMany(g => g.Select(x => new ResultDetailDto
                    {
                        Date = g.Key.ToString("dd-MM-yyyy"),
                        Result = x.Result1,
                        Time = x.Time,
                        Sorteo = x.Sorteo,
                        lottery = x.PremierId
                    })),
                });
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.Message, ex);
                throw;
            }
        }
    }
}
