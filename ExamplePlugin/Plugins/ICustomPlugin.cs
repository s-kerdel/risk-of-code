namespace RiskOfCodePlugin.Plugins
{
    public interface ICustomPlugin
    {
        // The Awake() method is run at the very start when the game is initialized.
        void Awake();
        void Uninitialize();

        // Called every frame
        void Update();
    }
}