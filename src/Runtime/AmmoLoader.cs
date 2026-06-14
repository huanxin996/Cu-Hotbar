namespace CasualtiesUnknown.Hotbar
{
    /// <summary>主手 GunScript 缺弹时从背包扫描匹配弹药并装填一发；每帧由控制器轮询调用。</summary>
    internal static class AmmoLoader
    {
        internal static bool TryAutoReloadHandGun(Body body)
        {
            if (body == null) return false;
            var hand = body.GetItem(body.handSlot);
            if (hand == null) return false;
            var gun = hand.GetComponent<GunScript>();
            if (gun == null) return false;

            bool needChamber = gun.racked && gun.roundInChamber == GunScript.RoundInChamber.None;
            bool needMag = !gun.hasMag && gun.feedType == GunScript.FeedType.Mag;
            bool needDirect = gun.feedType == GunScript.FeedType.Direct
                && gun.roundsInMag < gun.magCapacity;
            if (!needChamber && !needMag && !needDirect) return false;

            AmmoScript pick = null;
            foreach (var it in body.GetAllItemsThorough())
            {
                if (it == null || it == hand) continue;
                var a = it.GetComponent<AmmoScript>();
                if (a == null || a.ammoType != gun.ammoType) continue;
                if (needMag && a.itemType == AmmoScript.AmmoItemType.Magazine)
                {
                    if (pick == null || a.rounds > pick.rounds) pick = a;
                    if (pick != null && pick.rounds == pick.maxRounds) break;
                }
                else if ((needChamber || needDirect) && a.itemType == AmmoScript.AmmoItemType.Round)
                {
                    pick = a;
                    break;
                }
            }
            if (pick == null) return false;
            gun.LoadMag(pick);
            return true;
        }
    }
}
