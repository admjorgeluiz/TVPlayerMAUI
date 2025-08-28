namespace TVPlayerMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        // Esta é a forma moderna e recomendada de definir a janela principal
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}