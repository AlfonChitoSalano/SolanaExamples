using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PhantomWalletTest.Pages
{
    public partial class Index
    {
        [Inject] private IJSRuntime? JsRuntime { get; set; }
        private DotNetObjectReference<Index>? _objRef;
        private bool _isConnected;
        private string _publicAddress = "";

        [JSInvokable("OnConnected")]
        public void OnConnected(string publicKey)
        {
            _isConnected = true;
            _publicAddress = publicKey;
            StateHasChanged();
        }

        [JSInvokable("OnDisConnected")]
        public void OnDisConnected()
        {
            _isConnected = false;
            _publicAddress = "";
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            _objRef = DotNetObjectReference.Create(this);
        }

        private async Task ConnectAsync()
        {
            if (JsRuntime == null)
            {
                return;
            }

            await JsRuntime.InvokeVoidAsync("solanaPhantom.login", _objRef);
        }

        private async Task DisConnectAsync()
        {
            if (JsRuntime == null)
            {
                return;
            }

            await JsRuntime.InvokeVoidAsync("solanaPhantom.signOut", _objRef);
        }
    }
}