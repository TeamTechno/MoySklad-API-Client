using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MoySklad.Client.Tests
{
    /// <summary>
    ///     Создает рабочий Environment МойСклад или очищает существующий.
    ///     Дополнитрельная инфомрация здесь:
    ///     https://support.moysklad.ru/hc/ru/articles/203404253-REST-%D1%81%D0%B5%D1%80%D0%B2%D0%B8%D1%81-%D1%81%D0%B8%D0%BD%D1%85%D1%80%D0%BE%D0%BD%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D0%B8-%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85
    ///     http://wiki.moysklad.ru/wiki/REST-%D1%81%D0%B5%D1%80%D0%B2%D0%B8%D1%81_%D1%81%D0%B8%D0%BD%D1%85%D1%80%D0%BE%D0%BD%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D0%B8_%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85
    /// </summary>
    public class MoySkladSetupCleanup
    {
        private const bool DoCleanUp = false;

        /// <summary>
        ///     Sets up environment:
        ///     1. If DoCleanUp set to true, cleans up main data entities: PaymentIn, cashOut, customerOrder, retailDemand, move,
        ///     supply, good, company
        /// </summary>
        [Fact]
        public async Task SetupEnvironmentAsync()
        {
            const string buyer = "ООО \"Покупатель\"";
            const string retailBuyer = "Розничный покупатель";
            const string seller = "ООО \"Поставщик\"";
            // TODO: Set username and password
            MoySkladClient.UserName = "";
            MoySkladClient.Password = "";
            if (DoCleanUp)
            {
                await MoySkladClient.RemoveAllEntitiesAsync<paymentIn>();
                await MoySkladClient.RemoveAllEntitiesAsync<cashOut>();
                await MoySkladClient.RemoveAllEntitiesAsync<demand>();
                await MoySkladClient.RemoveAllEntitiesAsync<customerOrder>();
                await MoySkladClient.RemoveAllEntitiesAsync<retailDemand>();
                await MoySkladClient.RemoveAllEntitiesAsync<move>();
                await MoySkladClient.RemoveAllEntitiesAsync<supply>();
                await MoySkladClient.RemoveAllEntitiesAsync<good>();
                var goodFolders = await MoySkladClient.GetMoySkladEntitiesAsync<goodFolder>();
                await
                    MoySkladClient.RemoveEntityListAsync<goodFolder>(
                        new IdCollection(goodFolders.Where(x => !string.IsNullOrEmpty(x.parentUuid)).Select(x => x.uuid)));
                await
                    MoySkladClient.RemoveEntityListAsync<goodFolder>(
                        new IdCollection(goodFolders.Where(x => string.IsNullOrEmpty(x.parentUuid)).Select(x => x.uuid)));
                var companies = await MoySkladClient.GetMoySkladEntitiesAsync<company>();
                await
                    MoySkladClient.RemoveEntityListAsync<company>(
                        new IdCollection(
                            companies.Where(x => x.name != buyer && x.name != retailBuyer && x.name != seller)
                                .Select(x => x.uuid)));
            }
            var uoms = await MoySkladClient.GetMoySkladEntitiesAsync<uom>();
            var sht = uoms.SingleOrDefault(x => x.name == "шт");
            if (sht == null)
                throw new InvalidOperationException("Отсутсвует единица измерения шт");
            good good;
            var goodRootFolder = await MoySkladClient.InsertUpdateEntityAsync(new goodFolder { name = "Электроника", code = "E" });
            var goodFolder = await MoySkladClient.InsertUpdateEntityAsync(new goodFolder { name = "Планшеты", code = "ET", parentUuid = goodRootFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "iPad Mini 16GB", code = "iPadMini16", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "iPad Air 16GB", code = "iPadAir16", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "iPad Air 64GB", code = "iPadAir64", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            goodFolder = await MoySkladClient.InsertUpdateEntityAsync(new goodFolder { name = "Телефоны", code = "EP", parentUuid = goodRootFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "iPhone 16GB", code = "iPhone16", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "iPhone 32GB", code = "iPhone32", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "iPhone 64GB", code = "iPhone64", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            goodFolder = await MoySkladClient.InsertUpdateEntityAsync(new goodFolder { name = "Ноутбуки", code = "EN", parentUuid = goodRootFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "MacBook Pro", code = "MBP", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "MacBook Air", code = "MBA", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
            good = await MoySkladClient.InsertUpdateEntityAsync(new good { name = "MacBrook Pro Retina", code = "MBPR", uomUuid = sht.uuid, parentUuid = goodFolder.uuid });
        }
    }
}