using Microsoft.AspNetCore.Components.Server.Circuits;

namespace WebClient.Services;

public class TrackingCircuitHandler : CircuitHandler
{
    private static int _onlineCount = 0;
    
        public static int OnlineCount => _onlineCount;
    
        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _onlineCount);
            return Task.CompletedTask;
        }
    
        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            Interlocked.Decrement(ref _onlineCount);
            return Task.CompletedTask;
        }
    
}