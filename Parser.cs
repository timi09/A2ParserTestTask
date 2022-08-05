using A2ParserTestTask.Data;
using A2ParserTestTask.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace A2ParserTestTask
{
    enum ParseStatus
    {
        Stoped,
        Working,
        Sleeping
    }

    class Parser
    {
        public ParseStatus Status { get; private set; }

        private WoodDealsApi Api;

        private Database Database;
        
        private Thread ParseThread;

        private CancellationTokenSource CancellationTokenSource;

        private const int SleepMiliseconds = 600000;

        private const int PageSize = 1000;

        private const int MinNameLen = 3;

        private const double MinVolumeDiff = 0.5;

        public Parser(Database database) 
        {
            Api = new WoodDealsApi();
            Database = database;
        }

        public void Start() 
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = CancellationTokenSource.Token;

            ParseThread = new Thread(() => { Parse(token); });
            ParseThread.Start(); 
        }

        public void Stop() 
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();

            if (Status == ParseStatus.Sleeping)
                ParseThread.Abort();
        }

        private async void Parse(CancellationToken token) 
        {
            while (!token.IsCancellationRequested) 
            {
                Status = ParseStatus.Working;

                DealsInfo info = await Api.GetDealsInfo();

                int pagesCount = (int)Math.Ceiling((double)info.total / PageSize);

                for (int i = 0; i < pagesCount && !token.IsCancellationRequested; i++)
                {
                    List<Deal> deals = await Api.GetDealsPage(PageSize, i);
                    List<DealModel> models = ConvertToModels(deals);
                    CheckRange(models);
                }

                Status = ParseStatus.Sleeping;

                Thread.Sleep(SleepMiliseconds);
            }

            Status = ParseStatus.Stoped;
        }

        private List<DealModel> ConvertToModels(List<Deal> deals) 
        {
            List<DealModel> models = new List<DealModel>();
            foreach (var deal in deals)
            {
                ContractorModel buyerModel = new ContractorModel
                {
                    Name = deal.buyerName,
                    INN = deal.buyerInn
                };

                ContractorModel sellerModel = new ContractorModel()
                {
                    Name = deal.sellerName,
                    INN = deal.sellerInn
                };

                DealModel dealModel = new DealModel()
                {
                    Number = deal.dealNumber,
                    Date = deal.dealDate,
                    VolumeBuyer = deal.woodVolumeBuyer,
                    VolumeSeller = deal.woodVolumeSeller,
                    Buyer = buyerModel,
                    Seller = sellerModel
                };

                models.Add(dealModel);
            }

            return models;
        }

        private void CheckRange(List<DealModel> deals)
        {
            foreach (var deal in deals)
            {
                if (ValidContractor(deal.Buyer) && ValidContractor(deal.Seller) && ValidDeal(deal))
                {
                    var findedDeal = Database.ReadDeal(deal);

                    if (findedDeal == null) 
                    {
                        deal.BuyerId = UpdateContractor(deal.Buyer);
                        deal.SellerId = UpdateContractor(deal.Seller);
                        Database.CreateDeal(deal);
                    }
                    else if (Math.Abs(findedDeal.VolumeBuyer - deal.VolumeBuyer) > MinVolumeDiff || Math.Abs(findedDeal.VolumeSeller - deal.VolumeSeller) > MinVolumeDiff)
                    {
                        Database.UpdateDeal(findedDeal.Id, deal);
                    }
                }
            }
        }

        private bool ValidContractor(ContractorModel contractor) 
        {
            if (contractor.Name != null && contractor.INN != null 
                && contractor.Name.Length > MinNameLen && (IsValidINN(contractor.INN) || (contractor.Name == "Физическое лицо" && contractor.INN == "")))
                return true;
            return false;
        }

        private bool IsValidINN(string str)
        {
            foreach (char c in str)
                if (c < '0' || c > '9')
                    return false;
            return str.Length >= 10 && str.Length <= 12;
        }

        private bool ValidDeal(DealModel deal)
        {
            if (deal.Number == null || deal.Date == null)
                return false;

            DateTime Date = DateTime.ParseExact(deal.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            if (Date.Year >= 1994 && Date <= DateTime.Now)
                return true;
            return false;
        }

        private int UpdateContractor(ContractorModel contractor)
        {
            var findedContractor = Database.ReadСontractor(contractor);

            if (findedContractor == null)
                return Database.CreateСontractor(contractor);

            if (contractor.Name.Length > findedContractor.Name.Length)
                Database.UpdateContractor(findedContractor.Id, contractor);

            return findedContractor.Id;
        }
    }
}
