namespace Blobstorage.Mock.Helpers;

public class WaitHelper
{
    public static async Task Wait(int? minimun, int? maximun)
    {
        if (minimun == null && maximun == null) return;

        int waitingTime;

        if (minimun == null) {
            waitingTime = Random.Shared.Next(0, maximun.Value);
        } else if ( maximun == null) {
            waitingTime = minimun.Value;
        } else {
            waitingTime = Random.Shared.Next(minimun.Value, maximun.Value);
        }

        await Task.Delay(waitingTime);
    }
}
