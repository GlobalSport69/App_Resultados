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
        public async Task<IEnumerable<ResultsDto>> Get([FromQuery] string date = null)
        {
            try
            {
                DateTime venezuelaNow = DateTime.Now;
                if (!string.IsNullOrEmpty(date))
                {
                    venezuelaNow = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                var products = await unitOfWork.ProductRepository.GetResultByProductsByDate(venezuelaNow);

                return products.Select(p => new ResultsDto
                {
                    ProductName = p.Name,
                    Results = p.Results.Select(x => new ResultDetailDto { 
                        Result = x.Result1,
                        Time = x.Time,
                        Sorteo = x.Sorteo
                    }),
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
