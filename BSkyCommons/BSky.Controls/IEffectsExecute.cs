
namespace BSky.Controls
{
    /// <summary>
    /// interface decides if a control can affect execution
    /// </summary>
    public interface IBSkyAffectsExecute
    {
        bool CanExecute { get; set; }
    }
}
