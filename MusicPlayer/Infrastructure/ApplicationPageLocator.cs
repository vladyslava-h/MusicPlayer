namespace MusicPlayer
{
    class ApplicationPageLocator
    {
       public static ApplicationPageLocator Instanse => new ApplicationPageLocator();
       public static ApplicationPageViewModel ApplicationPageViewModel => ServiceLocator.Get<ApplicationPageViewModel>();
    }
}
