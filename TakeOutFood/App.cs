namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            string ret = "============= Order details =============\n";
            var inputDict = new Dictionary<string, int>();
            var savingList = new List<string>();
            bool isUsingPromo = false;
            double total = 0.0;
            var itemDict = itemRepository.FindAll().ToDictionary(x=>x.Id, x=>x);
            var promoList = salesPromotionRepository.FindAll();
            var totalRelatedItems = promoList.SelectMany(x => x.RelatedItems).ToList();
            foreach (var item in inputs)
            {
                var parsedStrings = item.Split(" ");
                inputDict.Add(parsedStrings[0], Convert.ToInt32(parsedStrings[2]));
                double subtotal = itemDict[parsedStrings[0]].Price * Convert.ToInt32(parsedStrings[2]);
                total += subtotal;
                ret += $"{itemDict[parsedStrings[0]].Name} x {parsedStrings[2]} = {(int)subtotal} yuan\n";
                if (totalRelatedItems.Contains(parsedStrings[0])) isUsingPromo = true;
            }
            ret += "-----------------------------------\n";

            if (isUsingPromo)
            {
                ret += "Promotion used:\n";

                double saving = 0.0;
                string savingString = "";
                SalesPromotion selectedCoupon = promoList[0];
                foreach(var coupon in promoList)
                {
                    var discount = 0.0;
                    var discountList = coupon.RelatedItems.Where(x => inputDict.ContainsKey(x));
                    if (!discountList.Any()) 
                        continue;
                    var currentSaving = discountList.Select(x => itemDict[x].Price * inputDict[x] * 0.5).Sum();
                    if (currentSaving > saving)
                    {
                        saving = currentSaving;
                        selectedCoupon = coupon;
                    }
                }
                savingList = selectedCoupon.RelatedItems.Where(x =>inputDict.ContainsKey(x)).ToList();
                foreach (var id in savingList)
                {
                    if(savingList.IndexOf(id) == 0)
                    {
                        savingString += itemDict[id].Name;
                    }
                    else
                    {
                        savingString += ", " + itemDict[id].Name;
                    }
                }
                total -= saving;
                ret += $"{promoList[0].DisplayName} ({savingString}), saving {(int)saving} yuan\n";
                ret += "-----------------------------------\n";
            }

            ret += $"Total：{(int)total} yuan\n";
            ret += "===================================";
            return ret;
        }
    }
}
