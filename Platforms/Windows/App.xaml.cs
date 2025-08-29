using Microsoft.UI.Xaml;
using System.Threading; // Adicionado para a classe Mutex

namespace TVPlayerMAUI.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        // Guarda a referência ao Mutex para que ele viva durante toda a execução do app
        private static Mutex? _mutex;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // --- INÍCIO DA LÓGICA DO MUTEX ---
            // O nome aqui deve ser EXATAMENTE o mesmo que está no seu script do Inno Setup
            const string appMutexName = "TVPlayerMAUI_Mutex";
            _mutex = new Mutex(true, appMutexName, out bool createdNew);

            if (!createdNew)
            {
                // Se o Mutex já existia, significa que outra instância do aplicativo já está rodando.
                // Simplesmente fechamos esta nova instância para evitar duplicatas e permitir
                // que o instalador detecte o processo em execução.
                this.Exit();
                return;
            }
            // --- FIM DA LÓGICA DO MUTEX ---
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}