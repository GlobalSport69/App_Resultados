//using Hangfire;
//using LotteryResult.Data.Abstractions;
//using LotteryResult.Data.Models;
//using LotteryResult.Services.LoteriaDeHoy;

//namespace LotteryResult.Services
//{
//    public interface IBootstrapJobs
//    {
//        Task Init();
//    }
//    public class BootstrapJobs : IBootstrapJobs
//    {
//        private IProductRepository productRepository;

//        private TripleZamoranoOfficial _GetResultfromTripleZamorano;
//        private LoteriaDeHoy _GetResultFromLoteriaDeHoy;
//        public BootstrapJobs(IProductRepository productRepository, 
//            TripleZamoranoOfficial getResultfromTripleZamorano, 
//            LoteriaDeHoy getResultFromLoteriaDeHoy)
//        {
//            this.productRepository = productRepository;

//            _GetResultfromTripleZamorano = getResultfromTripleZamorano;
//            _GetResultFromLoteriaDeHoy = getResultFromLoteriaDeHoy;
//        }

//        public async Task Init()
//        {
//            try
//            {
//                var productsEnables = await productRepository.GetAllByAsync(x => x.Enable, 
//                new string[] {
//                    nameof(Product.ProviderProducts),
//                    $"{nameof(Product.ProviderProducts)}.{nameof(ProviderProduct.Provider)}"
//                });

//                foreach (var product in productsEnables)
//                {
//                    var pp = product.ProviderProducts.FirstOrDefault(x => x.Provider.Enable);
//                    if (pp != null)
//                    {
//                        RecurringJob.AddOrUpdate(pp.Provider.Name,
//                            () => Job(pp.Provider.Id).Handel(),
//                            pp.CronExpression);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//        }

//        private IGetResult Job(int provider_id) {

//            // Triple Zamonaro Oficial
//            if (provider_id == 1)
//            {
//                return _GetResultfromTripleZamorano;
//            }

//            // Triple Zamonaro Loteria de hoy
//            if (provider_id == 2)
//            {
//                return _GetResultFromLoteriaDeHoy;
//            }

//            throw new Exception("Provider not found");
//        }
//    }
//}
