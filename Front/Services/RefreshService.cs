namespace AutoDiffusion.Services
{
    public class RefreshService
    {
        public event Action OnRefreshRequested;

        public void RequestRefresh()
        {
            OnRefreshRequested?.Invoke();
        }
    }
}
